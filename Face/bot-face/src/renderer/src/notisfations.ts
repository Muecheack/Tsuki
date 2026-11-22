// 有新红点时候调用, 展示头像, ai名和新消息
export function sendNotisfy(
  title: string | undefined,
  body: string | undefined,
  avatarUrl: string | undefined
): void {
  // codex: renderer 只能通过 preload 暴露的桥接 API 通知主进程，不能直接导入 electron。
  window.api.sendNotify({
    title,
    body,
    avatarUrl
  })
}
