<script setup lang="ts">
import { useChatStore } from '@/stores/chat';
import { createMessageBar } from '@creatorsn/vfluent3';
import { t } from '@/i18n';
import { getCurrentInstance } from 'vue';
import { applyFriendAsync } from '../../../../server/api/http'

const chat = useChatStore()

// appContext 必须在 setup 顶层取（createMessageBar 需要它继承主题）；setup 内实例必然存在
const appContext = getCurrentInstance()!.appContext

const prop = defineProps<{ name: string, avator: string }>()

// tags, avatarUrl, userName, aiName, preReply
const draftData = chat.draft[0]
const preReply = draftData[0]
const tags = draftData[1]
const avatarUrl = draftData[2]
const user = JSON.parse(localStorage.getItem('bot-face-user') || '{}');
const userName = user.nickname || '';

// 通知父组件（ChatPage）关闭卡片详情面板
const emit = defineEmits<{ (e: 'close'): void }>()

async function applyFriend(aiName: string) {
    // chatListInit 返回 true = 已是好友，false = 添加成功
    const isExist = chat.chatListInit(prop.name, prop.avator);
    emit('close') // 申请完关闭卡片面板
    // 落库结果：true = 后端 code 200；false = code ≠ 200 或请求异常
    let ok = true
    if (isExist === false) {
        // 落库
        try {
            const { code, sessionId } = await applyFriendAsync(tags, avatarUrl, preReply, userName, aiName)
            chat.addChatList(aiName, "Tsuki", preReply, 'text')
            ok = code === 200
            if (ok && sessionId != null) chat.setFriendSessionId(aiName, sessionId)
        } catch (err) {
            // 后端 code ≠ 200 会抛 ApiError；网络异常/后端没起也走这里
            console.error('申请好友失败:', err)
            ok = false
        }
        // 申请失败：把 chatListInit 刚加进列表的好友回滚掉
        // （它是先加好友再发请求的，不回滚会跟着 watch(friends) 一起落盘，重启后还赖在侧栏）
        if (!ok) chat.removeFriend(prop.name)
    }
    // 全局提示（fv-message-bar）
    createMessageBar(
        ok
            ? (isExist ? t('addFriend.alreadyFriend') : t('addFriend.addSuccess'))
            : t('addFriend.applyFail'),
        { status: ok ? 'default' : 'error', once: true, autoClose: 2000 },
        appContext
    )
}
</script>

<template>
    <div class="flex flex-col gap-7 items-center justify-start p-4 w-full overflow-y-auto">
        <!-- 头像 -->
        <div class="overflow-hidden w-20 h-20 rounded-full">
            <img :src=prop.avator alt="占位头像" class="object-cover w-full h-full object-center" />
        </div>
        <!-- 名字 -->
        <span class="text-gray-700 text-2xl">{{ prop.name }}</span>

        <!-- 性格 -->
        <div class="flex flex-col w-full">
            <!-- 性格（i18n） -->
            <div class="flex">
                <span class="text-gray-500">{{ t('card.personality') }}</span>
            </div>
            <fv-tag v-model="chat.personTages"></fv-tag>
        </div>

        <!-- 申请好友按钮（i18n） -->
        <div @click="applyFriend(prop.name)">
            <fv-button class="text-gray-700">{{ t('card.applyFriend') }}</fv-button>
        </div>
    </div>
</template>