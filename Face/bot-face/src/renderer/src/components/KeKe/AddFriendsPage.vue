<script setup lang="ts">
import { ref, getCurrentInstance, computed } from 'vue';
import nekoHead from '@/assets/avator/neko_head.png'
import { t } from '@/i18n';
import { useChatStore } from '@/stores/chat';
import { createMessageBar } from '@creatorsn/vfluent3';
import { preAiPerson, applyFriendAsync } from '../../../../server/api/http'

const chat = useChatStore();

// appContext 必须在 setup 顶层取（createMessageBar 需要它继承主题）
const appContext = getCurrentInstance()!.appContext
const friendName = ref(''); // 输入框里的草稿（搜索词）
const searchedName = ref(''); // 搜索结果：昵称（回车请求完成后才有）
const status = ref<'idle' | 'searching' | 'done' | 'error'>('idle'); // idle=未搜索 searching=请求中 done=出结果 error=搜索失败
const user = JSON.parse(localStorage.getItem('bot-face-user') || '{}');
const userName = user.nickname || '';

// 从本地读取的最新预人设(标签, 头像), 匹配输入框输入的昵称, 防止二次发人设
const draftData = computed(() => {
    const name = searchedName.value
    if (name) {
        for (let i = chat.draft.length - 1; i >= 0; i--) {
            if (chat.draft[i][3] === name) return chat.draft[i]
        }
    }
    return null
})
const tags = computed<string[]>(() => draftData.value?.[1] ?? [])
const avatarUrl = computed(() => draftData.value?.[2] ?? '')

// 申请成功的好友名：用来把添加图标从 Add 换成 CheckMark（√）
// 存名字而不是布尔量，这样重新搜索另一个人时图标会自动回到 Add，不用额外重置
const addedName = ref('')
const addIcon = computed(() => (addedName.value !== '' && addedName.value === searchedName.value ? 'CheckMark' : 'Add'))

// 回车搜索：先进入“搜索中...”，模拟请求后端，完成后显示结果卡片
// status === 'searching' 时return，防止连按回车重复请求
async function search() {
    if (!friendName.value.trim() || status.value === 'searching') return

    status.value = 'searching'

    // setTimeout(() => {
    searchedName.value = friendName.value.trim()
    //     // 已存在的好友：复用他列表里的头像；新朋友：先占位用测试头像
    //     searchedAvatar.value = chat.friends.find(x => x.name === searchedName.value)?.avator ?? nekoHead
    //     status.value = 'done'
    // }, 1200) // 模拟网络延迟 1.2s
    // 找出name对应的人设, draft: [[aiReply, tags, avatarUrl, name], ...]
    const existingDraft = chat.draft.find(x => Array.isArray(x) && x[3] === searchedName.value)
    if (existingDraft) {
        status.value = 'done'
        return;
    }

    try {
        // 预发人设, 暂存本地, 更改状态为done
        const [aiReply, selectedTags, avatarUrl] = await preAiPerson(userName, searchedName.value)
        chat.setDraft([aiReply, selectedTags, avatarUrl, searchedName.value])

        status.value = 'done'
    } catch (error) {
        console.error('搜索AI人设失败:', error)
        // 失败态：UI 显示"出错了"，同时不再卡在 searching（否则下次回车会被开头的守卫拦住）
        status.value = 'error'
    }
}

// 添加好友到store；结果用全局提示（fv-message-bar 默认样式）替代 alert
// 预人设落本地 -> 拿reply/tags/avatar -> 申请好友 -> 拿sessionId/code -> 创建会话 -> 弹消息提示
async function addFriend(name) {
    // chatListInit 返回 true = 已是好友，false = 添加成功（注意：语义是反的）
    const isExist = chat.chatListInit(name, avatarUrl.value);
    let ok = true
    if (isExist === false) {
        try {
            const { code, sessionId } = await applyFriendAsync(tags.value, avatarUrl.value, draftData.value?.[0] ?? '', userName, name)
            ok = code === 200
            if (ok && sessionId !== null) {
                chat.setFriendSessionId(name, sessionId)
            }
        } catch (error) {
            console.error('申请好友失败:', error)
            ok = false
        }
        if (!ok) chat.removeFriend(name) // 申请失败 -> 把 chatListInit 刚加进列表的好友回滚掉
    }

    // 添加成功（新申请通过，或本来就是好友）-> 图标换成 √
    if (ok) addedName.value = name

    // 全局提示
    createMessageBar(
        ok ? t('addFriend.alreadyFriend') : t('addFriend.addSuccess'),
        { status: 'default', once: true, autoClose: 2000 },
        appContext
    )
}
</script>

<template>
    <!-- 
    只有一个搜索框和一个显示框，
    显示框显示搜索内容动态生成对应的ai联系人，
    点击后显示详情和“申请添加”按钮，
    点击后传进数据渲染进Home.
    -->
    <!-- 
    搜索框搜索不实时显示，
    点击搜索后立即向后端发送请求，
    生成ai名字+ai选择头像+ai打招呼内容（发送到聊天区域),
    中间时间用“搜索中...”代替
    -->
    <Transition name="view-slide" mode="out-in">
        <!-- 搜索视图 -->
        <div class="flex flex-col gap-4">
            <fv-text-box icon="Search" :placeholder="t('addFriend.searchPlaceholder')" class="text-gray-500"
                v-model="friendName" @keyup.enter="search" />

            <!-- 搜索中...：等待后端响应，用进度环代替 -->
            <div v-if="status === 'searching'" class="flex flex-row gap-2 items-center">
                <fv-progress-ring :legacy="true" />
                <span class="text-gray-500 text-sm">{{ t('addFriend.searching') }}</span>
            </div>

            <!-- 搜索失败：只展示文字（出错了 + 颜文字） -->
            <div v-if="status === 'error'" class="flex flex-row gap-2 items-center">
                <span class="text-red-300 text-sm">{{ t('addFriend.searchFail') }}</span>
            </div>

            <!-- 搜索结果卡片（回车请求完成后才显示，点击跳详情） -->
            <div v-if="status === 'done'" class="flex hover:bg-gray-300 relative">
                <!-- 联系人卡片 -->
                <div class="flex flex-row gap-4 h-15 items-center">
                    <!-- 头像（已存在好友显示他的头像；新朋友先占位用测试图，TODO: 后端返回后换掉） -->
                    <div class="w-13 h-13 overflow-hidden rounded-full">
                        <img :src="avatarUrl" class="w-full h-full object-center object-cover" alt="AI头像" />
                    </div>

                    <div class="flex flex-col gap-1">
                        <!-- 昵称 -->
                        <span class=" text-xl text-gray-700">{{ searchedName }}</span>
                        <!-- 人设标签, 从本地读取的tags列表循环 -->
                        <div class="flex flex-wrap gap-2">
                            <span v-for="tag in tags" :key="tag" class="text-xs text-gray-700 px-2 py-1 rounded">
                                {{ tag }}
                            </span>
                        </div>
                    </div>

                    <!-- 添加图标：未添加/添加失败 = Add，添加成功 = CheckMark（√），切换带旋转缩放动画 -->
                    <div class="flex cursor-pointer hover:opacity-50 absolute right-4" @click="addFriend(searchedName)">
                        <Transition name="add-icon" mode="out-in">
                            <fv-icon-element :key="addIcon" :value="addIcon" :font-size="20" />
                        </Transition>
                    </div>
                </div>
            </div>
        </div>


    </Transition>
</template>

<style scoped>
/* 面板内视图切换：从右滑入 */
.view-slide-enter-active {
    transition: all 0.25s ease;
}

.view-slide-enter-from {
    transform: translateX(24px);
    opacity: 0;
}

/* 添加图标：Add(＋) -> CheckMark(√) 的切换动画（旋转 + 缩放 + 淡入淡出） */
.add-icon-enter-active,
.add-icon-leave-active {
    transition: opacity 0.2s ease, transform 0.2s ease;
}

.add-icon-enter-from {
    opacity: 0;
    transform: rotate(-90deg) scale(0.4);
}

.add-icon-leave-to {
    opacity: 0;
    transform: rotate(90deg) scale(0.4);
}
</style>