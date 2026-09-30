import axios from 'axios'

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

export type RegisterReply = ApiReply<RegisterData>
export type ChatReply = ApiReply<ChatData>
export type PrepersonReply = ApiReply<PrePerson>
export type ApplyFriendReply = ApiReply<ApplyFirend>

// 前端把名字和人设和头像名给ai发过去
// 发送注册信息，成功返回 { id, token }（token 是免登录凭证，id 是新用户 Id）
// 失败统一抛 ApiError（带后端 code）或网络异常，不在内部吞错
export async function sendUserInfoAsync(
  name: string,
  gender: string,
  call: string,
  apiKey: string
): Promise<RegisterReply['data']> {
  // TODO: 这里加上后端地址
  try {
    const reply = await axios.post<RegisterReply>('http://localhost:5108/api/Auth/register', {
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
    const reply = await axios.post<ChatReply>('http://localhost:5108/api/Chat/chat1', {
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
    // 上面显式 throw 的 ApiError（code !== 200 分支）→ 不是 AxiosError，落到 throw err 原样抛
    // 网络错误/后端没起（AxiosError 但无 response.data）→ 落到 throw err 原样抛
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
export async function preAiPerson(
  userName: string,
  aiName: string
): Promise<[string, string[], string]> {
  const baseUrl = 'http://localhost:5108'
  try {
    // TODO: 填路径
    const reply = await axios.post<PrepersonReply>(`${baseUrl}/api/PrePerson/preperson1`, {
      userName,
      aiName
    })
    const aiReply = reply.data.data.reply
    const selectedTags = reply.data.data.selected.tags
    const avatarUrl = `${baseUrl}/${reply.data.data.selected.avatarUrl}`
    return [aiReply, selectedTags, avatarUrl]
    // {
    //    reply: ...,
    //    selected:{
    //        tags: [],
    //        avatarUrl: xxx
    //    }
    // }
  } catch (err) {
    // HTTP 非 2xx（axios 抛 AxiosError，且后端返回体有 code）→ 转成带 code 的 ApiError
    // 网络错误/后端没起（AxiosError 但无 response.data）→ 落到 throw err 原样抛
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
export async function applyFriendAsync(
  tags: string[],
  avatarUrl: string,
  preReply: string,
  userName: string,
  aiName: string
) {
  try {
    const responst = await axios.post<ApplyFriendReply>(
      'http://localhost:5108/api/ApplyFriend/apply',
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
    // HTTP 非 2xx（axios 抛 AxiosError，且后端返回体有 code）→ 转成带 code 的 ApiError
    // 网络错误/后端没起（AxiosError 但无 response.data）→ 落到 throw err 原样抛
    if (axios.isAxiosError(err)) {
      const data = err.response?.data as PrepersonReply | undefined
      if (data && typeof data.code === 'number') {
        throw new ApiError(data.code, data.message)
      }
    }
    throw err
  }
}
