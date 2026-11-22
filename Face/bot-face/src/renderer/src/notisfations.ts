import { ipcRenderer } from 'electron'

// 有新红点时候调用, 展示头像, ai名和新消息
export function sendNotisfy(
  title: string | undefined,
  body: string | undefined,
  avatarUrl: string | undefined
) {
  ipcRenderer.send('show-notify', {
    title,
    body,
    avatarUrl
  })
}
