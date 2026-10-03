import { ElectronAPI } from '@electron-toolkit/preload'

// 注：这个文件千万别改名成 index.d.ts —— 同目录下存在 index.ts 时，
// TypeScript 会把 index.d.ts 当成"index.ts 编译产物的声明文件"直接跳过，
// declare global 就永远不生效（表现：tsc 报 Property 'electron' does not exist on type 'Window'，
// 而 npm run build = typecheck && electron-vite build，于是整个打包被卡在第一步）
declare global {
  interface Window {
    electron: ElectronAPI
    api: {
      selectFile: () => Promise<string[]>
    }
  }
}
