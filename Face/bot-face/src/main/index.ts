import { app, shell, BrowserWindow, ipcMain, dialog } from 'electron'
import { join } from 'path'
import { electronApp, optimizer, is } from '@electron-toolkit/utils'
// Linux 下的窗口图标。文件名要跟 resources\ 里的实际文件一致（不是模板默认的 icon.png），
// 换图标时保持这个名字或同步改这行，否则 vite 构建会报"找不到模块"。
import icon from '../../resources/tsuki_ic_pn.png?asset'
import { startServices, userPaths, type RunningServices } from './services'

/**
 * 要不要由桌面壳自己拉起后端和 AI 服务。
 *
 * - 打包后（用户装的版本）：一定要拉，用户机器上没有开发环境。
 * - 开发时（npm run dev）：默认不拉，沿用你手动 `dotnet run` + `python app.py` 的工作方式，
 *   否则每次调界面都得先把服务停掉。
 * - 想验证 supervisor 逻辑：`$env:TSUKI_SPAWN='1'; npm run dev`
 */
const RUN_SIDECAR = app.isPackaged || process.env['TSUKI_SPAWN'] === '1'

/** 开发期后端的默认地址（launchSettings 里的 5108） */
const DEV_API_BASE = process.env['TSUKI_API_BASE'] ?? 'http://localhost:5108'

/** 当前后端基址：打包后由 supervisor 动态端口决定，开发期是上面的默认值 */
let apiBase = DEV_API_BASE

let services: RunningServices | undefined
let cleanedUp = false

/** 打包后 = process.resourcesPath；开发期指回仓库根的 build/（dotnet publish 的暂存目录） */
function resolveResRoot(): string {
  // 调试用：把 backend/ 和 ai/ 摆在任意位置，指过来就能跑，不必先打包
  const override = process.env['TSUKI_RES_ROOT']
  if (override) return override
  if (app.isPackaged) return process.resourcesPath
  return join(app.getAppPath(), '..', '..', 'build')
}

function createWindow(): void {
  // Create the browser window.
  const mainWindow = new BrowserWindow({
    width: 900,
    height: 570,
    minWidth: 900,
    minHeight: 570,
    show: false,
    autoHideMenuBar: true,
    // 窗口图标只在 Linux 用到；Windows/macOS 的图标由 electron-builder 写进 exe，
    // 安装器图标由 Inno 从 installer\Tsuki.iss 的 SetupIconFile 带。
    ...(process.platform === 'linux' ? { icon } : {}),
    webPreferences: {
      preload: join(__dirname, '../preload/index.js'),
      sandbox: false,
      contextIsolation: true,
      // 渲染进程看不到主进程的变量，地址得这样传过去：main → 启动参数 → preload → window.api.apiBase
      additionalArguments: [`--api-base=${apiBase}`]
    }
  })

  mainWindow.on('ready-to-show', () => {
    mainWindow.show()
  })

  mainWindow.webContents.setWindowOpenHandler((details) => {
    shell.openExternal(details.url)
    return { action: 'deny' }
  })

  // HMR for renderer base on electron-vite cli.
  // Load the remote URL for development or the local html file for production.
  if (is.dev && process.env['ELECTRON_RENDERER_URL']) {
    mainWindow.loadURL(process.env['ELECTRON_RENDERER_URL'])
  } else {
    mainWindow.loadFile(join(__dirname, '../renderer/index.html'))
  }
}

/** 收掉两个子进程。多处事件都会调，靠 cleanedUp 保证幂等。 */
function cleanup(): void {
  if (cleanedUp) return
  cleanedUp = true
  services?.stop()
}

async function bootstrap(): Promise<void> {
  // Set app user model id for windows
  electronApp.setAppUserModelId('com.muecheack.tsuki')

  // Default open or close DevTools by F12 in development
  // and ignore CommandOrControl + R in production.
  // see https://github.com/alex8088/electron-toolkit/tree/master/packages/utils
  app.on('browser-window-created', (_, window) => {
    optimizer.watchWindowShortcuts(window)
  })

  // 打开资源管理器相关
  ipcMain.handle('select-file', async () => {
    const result = await dialog.showOpenDialog({
      properties: ['openFile'],
      filters: [
        { name: '图片', extensions: ['jpg', 'png', 'webp'] },
        { name: '所有文件', extensions: ['*'] }
      ]
    })
    return result.filePaths
  })

  if (RUN_SIDECAR) {
    try {
      services = await startServices({
        resRoot: resolveResRoot(),
        userData: app.getPath('userData')
      })
      apiBase = services.apiBase
    } catch (e) {
      const logs = userPaths(app.getPath('userData')).logs
      dialog.showErrorBox(
        'Tsuki 启动失败',
        `${e instanceof Error ? e.message : String(e)}\n\n日志目录：${logs}`
      )
      app.quit()
      return
    }
  }

  createWindow()

  app.on('activate', function () {
    // On macOS it's common to re-create a window in the app when the
    // dock icon is clicked and there are no other windows open.
    if (BrowserWindow.getAllWindows().length === 0) createWindow()
  })
}

// ── 单实例锁 ──
// 不做的话双击两次会有两套 sidecar 抢 SQLite 写锁和端口。
if (!app.requestSingleInstanceLock()) {
  app.quit()
} else {
  app.on('second-instance', () => {
    // 第二下双击：把已有窗口拉到前台
    const win = BrowserWindow.getAllWindows()[0]
    if (win) {
      if (win.isMinimized()) win.restore()
      win.focus()
    }
  })

  // This method will be called when Electron has finished
  // initialization and is ready to create browser windows.
  app.whenReady().then(bootstrap)
}

// Quit when all windows are closed, except on macOS. There, it's common
// for applications and its menu bar to stay active until the user quits
// explicitly with Cmd + Q.
app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit()
  }
})

// 三条退出路径都挂上：正常退出、窗口全关、进程被要求退出
app.on('before-quit', cleanup)
app.on('will-quit', cleanup)
process.on('exit', cleanup)
