<script setup lang="ts">
import { onMounted } from 'vue'
import { useChatStore, AI_PERSON_NAME } from '@/stores/chat'
import { preAiPerson } from '../../server/api/http'

const chat = useChatStore()

// 启动兜底：免登录复访时 draft 是空的（pinia 是内存态，刷新/重启即丢），补发一次 AI 人设
// 首次注册的场景由 Register_V 在注册成功时发（那时才能拿到准确的昵称）
onMounted(async () => {
    // 还没注册（没凭证）→ 不发
    if (!localStorage.getItem('bot-face-token')) return
    // 已经有数据（本次会话注册时发过了）→ 不重复发
    if (chat.draft.length > 0) return

    const user = JSON.parse(localStorage.getItem('bot-face-user') || '{}')
    const userName = user.nickname || ''
    if (!userName) return

    try {
        // 注：await 只影响这个函数自身，不阻塞页面渲染
        const [aiReply, tags, avatarUrl] = await preAiPerson(userName, AI_PERSON_NAME)
        chat.setDraft([aiReply, tags, avatarUrl])
        console.log('[draft] 启动预发人设:', { aiReply, tags, avatarUrl })
    } catch (e) {
        // 失败不影响使用（卡片头像会走 nekoHead 兜底）
        console.error('[draft] 启动预发人设失败（不影响启动）:', e)
    }
})
</script>

<template>
  <router-view />
</template>
