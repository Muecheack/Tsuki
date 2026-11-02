import { execFileSync, spawn, type ChildProcess } from 'node:child_process'
import {
  createWriteStream,
  existsSync,
  mkdirSync,
  readFileSync,
  renameSync,
  rmSync,
  statSync,
  writeFileSync,
  type WriteStream
} from 'node:fs'
import { createServer } from 'node:net'
import { join } from 'node:path'

/**
 * 桌面壳的"服务看门狗"。
 *
 * 职责：挑空闲端口 → 拉起两个 sidecar（.NET 后端 + AI 服务）→ 等它们就绪 → 退出时收干净。
 * 刻意不 import electron：这里的逻辑只跟进程和文件打交道，方便脱离 GUI 单独验证。
 *
 * 端口策略：由这里挑好再传给子进程（后端 `--urls`、AI `PORT`）。
 * 没让 C# 自己 bind 端口 0 再回报，是因为"回报"要多一条通道（解析 stdout 或写端口文件），
 * 多一个会出错的地方；这里 bind(0) 拿号再关掉，窗口期只有毫秒级，收益更大。
 */

/** 后端 exe 的候选名：以后把 AssemblyName 改成 Tsuki.Api 不用改这里 */
const API_EXE_CANDIDATES = ['Tsuki.Api.exe', 'cs.exe']
/** AI 服务 exe（PyInstaller 产物，见 pyai/TsukiAi.spec） */
const AI_EXE = 'TsukiAi.exe'
/** 用于判断"残留进程还是不是我们的"，避免 PID 被系统复用时误杀 */
const OUR_PROCESS_NAMES = ['tsuki.api.exe', 'cs.exe', 'tsukiai.exe']

export interface UserPaths {
  /** 用户数据：KeKe.db 就放这，升级/卸载都不碰 */
  data: string
  /** 两个子进程的 stdout/stderr + supervisor 自己的日志 */
  logs: string
  /** 运行期状态（pids.json），用来清理上次留下的孤儿进程 */
  run: string
}

export interface StartOptions {
  /** 打包后是 process.resourcesPath；开发期指向仓库根的 build/ */
  resRoot: string
  /** app.getPath('userData') → %APPDATA%\Tsuki */
  userData: string
}

export interface RunningServices {
  /** 形如 http://127.0.0.1:51234，要传给渲染进程 */
  apiBase: string
  aiBase: string
  /** 幂等；退出时调用 */
  stop: () => void
}

export function userPaths(userData: string): UserPaths {
  return {
    data: join(userData, 'data'),
    logs: join(userData, 'logs'),
    run: join(userData, 'run')
  }
}

export function ensureDirs(paths: UserPaths): void {
  for (const dir of Object.values(paths)) mkdirSync(dir, { recursive: true })
}

/**
 * 让系统分配一个空闲端口：bind(0) 拿到号之后立刻 close，把这个号交给子进程。
 * 严格说存在"关掉到子进程 bind"之间的竞态，但窗口是毫秒级，且失败会走就绪超时那条路
 * （弹错误框 + 日志目录），不会白屏。
 */
export function getFreePort(): Promise<number> {
  return new Promise((resolve, reject) => {
    const srv = createServer()
    srv.once('error', reject)
    srv.listen(0, '127.0.0.1', () => {
      const address = srv.address()
      const port = typeof address === 'object' && address ? address.port : 0
      srv.close(() => (port ? resolve(port) : reject(new Error('拿不到空闲端口'))))
    })
  })
}

/** 逐个探测候选 exe 名 */
function findExe(dir: string, candidates: string[]): string | null {
  for (const name of candidates) {
    const full = join(dir, name)
    if (existsSync(full)) return full
  }
  return null
}

/** 日志文件超过 5MB 就滚一份，避免长期运行无限增长 */
function rotateIfBig(file: string): void {
  try {
    if (existsSync(file) && statSync(file).size > 5 * 1024 * 1024) {
      renameSync(file, `${file}.1`)
    }
  } catch {
    /* 滚不动就算了，日志不该影响启动 */
  }
}

function openLog(file: string): WriteStream {
  rotateIfBig(file)
  return createWriteStream(file, { flags: 'a' })
}

function pipeChildLog(tag: string, child: ChildProcess, logsDir: string): void {
  const stream = openLog(join(logsDir, `${tag}.log`))
  const write = (chunk: Buffer): void => {
    stream.write(`[${new Date().toISOString()}] ${chunk.toString()}`)
  }
  child.stdout?.on('data', write)
  child.stderr?.on('data', write)
}

/** Windows 上必须整棵树杀，否则 cs.exe 可能留下子进程 */
export function killTree(child?: ChildProcess): void {
  const pid = child?.pid
  if (!pid) return
  try {
    if (process.platform === 'win32') {
      spawn('taskkill', ['/PID', String(pid), '/T', '/F'], { windowsHide: true })
    } else {
      child.kill('SIGTERM')
    }
  } catch {
    /* 进程可能已经没了 */
  }
}

function killPid(pid: number): void {
  try {
    if (process.platform === 'win32') {
      spawn('taskkill', ['/PID', String(pid), '/T', '/F'], { windowsHide: true })
    } else {
      process.kill(pid, 'SIGTERM')
    }
  } catch {
    /* 忽略 */
  }
}

/** 这个 pid 现在还活着、而且确实是我们的进程吗（防 PID 复用误杀） */
function isOurProcess(pid: number): boolean {
  if (process.platform !== 'win32') {
    try {
      process.kill(pid, 0)
      return true
    } catch {
      return false
    }
  }
  try {
    const out = execFileSync(
      'tasklist',
      ['/FI', `PID eq ${pid}`, '/FO', 'CSV', '/NH'],
      { encoding: 'utf8', windowsHide: true }
    )
    const firstLine = (out.trim().split(/\r?\n/)[0] ?? '').toLowerCase()
    return OUR_PROCESS_NAMES.some((name) => firstLine.includes(name))
  } catch {
    return false
  }
}

/**
 * 清理上次没退干净的 sidecar。
 * 用户可能用任务管理器强杀 Electron，这时 before-quit 不会执行，子进程就成了孤儿。
 */
function cleanupStaleProcesses(runDir: string, log: (msg: string) => void): void {
  const file = join(runDir, 'pids.json')
  if (!existsSync(file)) return
  try {
    const parsed = JSON.parse(readFileSync(file, 'utf8')) as { api?: number; ai?: number }
    for (const [label, pid] of [
      ['api', parsed.api],
      ['ai', parsed.ai]
    ] as const) {
      if (!pid) continue
      if (isOurProcess(pid)) {
        log(`清理上次残留的 ${label} 进程 pid=${pid}`)
        killPid(pid)
      }
    }
  } catch (e) {
    log(`读 pids.json 失败（忽略）: ${String(e)}`)
  }
  rmSync(file, { force: true })
}

function sleep(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms))
}

/**
 * 等某个服务起来：只要有 HTTP 响应就算活（404/405 也算，说明管道已经通了）。
 * 同时盯着子进程，它要是先死了就立刻失败，不用干等 20 秒。
 */
async function waitReady(url: string, child: ChildProcess, timeoutMs = 20000): Promise<void> {
  const deadline = Date.now() + timeoutMs
  let exited = false
  child.once('exit', () => {
    exited = true
  })

  while (Date.now() < deadline) {
    if (exited) throw new Error(`子进程提前退出：${child.spawnfile}`)
    try {
      await fetch(url, { signal: AbortSignal.timeout(1500) })
      return
    } catch {
      await sleep(200)
    }
  }
  throw new Error(`等待就绪超时：${url}`)
}

/**
 * 拉起两个服务并等就绪。失败时会先把已经起来的子进程收掉再抛，不留半套。
 */
export async function startServices(options: StartOptions): Promise<RunningServices> {
  const paths = userPaths(options.userData)
  ensureDirs(paths)

  const logStream = openLog(join(paths.logs, 'supervisor.log'))
  const log = (msg: string): void => {
    logStream.write(`[${new Date().toISOString()}] ${msg}\n`)
  }

  try {
    cleanupStaleProcesses(paths.run, log)

    const backendDir = join(options.resRoot, 'backend')
    const aiDir = join(options.resRoot, 'ai')

    const apiExe = findExe(backendDir, API_EXE_CANDIDATES)
    if (!apiExe) {
      throw new Error(`找不到后端 exe，期望在 ${backendDir} 里（${API_EXE_CANDIDATES.join(' / ')}）`)
    }
    const aiExe = join(aiDir, AI_EXE)
    if (!existsSync(aiExe)) {
      throw new Error(`找不到 AI 服务 exe，期望在 ${aiExe}`)
    }

    const [apiPort, aiPort] = await Promise.all([getFreePort(), getFreePort()])
    const apiBase = `http://127.0.0.1:${apiPort}`
    const aiBase = `http://127.0.0.1:${aiPort}`
    log(`端口分配完成 api=${apiPort} ai=${aiPort}`)

    // ── .NET 后端 ──
    // 每一条 env 都不能少：库路径漏了会把数据写进安装目录，Address__Pyai 漏了后端会去找默认 10086。
    const api = spawn(apiExe, ['--urls', apiBase], {
      cwd: backendDir,
      windowsHide: true,
      stdio: ['ignore', 'pipe', 'pipe'],
      env: {
        ...process.env,
        ASPNETCORE_ENVIRONMENT: 'Production',
        ConnectionStrings__DefaultConnection: `Data Source=${join(paths.data, 'KeKe.db')};Default Timeout=30;Foreign Keys=True`,
        Address__Pyai: aiBase
      }
    })
    pipeChildLog('api', api, paths.logs)

    // ── AI 服务（Python 打出来的 exe）──
    // cwd 设成 userData：它会把 prmp/*.txt 写在"当前工作目录"，
    // 这样提示词落在用户目录，安装目录保持只读也能跑。
    const ai = spawn(aiExe, [], {
      cwd: options.userData,
      windowsHide: true,
      stdio: ['ignore', 'pipe', 'pipe'],
      env: { ...process.env, PORT: String(aiPort) }
    })
    pipeChildLog('ai', ai, paths.logs)

    writeFileSync(join(paths.run, 'pids.json'), JSON.stringify({ api: api.pid, ai: ai.pid }))

    let stopped = false
    const stop = (): void => {
      if (stopped) return
      stopped = true
      killTree(api)
      killTree(ai)
      rmSync(join(paths.run, 'pids.json'), { force: true })
      log('服务已停止')
    }

    try {
      // 并行等，否则启动时间会叠加（后端约 1s、AI 约 1.2s）
      await Promise.all([waitReady(`${apiBase}/`, api), waitReady(`${aiBase}/`, ai)])
    } catch (e) {
      stop()
      throw e
    }

    log('两个服务均已就绪')
    return { apiBase, aiBase, stop }
  } catch (e) {
    log(`启动失败: ${String(e)}`)
    logStream.end()
    throw e
  }
}
