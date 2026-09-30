<script setup lang="ts">
import { ref, getCurrentInstance } from 'vue';
import nekoHead from '@/assets/avator/neko_head.png'
import { t } from '@/i18n';
import { useChatStore } from '@/stores/chat';
import { createMessageBar } from '@creatorsn/vfluent3';

const chat = useChatStore();

// appContext 必须在 setup 顶层取（createMessageBar 需要它继承主题）
const appContext = getCurrentInstance()!.appContext
const friendName = ref(''); // 输入框里的草稿（搜索词）
const searchedName = ref(''); // 搜索结果：昵称（回车请求完成后才有）
const searchedAvatar = ref(nekoHead); // 搜索结果显示的头像（已是好友则复用他的头像，否则用测试头像）
const status = ref<'idle' | 'searching' | 'done'>('idle'); // idle=未搜索 searching=请求中 done=出结果

// 回车搜索：先进入“搜索中...”，模拟请求后端，完成后显示结果卡片
// 注：status === 'searching' 时直接 return，防止连按回车重复发请求
function search() {
    if (!friendName.value.trim() || status.value === 'searching') return

    status.value = 'searching'

    // TODO: 以后这里换成真实后端请求（fetch/axios），头像/昵称由后端返回
    setTimeout(() => {
        searchedName.value = friendName.value.trim()
        // 已存在的好友：复用他列表里的头像；新朋友：先占位用测试头像
        searchedAvatar.value = chat.friends.find(x => x.name === searchedName.value)?.avator ?? nekoHead
        status.value = 'done'
    }, 1200) // 模拟网络延迟 1.2s
}

// 添加好友到store；结果用全局提示（fv-message-bar 默认样式）替代 alert
function addFriend(name) {
    // chatListInit 返回 true = 已是好友，false = 添加成功（注意：语义是反的）
    const isExist = chat.chatListInit(name, nekoHead);
    createMessageBar(
        isExist ? t('addFriend.alreadyFriend') : t('addFriend.addSuccess'),
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

            <!-- 搜索结果卡片（回车请求完成后才显示，点击跳详情） -->
            <div v-if="status === 'done'" class="flex hover:bg-gray-300 relative">
                <!-- 联系人卡片 -->
                <div class="flex flex-row gap-4 h-15 items-center">
                    <!-- 头像（已存在好友显示他的头像；新朋友先占位用测试图，TODO: 后端返回后换掉） -->
                    <div class="w-13 h-13 overflow-hidden rounded-full">
                        <img :src="searchedAvatar" class="w-full h-full object-center object-cover" alt="AI头像" />
                    </div>

                    <!-- 昵称 -->
                    <span class=" text-xl text-gray-700">{{ searchedName }}</span>

                    <!-- 添加图标 -->
                    <div class="flex cursor-pointer hover:opacity-50 absolute right-4" @click="addFriend(searchedName)">
                        <fv-icon-element value="Add" :font-size="20" />
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
</style>