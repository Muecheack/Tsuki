import axios from 'axios'

// 后端基址：由 Electron 主进程在启动时通过 preload 注入（打包后端口是动态的，不能写死）。
// 不在 Electron 里跑（比如直接用浏览器打开调试页）时回退到开发用的默认地址。
const API_BASE = window.api?.apiBase ?? 'http://localhost:5108'

// 后端返回的业务错误（携带 code，便于前端区分"用户已存在"等场景）
export class ApiError extends Error {
  constructor(
    public code: number,
    message: string
  ) {
    super(message)
  }
}

// 通用后端返回格式
export interface ApiReply<T> {
  code: number
  message: string
  data: T
}

// 注册接口返回结构（与后端约定：{ code, message, data: { id, token } }）
// 注：后端是 C# 匿名对象 new { saved.Id, token }，ASP.NET Core 默认 camelCase 序列化 → "id"
export interface RegisterData {
  id?: string
  token?: string
}

// 聊天接口返回结构（与后端约定：{ code, message, data: { sessionId, reply } }）
export interface ChatData {
  sessionId?: number
  message?: string
}

// 预人设接口
export interface Selected_PrePerson {
  tags: string[]
  avatarUrl: string
}
export interface PrePerson {
  reply: string
  selected: Selected_PrePerson
}

// 申请好友接口
export interface ApplyFirend {
  sessionId: number
}

// 文件发送接口
// 后端 FileController.ReceivedFile 用 [FromForm] FileDto 接：字段名 aiName / userName / sessionId / file
export interface UploadFile {
  code?: number
  message?: string
}

export type RegisterReply = ApiReply<RegisterData>
export type ChatReply = ApiReply<ChatData>
export type PrepersonReply = ApiReply<PrePerson>
export type ApplyFriendReply = ApiReply<ApplyFirend>
export type UploadFileReply = ApiReply<UploadFile>

// 前端把名字和人设和头像名给ai发过去
// 发送注册信息，成功返回 { id, token }（token 是免登录凭证，id 是新用户 Id）
// 失败统一抛 ApiError（带后端 code）或网络异常，不在内部吞错
export async function sendUserInfoAsync(
  name: string,
  gender: string,
  call: string,
  apiKey: string
): Promise<RegisterReply['data']> {
  try {
    const reply = await axios.post<RegisterReply>(`${API_BASE}/api/Auth/register`, {
      name,
      gender,
      call,
      apiKey
    })
    // 后端成功约定 code = 200
    if (reply.data.code !== 200) {
      console.log(`回消息失败:${reply.data.message}`)
      throw new ApiError(reply.data.code, reply.data.message)
    }
    return reply.data.data
  } catch (err) {
    // HTTP 非 2xx（如 400 用户已存在）：axios 会抛 AxiosError，把后端返回体转成 ApiError
    if (axios.isAxiosError(err)) {
      const data = err.response?.data as RegisterReply | undefined
      if (data && typeof data.code === 'number') {
        throw new ApiError(data.code, data.message)
      }
    }
    throw err // 网络错误、后端没起等原样抛
  }
}

// 发消息的接口
export async function sendChatMessageAsync(
  aiName: string,
  userName: string,
  message: string,
  type: 'text' | 'file' = 'text'
) {
  // 查本地有没有sessionId
  const sessionId = localStorage.getItem(`sessionId_${aiName}`)
  try {
    const reply = await axios.post<ChatReply>(`${API_BASE}/api/Chat/chat1`, {
      userName,
      sessionId: sessionId ? Number(sessionId) : null,
      type,
      message
    })

    const replyMessage = reply.data.data.message
    const replySessionId = reply.data.data.sessionId as any

    console.log(`收到回复:${reply.data.data.message}`)

    if (reply.data.code !== 200) {
      console.log(`回消息失败:${reply.data.message}`)
      throw new ApiError(reply.data.code, reply.data.message)
    }
    if (!sessionId) {
      localStorage.setItem(`sessionId_${aiName}`, replySessionId?.toString())
      return { replyMessage, replySessionId }
    }
    return { replyMessage, replySessionId }
  } catch (err) {
    // HTTP 非 2xx（axios 抛 AxiosError，且后端返回体有 code）→ 转成带 code 的 ApiError
    if (axios.isAxiosError(err)) {
      const data = err.response?.data as ChatReply | undefined
      if (data && typeof data.code === 'number') {
        throw new ApiError(data.code, data.message)
      }
    }
    throw err
  }
}

// 预发人设
// 不出意外这个tags只有guide那次用上, 其它都用不上
// 返回[aiReply, selectedTags, avatarUrl]
export async function preAiPerson(
  userName: string,
  aiName: string
): Promise<[string, string[], string]> {
  try {
    const reply = await axios.post<PrepersonReply>(`${API_BASE}/api/PrePerson/preperson1`, {
      userName,
      aiName
    })
    const aiReply = reply.data.data.reply
    const selectedTags = reply.data.data.selected.tags
    const avatarUrl = `${API_BASE}/${reply.data.data.selected.avatarUrl}`
    return [aiReply, selectedTags, avatarUrl]
    // {
    //    reply: ...,
    //    selected:{
    //        tags: [],
    //        avatarUrl: xxx
    //    }
    // }
  } catch (err) {
    if (axios.isAxiosError(err)) {
      const data = err.response?.data as PrepersonReply | undefined
      if (data && typeof data.code === 'number') {
        throw new ApiError(data.code, data.message)
      }
    }
    throw err
  }
}

// 申请好友
// 返回 { code: responst.data.code, sessionId }
export async function applyFriendAsync(
  tags: string[],
  avatarUrl: string,
  preReply: string,
  userName: string,
  aiName: string
) {
  try {
    const responst = await axios.post<ApplyFriendReply>(
      `${API_BASE}/api/ApplyFriend/apply`,
      {
        tags,
        avatarUrl,
        preReply,
        userName,
        aiName
      }
    )
    const sessionId = responst.data.data.sessionId
    localStorage.setItem(`sessionId_${aiName}`, sessionId?.toString())
    if (responst.data.code !== 200) {
      console.log(`回消息失败:${responst.data.message}`)
      throw new ApiError(responst.data.code, responst.data.message)
    }
    // code 给调用方判断成败；sessionId 给调用方绑到联系人上（好友对象里也留一份）
    return { code: responst.data.code, sessionId }
  } catch (err) {
    if (axios.isAxiosError(err)) {
      const data = err.response?.data as PrepersonReply | undefined
      if (data && typeof data.code === 'number') {
        throw new ApiError(data.code, data.message)
      }
    }
    throw err
  }
}

// 后端校验失败时回的是 ASP.NET 的 problem+json（{ title, errors: { 字段: [说明] } }），
// 没有 code 字段。以前这里只认 code，认不出就把响应吞掉返回 undefined，
// 日志里只剩「上传文件失败: undefined」——这里统一榨成一句能看懂的说明
function pickProblemMessage(data: unknown): string | undefined {
  if (!data || typeof data !== 'object') return undefined
  const body = data as {
    message?: unknown
    title?: unknown
    errors?: Record<string, string[] | undefined>
  }
  if (typeof body.message === 'string' && body.message) return body.message
  if (body.errors && typeof body.errors === 'object') {
    const first = Object.entries(body.errors)[0]
    if (first) return `${first[0]}: ${first[1]?.[0] ?? '参数不合法'}`
  }
  if (typeof body.title === 'string' && body.title) return body.title
  return undefined
}

// 上传文件
// 注意：后端 FileDto.file 是 IFormFile，必须走 multipart/form-data 表单；
// 以前把 FormData 塞进 JSON body（{ file: formData }）序列化后只剩 {}，后端永远收不到文件
export async function uploadFileAsync(aiName: string, userName: string, file: File) {
  const sessionId = localStorage.getItem(`sessionId_${aiName}`)
  const formData = new FormData()
  formData.append('file', file) // 字段名必须有，和后端 FileDto.file 对应
  formData.append('aiName', aiName)
  formData.append('userName', userName)
  formData.append('sessionId', sessionId ?? '')
  // 排查用：真发出去的是什么。aiName/userName 为空时后端只会回「field is required」，很难反查
  console.log('[upload] 发送:', {
    aiName,
    userName,
    sessionId,
    file: file.name,
    size: file.size
  })
  try {
    // 不要手动设置 Content-Type：浏览器会给 multipart 补上 boundary，手写反而会缺 boundary 被后端拒
    const reply = await axios.post<UploadFileReply>(
      `${API_BASE}/api/File/file1`,
      formData
    )
    const replyCode = reply.data.code
    return replyCode
  } catch (err) {
    if (axios.isAxiosError(err)) {
      const data = err.response?.data
      const status = err.response?.status
      const code = (data as { code?: unknown } | undefined)?.code
      // 业务错误带 code → 原样透出；校验错误（problem+json）→ 用 HTTP 状态码 + 具体字段说明
      throw new ApiError(
        typeof code === 'number' ? code : (status ?? 0),
        pickProblemMessage(data) ?? err.message
      )
    }
    throw err
  }
}
