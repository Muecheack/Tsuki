import { defineStore } from 'pinia'
import { computed, ref, watch } from 'vue'
import nekoHead from '@/assets/avator/neko_head.png'
import twohiregird from '@/assets/avator/twohiregird.jpg'

// 官方账号的稳定标识（不随语言变：会话查找 / sessionId key / 判断都用它）
// 显示名走 nameKey → 渲染时 t() 翻译，切语言实时跟随（只有官方有 nameKey，其它好友不译）
const OFFICIAL_ID = 'official'

// AI 人设名：预发人设接口（preAiPerson）的 aiName 参数
// 注册页注册成功后发一次、应用启动时补发一次，两处共用这个常量，避免硬编码漂移
export const AI_PERSON_NAME = 'Tsuki'

// 非官方好友的本地缓存 key（官方由常量提供，不入库）
const LOCAL_FRIEND = 'local_friend'

// 官方未读数的本地缓存 key：官方好友本身不入库，但它「读过没读过」的状态要持久化
const LOCAL_UNREAD_OFFICIAL = 'local_unread_official'

// 一条消息：谁发的 + 内容
export interface IMessage {
  sender: string // 'user' = 自己；其它 = 对方名字
  type: 'text' | 'card' | 'file'
  text?: string
  // i18nKey：消息内容走语言包（写死的引导/前言消息用）。
  // 有 i18nKey 时优先用它渲染（t(i18nKey)），切语言自动跟随；text 作为普通动态消息的兜底
  i18nKey?: string
}

// 好友部分
export interface IFriend {
  name: string
  nameKey?: string
  message: IMessage[]
  avator?: string
  unread: number
  // 后端给的会话 id：申请好友成功后绑上来（跟着好友对象一起落盘，重启也还在）
  // 官方等没走过申请流程的联系人没有这个字段
  sessionId?: number
}

// 头像资源
export interface IAvatorDescription {
  url: string
  description: string
}

// 朋友圈行为
export interface IMoments {
  name: string
  action: 'like' | null
  comment: string | null
  time: string
}

// 性格标签
export interface IPersonTags {
  text: string
  type: string
}

// 预人设草稿
// [aiReply, tags, avatarUrl]
export type IPrePersonDataDraft = [aiReply: string, tags: string[], avatarUrl: string]

// 预人设草稿的本地缓存 key
const DRAFT_STORAGE_KEY = 'bot-face-draft'

// 从本地缓存读草稿
function readDraftFromStorage(): IPrePersonDataDraft[] {
  try {
    const raw = localStorage.getItem(DRAFT_STORAGE_KEY)
    return raw ? (JSON.parse(raw) as IPrePersonDataDraft[]) : []
  } catch {
    return []
  }
}

// 从本地缓存读其它好友（官方不读：它由常量拼在前面）
// 只认数组形态：旧数据/手改坏的数据一律当空处理，否则 spread 时会直接抛错把整个 store 打死
function getAllFriendLocal(): IFriend[] {
  try {
    const raw = localStorage.getItem(LOCAL_FRIEND)
    if (!raw) return []
    const parsed: unknown = JSON.parse(raw)
    if (!Array.isArray(parsed)) return []
    return parsed
      .filter((x): x is IFriend => !!x && typeof x.name === 'string' && x.name !== OFFICIAL_ID)
      .map((f) => ({
        ...f,
        // 兜底：message / unread 缺失时给可用默认值，避免渲染到 undefined 报错
        message: Array.isArray(f.message) ? f.message : [],
        unread: typeof f.unread === 'number' ? f.unread : 0
      }))
  } catch {
    return []
  }
}

// 把其它好友整份写回本地（覆盖式：本地内容永远 = 当前内存里的非官方好友）
function setFriendLocal(list: IFriend[]) {
  try {
    localStorage.setItem(LOCAL_FRIEND, JSON.stringify(list.filter((f) => f.name !== OFFICIAL_ID)))
  } catch (e) {
    console.error('[friends] 好友本地持久化失败:', e)
  }
}

// 读官方未读数（没存过 / 存坏了都当 0）
function readOfficialUnread(): number {
  try {
    const raw = localStorage.getItem(LOCAL_UNREAD_OFFICIAL)
    if (!raw) return 0
    const n = Number(JSON.parse(raw))
    return Number.isFinite(n) && n > 0 ? Math.floor(n) : 0
  } catch {
    return 0
  }
}

// 写官方未读数（单独存一个数字，与 local_friend 解耦）
function writeOfficialUnread(n: number) {
  try {
    localStorage.setItem(LOCAL_UNREAD_OFFICIAL, JSON.stringify(n > 0 ? Math.floor(n) : 0))
  } catch (e) {
    console.error('[friends] 未读本地持久化失败:', e)
  }
}

export const useChatStore = defineStore('chat', () => {
  // 预人设草稿：初始化就从本地缓存读 —— 二次打开数据如旧，无需等请求
  const draft = ref<IPrePersonDataDraft[]>(readDraftFromStorage())

  // 写入草稿：内存 + 本地缓存一起更新（注册预发/启动补发都用它）
  function setDraft(data: IPrePersonDataDraft) {
    draft.value.push(data)
    localStorage.setItem(DRAFT_STORAGE_KEY, JSON.stringify(draft.value))
  }

  // 官方消息
  const official_friend: IFriend = {
    name: OFFICIAL_ID,
    nameKey: 'guide.official', // 显示名走语言包（只有官方有）
    message: [
      // 引导
      { sender: OFFICIAL_ID, i18nKey: 'guide.hello', type: 'text' },
      { sender: OFFICIAL_ID, i18nKey: 'guide.intro', type: 'text' },
      { sender: OFFICIAL_ID, i18nKey: 'guide.contactsAreAI', type: 'text' },
      { sender: OFFICIAL_ID, i18nKey: 'guide.chat', type: 'text' },
      { sender: OFFICIAL_ID, i18nKey: 'guide.talk', type: 'text' },
      { sender: OFFICIAL_ID, i18nKey: 'guide.discuss', type: 'text' },
      { sender: OFFICIAL_ID, i18nKey: 'guide.shareFile', type: 'text' },
      { sender: OFFICIAL_ID, i18nKey: 'guide.shareImage', type: 'text' },
      { sender: OFFICIAL_ID, i18nKey: 'guide.moments', type: 'text' },
      { sender: OFFICIAL_ID, i18nKey: 'guide.noLewd', type: 'text' },
      { sender: OFFICIAL_ID, i18nKey: 'guide.noCall', type: 'text' },
      { sender: OFFICIAL_ID, i18nKey: 'guide.recommend', type: 'text' },
      { sender: OFFICIAL_ID, i18nKey: 'guide.waiting', type: 'text' },
      { sender: OFFICIAL_ID, i18nKey: 'guide.bye', type: 'text' },
      // 卡片消息示例：sender 是卡片要展示的联系人，点击会弹出中央面板（详情+申请好友）
      { sender: 'Tsuki', type: 'card' }
    ],
    avator: nekoHead,
    // 未读：不再是写死的演示值，配合下面的 addChatList 按「对方来的新消息」动态累加
    // 读过的状态持久化在 LOCAL_UNREAD_OFFICIAL 里，重启不会被重置
    unread: readOfficialUnread()
  }

  // 好友列表 = 官方（常量，永远排在第一个）+ 本地持久化读回来的其它好友
  // 注：读回来的是「非官方」好友；官方由常量提供，不会进本地缓存
  const friends = ref<IFriend[]>([
    official_friend,
    ...getAllFriendLocal()
    // {
    //   name: 'Aiso',
    //   message: [
    //     { sender: 'Aiso', text: '我是第三个测试', type: 'text' },
    //     { sender: 'Aiso', text: '你好喵~', type: 'text' },
    //     { sender: 'Aiso', text: '天气真好', type: 'text' }
    //   ],
    //   sessionId: 000,
    //   avatar: xxx,
    //   unread: 3 // 演示用：= 对方消息数（3 条）；未来由 AI 回复/后端推送累加
    // }
  ])

  // 朋友圈行为
  const momentsAction = ref<IMoments[]>([])

  // 历史好友申请记录
  const friendsRequest = ref<string[]>([])

  // 性格标签
  const personTages = ref<IPersonTags[]>([
    // TODO: 测试数据
    // { text: '傲娇', type: '傲娇' },
    // { text: '萝莉', type: '萝莉' }
  ]) as any

  // 当前打开的会话（侧栏点中的那个）：用来区分「正在看的」和「没在看的新消息」
  // 由 KeKePage 选会话时调 setActiveChat；没选中时是空串，任何新消息都算未读
  const activeChat = ref('')
  function setActiveChat(name: string) {
    activeChat.value = name
  }

  // 监听自动落库
  watch(
    friends,
    (list) => {
      setFriendLocal(list)
      writeOfficialUnread(list.find((f) => f.name === OFFICIAL_ID)?.unread ?? 0)
    },
    { deep: true }
  )

  // 初始化会话：好友不存在就创建（存在则忽略）
  function chatListInit(name: string, avator?: string): Boolean {
    if (friends.value.find((x) => x.name === name)) return true
    friends.value.push({ name, message: [], avator, unread: 0 })
    return false
  }

  // 给某个好友追加消息
  function addChatList(
    parentName: string,
    sender: string,
    text: string,
    type: 'text' | 'card' | 'file' = 'text'
  ) {
    const target = friends.value.find((x) => x.name === parentName)
    if (target) {
      target.message.push({ sender, text, type })
      // 未读只算「对方发来的、且用户没在看的」新消息：
      // 自己发的（sender === 'user'）不算；用户正打开这个会话（activeChat）也不算
      if (sender !== 'user' && parentName !== activeChat.value) target.unread++
    }
  }

  // 置顶好友(把选中好友的位置删了然后插入index0)
  function pinFriend(name: string) {
    const currentIndex = friends.value.findIndex((x) => x.name === name)
    if (currentIndex > 0) {
      const [e] = friends.value.splice(currentIndex, 1)
      friends.value.unshift(e)
    }
  }

  // 删好友
  function removeFriend(name: string) {
    friends.value = friends.value.filter((x) => x.name !== name)
  }

  // 清零某好友的未读数（点击联系人进入聊天 / 点击红点都调它）
  function clearUnread(name: string) {
    const target = friends.value.find((f) => f.name === name)
    if (target) target.unread = 0
  }

  // 把后端返回的 sessionId 绑到某个联系人上（申请好友成功后调）
  // 好友对象一变，上面的 watch(friends, deep) 就会把它一起写回本地
  function setFriendSessionId(name: string, sessionId: number) {
    const target = friends.value.find((f) => f.name === name)
    if (target) target.sessionId = sessionId
  }

  return {
    draft,
    setDraft,
    friends,
    officialId: OFFICIAL_ID,
    momentsAction,
    friendsRequest,
    personTages,
    chatListInit,
    addChatList,
    pinFriend,
    removeFriend,
    clearUnread,
    setActiveChat,
    setFriendSessionId
  }
})
