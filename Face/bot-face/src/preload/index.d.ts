import { ElectronAPI } from '@electron-toolkit/preload'

export interface Api {
  /** 打开文件选择器，返回选中文件的路径数组 */
  selectFile: () => Promise<string[]>
  /** 后端基址（由主进程注入，形如 http://127.0.0.1:51234） */
  apiBase: string
  /** 请求主进程显示系统通知 */
  // codex: 补齐 preload 暴露给 renderer 的通知 API 类型。
  sendNotify: (payload: {
    title?: string
    body?: string
    avatarUrl?: string
  }) => void
}

declare global {
  interface Window {
    electron: ElectronAPI
    api: Api
  }
}
