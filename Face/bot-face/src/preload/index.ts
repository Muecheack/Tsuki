import { contextBridge, ipcRenderer } from 'electron'
import { electronAPI } from '@electron-toolkit/preload'

// 后端基址由主进程创建窗口时用 additionalArguments 传进来（打包后端口是动态的）。
// 注意两点：
//   1) preload 跑在隔离世界里，看不到主进程的变量，所以地址只能从 process.argv 里读；
//   2) 隔离世界里也拿不到上下文桥暴露出去的 window.electron，所以 ipcRenderer 要直接从 'electron' import
//      （之前写成 window.Electron 是拿不到的，只是那个方法没人调所以没炸）。
const API_BASE_ARG = '--api-base='

function readApiBase(): string {
  const hit = process.argv.find((arg) => arg.startsWith(API_BASE_ARG))
  return hit ? hit.slice(API_BASE_ARG.length) : 'http://localhost:5108'
}

// Custom APIs for renderer
const api = {
  // 打开文件选择器，返回选中文件的路径数组
  selectFile: (): Promise<string[]> => ipcRenderer.invoke('select-file'),
  // 渲染进程用它拼后端地址，不要在 http.ts 里写死端口
  apiBase: readApiBase()
}

// Use `contextBridge` APIs to expose Electron APIs to
// renderer only if context isolation is enabled, otherwise
// just add to the DOM global.
if (process.contextIsolated) {
  try {
    contextBridge.exposeInMainWorld('electron', electronAPI)
    contextBridge.exposeInMainWorld('api', api)
  } catch (error) {
    console.error(error)
  }
} else {
  // @ts-ignore (define in dts)
  window.electron = electronAPI
  // @ts-ignore (define in dts)
  window.api = api
}
