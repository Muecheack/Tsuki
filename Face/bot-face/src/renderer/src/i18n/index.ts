import { ref } from 'vue'
import zhCN from './zh-CN.json'
import en from './en.json'
import ja from './ja.json'
import meow from './meow.json'

// 支持的语言 key（对应设置面板语言下拉的 key）
export type LangKey = 'zh' | 'en' | 'ja' | 'meow'

const messages: Record<LangKey, Record<string, unknown>> = {
  zh: zhCN,
  en,
  ja,
  meow
}

const STORAGE_KEY = 'bot-face-lang'

// 从 localStorage 恢复上次选择的语言；非法值回退中文
function readStoredLang(): LangKey {
  const v = localStorage.getItem(STORAGE_KEY)
  return v === 'zh' || v === 'en' || v === 'ja' || v === 'meow' ? v : 'zh'
}

// 当前语言（ref：模板里调用 t() 时会建立响应式依赖，切换语言自动重渲染）
export const currentLang = ref<LangKey>(readStoredLang())

// 切换语言（同时持久化）
export function setLang(lang: LangKey): void {
  currentLang.value = lang
  localStorage.setItem(STORAGE_KEY, lang)
}

// 按点路径取值：t('login.nickname') → messages.zh.login.nickname
function getByPath(dict: Record<string, unknown>, path: string): unknown {
  return path.split('.').reduce<unknown>((acc, k) => {
    if (acc && typeof acc === 'object') {
      return (acc as Record<string, unknown>)[k]
    }
    return undefined
  }, dict)
}

// 取文案：当前语言 → 缺 key 回退中文 → 再缺回退 key 本身；支持 {name} 插值
export function t(key: string, params?: Record<string, string | number>): string {
  let raw: unknown = getByPath(messages[currentLang.value], key)
  if (typeof raw !== 'string') raw = getByPath(messages.zh, key)
  if (typeof raw !== 'string') return key
  let text: string = raw
  if (params) {
    for (const [k, v] of Object.entries(params)) {
      text = text.replaceAll(`{${k}}`, String(v))
    }
  }
  return text
}
