// 文件相关的小工具：文本白名单、体积格式化、截断预览
// 这里的白名单同时也是 ChatPage 里 <input accept> 的过滤条件（改一处就够）

// 会被读成文本、截断后作为"文件预览"发给 AI 的扩展名
// （就是原来写死在 ChatPage 的 accept 里那一串，挪过来集中管理）
export const TEXT_FILE_EXTENSIONS = [
  'txt', 'text', 'log', 'md', 'markdown', 'rst', 'adoc', 'asciidoc', 'tex', 'latex',
  'csv', 'tsv', 'json', 'jsonc', 'xml', 'yaml', 'yml', 'toml', 'ini', 'cfg', 'conf',
  'config', 'properties', 'env', 'gitignore', 'dockerignore', 'npmrc', 'babelrc',
  'eslintrc', 'prettierrc',
  'cs', 'java', 'js', 'mjs', 'cjs', 'ts', 'py', 'cpp', 'c', 'h', 'php', 'rb', 'go',
  'rs', 'swift', 'kt', 'dart', 'scala', 'pl', 'lua', 'r', 'm', 'sql',
  'sh', 'bash', 'ps1', 'html', 'htm', 'css', 'scss', 'sass', 'less', 'vue', 'jsx',
  'tsx', 'svg', 'dat', 'out'
] as const

// 喂给 <input accept>：只按扩展名过滤，用户在系统对话框里仍可切到"所有文件"
export const TEXT_FILE_ACCEPT = TEXT_FILE_EXTENSIONS.map((ext) => `.${ext}`).join(',')

// 单份预览的字符上限：头尾各留一半，中间省略
// 这个值直接决定每次聊天往模型上下文里塞多少东西（历史会被反复重发），别调太大
export const FILE_PREVIEW_LIMIT = 6000

// 超过这个体积就不读预览了（file.text() 会把整个文件读进渲染进程内存）
const PREVIEW_MAX_BYTES = 5 * 1024 * 1024

const TEXT_EXTENSION_SET: ReadonlySet<string> = new Set<string>(TEXT_FILE_EXTENSIONS)

// 是不是文本类文件：先看扩展名，再看浏览器给的 MIME（有些系统给 .md/.json 带的是 text/*）
export function isTextFile(file: File): boolean {
  const name = file.name.toLowerCase()
  const dot = name.lastIndexOf('.')
  if (dot >= 0 && TEXT_EXTENSION_SET.has(name.slice(dot + 1))) return true
  return file.type.startsWith('text/')
}

// 体积显示：和原来一样，1MB 以上按 MB，否则按 KB（都保留一位小数）
export function formatFileSize(bytes: number): string {
  if (bytes > 1024 * 1024) return (bytes / 1024 / 1024).toFixed(1) + ' MB'
  return (bytes / 1024).toFixed(1) + ' KB'
}

// 读出文本预览：非文本文件、读失败、内容是二进制、太大 —— 一律返回 undefined（只发文件元信息）
// 超长时保留头尾各一半，中间标出省略了多少字符，避免整个文件污染后续每一轮对话
export async function readTextPreview(file: File): Promise<string | undefined> {
  if (!isTextFile(file)) return undefined
  if (file.size > PREVIEW_MAX_BYTES) return undefined

  let text: string
  try {
    text = await file.text()
  } catch (err) {
    console.error('读取文件内容失败:', err)
    return undefined
  }

  // 带 NUL 字符的基本是二进制（扩展名骗人/无扩展名），别硬当文本发
  if (text.includes('\u0000')) return undefined

  const trimmed = text.trim()
  if (!trimmed) return undefined
  if (trimmed.length <= FILE_PREVIEW_LIMIT) return trimmed

  const half = Math.floor(FILE_PREVIEW_LIMIT / 2)
  const omitted = trimmed.length - FILE_PREVIEW_LIMIT
  return `${trimmed.slice(0, half)}\n……（此处省略 ${omitted} 个字符）……\n${trimmed.slice(-half)}`
}
