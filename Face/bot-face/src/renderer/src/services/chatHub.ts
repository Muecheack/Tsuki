import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel
} from '@microsoft/signalr'
import { API_BASE } from '../../../server/api/http'

// codex: 后端当前发送 receivedMessage(aiName, message)。
export type ChatHubMessageHandler = (aiName: string, message: string) => void

let connection: HubConnection | null = null
let registeredUserId: number | null = null
let messageHandler: ChatHubMessageHandler | null = null
let startPromise: Promise<void> | null = null

async function registerCurrentUser(): Promise<void> {
  if (!connection || connection.state !== HubConnectionState.Connected || registeredUserId === null) {
    return
  }

  await connection.invoke('RegisterUser', registeredUserId)
}

function createConnection(): HubConnection {
  const hub = new HubConnectionBuilder()
    // codex: 后端 CORS 使用 AllowAnyOrigin，浏览器凭据请求不接受 *，本地版关闭 credentials。
    .withUrl(`${API_BASE}/api/chatHub`, { withCredentials: false })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Information)
    .build()

  // codex: 监听只注册一次，避免每次页面初始化时重复追加消息。
  hub.on('receivedMessage', (aiName: string, message: string) => {
    messageHandler?.(aiName, message)
  })

  // codex: 重连后的 ConnectionId 会变化，必须重新加入用户 Group。
  hub.onreconnected(async () => {
    try {
      await registerCurrentUser()
    } catch (error) {
      console.error('[SignalR] 重连后重新入组失败:', error)
    }
  })

  hub.onclose((error) => {
    console.error('[SignalR] 连接已关闭:', error)
  })

  return hub
}

// codex: App 根组件只调用这一个入口；userId 来自注册接口返回的数据库 Users.Id。
export async function connectChatHub(
  userId: number,
  onMessage: ChatHubMessageHandler
): Promise<void> {
  if (!Number.isInteger(userId) || userId <= 0) {
    console.warn('[SignalR] userId 无效，跳过连接:', userId)
    return
  }

  registeredUserId = userId
  messageHandler = onMessage
  connection ??= createConnection()

  if (connection.state === HubConnectionState.Connected) {
    await registerCurrentUser()
    return
  }

  if (
    connection.state === HubConnectionState.Connecting ||
    connection.state === HubConnectionState.Reconnecting
  ) {
    return
  }

  if (!startPromise) {
    startPromise = connection
      .start()
      .then(registerCurrentUser)
      .finally(() => {
        startPromise = null
      })
  }

  await startPromise
}
