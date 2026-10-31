<template>
    <!-- 先画背景图-->
    <Transition name="fade" appear>
        <div class="flex w-full h-screen bg-cover bg-center" :style="{ backgroundImage: `url(${bgImageUrl})` }"
            @click="activeMenu = ''">

            <!-- 再画侧面板 -->
            <aside
                class="flex flex-col border border-r backdrop-blur-2xl backdrop-saturate-150 bg-white/20 border-white/15 relative rounded gap-2 shrink-0 aside-3d"
                :style="{ width: asideWidth + 'px' }">

                <!-- 顶部文字 -->
                <div class="flex flex-row justify-between h-11 items-center px-2 border border-b rounded">
                    <span class="text-sm text-black font-[serif]">KeKe</span>
                    <div class="relative group ">
                        <fv-icon-element value="Zoom" :font-size="17" class="cursor-pointer hover:opacity-50" />
                        <div class="absolute top-full mb-2 left-1/2 -translate-x-3/4 whitespace-nowrap
                            bg-white/70 backdrop-blur-2xl border border-white/30 rounded-sm px-2 py-1 text-xs text-gray-700
                            opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none">
                            {{ t('keke.zoom') }}
                        </div>
                    </div>
                </div>

                <!-- 逐条渲染朋友列表（卡片淡入） -->
                <TransitionGroup name="bubble" tag="div" class="flex flex-col flex-1 border-white">

                    <!-- 装头像和昵称的，拿来循环的卡片 -->
                    <div v-for="(i, index) in chat.friends" :key="index"
                        @click="sessionRowClicked(i.name)"
                        class="relative flex flex-row gap-4 cursor-pointer items-center justify-between p-4 w-full h-16 hover:bg-gray-200"
                        :class="selectedName === i.name ? 'bg-gray-200' : ''">

                        <!-- 未读红点徽章：有未读才显示，红底白字圆形，点击直接清零 -->
                        <div v-if="i.unread > 0" @click.stop="chat.clearUnread(i.name)"
                            class="absolute top-2 right-2 z-10 h-[18px] min-w-[18px] px-1 rounded-full bg-red-500 text-white text-[10px] leading-none flex items-center justify-center cursor-pointer">
                            {{ i.unread > 99 ? '99+' : i.unread }}
                        </div>

                        <!-- 头像 -->
                        <div class="w-10 h-10 overflow-hidden rounded-full">
                            <img :src="i.avator" class="w-full h-full object-cover object-center" />
                        </div>

                        <!-- 文字区域 -->
                        <div class="flex flex-col flex-1 min-w-0">
                            <!-- 官方走语言包（t(nameKey)），其它好友直接显示 name -->
                            <span class="text-[#1A1A1A] text-base">{{ i.nameKey ? t(i.nameKey) : i.name }}</span>
                            <!-- 最新消息预览：取最后一条消息的 text -->
                            <span class="text-sm text-gray-500">{{ i.message.at(-1)?.text?.slice(0, 10) }}</span>
                        </div>

                        <!-- 旁边的操作按钮 -->
                        <div class="relative" @click.stop="activeMenu = activeMenu === i.name ? '' : i.name">
                            <fv-icon-element value="More" :font-size="17" class="cursor-pointer hover:opacity-50" />

                            <!-- 弹出的选项框：从上方弹出（带淡入动画）-->
                            <Transition name="pop">
                                <div v-if="activeMenu === i.name"
                                    class="absolute bottom-full right-0 mb-1 z-50 bg-white/80 backdrop-blur-2xl border border-white/30 rounded-sm shadow-lg min-w-28 overflow-hidden">
                                    <!-- 顶置 -->
                                    <div class="flex items-center gap-2 px-3 py-2 cursor-pointer hover:bg-white/50"
                                        @click.stop="chat.pinFriend(i.name); activeMenu = ''">
                                        <fv-icon-element value="Upload" :font-size="14" />
                                        <span class="text-xs text-gray-700">{{ t('keke.pin') }}</span>
                                    </div>
                                    <!-- 移除 -->
                                    <div class="flex items-center gap-2 px-3 py-2 cursor-pointer hover:bg-white/50 border-t border-white/30"
                                        @click.stop="chat.removeFriend(i.name); activeMenu = ''">
                                        <fv-icon-element value="Remove" :font-size="14" />
                                        <span class="text-xs text-gray-700">{{ t('keke.remove') }}</span>
                                    </div>
                                </div>
                            </Transition>
                        </div>
                    </div>
                </TransitionGroup>

                <!-- 下方若干按钮 -->
                <div class="flex flex-row h-7 justify-around items-center border border-t p-4">
                    <!-- 朋友圈（鼠标处弹出 tooltip）-->
                    <div class="relative group cursor-pointer" @click="canShowMomentsPanel = true">
                        <fv-icon-element value="EmojiTabFoodPlants" :font-size="17" class="hover:opacity-50" />
                        <div class="absolute bottom-full mb-2 left-1/2 -translate-x-1/2 whitespace-nowrap
                            bg-white/70 backdrop-blur-2xl border border-white/30 rounded-sm px-2 py-1 text-xs text-gray-700
                            opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none">
                            {{ t('keke.moments') }}
                        </div>
                    </div>

                    <!-- 添加朋友(鼠标弹出 tooltip) -->
                    <div class="relative group cursor-pointer" @click="canShowAddFriendsPanel = true">
                        <fv-icon-element value="AddFriend" :font-size="17" class="hover: opacity-50" />
                        <div class="absolute bottom-full mb-2 left-1/2 -translate-x-1/2 whitespace-nowrap
                            bg-white/70 backdrop-blur-2xl border border-white/30 rounded-sm px-2 py-1 text-xs text-gray-700
                            opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none">
                            {{ t('keke.addFriend') }}
                        </div>
                    </div>

                    <!-- 好友申请 -->
                    <div class="relative group cursor-pointer" @click="canShowFriendsRequest = true">
                        <fv-icon-element value="People" :font-size="17" class="hover: opacity-50" />
                        <div class="absolute bottom-full mb-2 left-1/2 -translate-x-1/2 whitespace-nowrap
                            bg-white/70 backdrop-blur-2xl border border-white/30 rounded-sm px-2 py-1 text-xs text-gray-700
                            opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none">
                            {{ t('keke.requests') }}
                        </div>
                    </div>

                    <!-- 设置（鼠标处弹出 tooltip）-->
                    <div class="relative group cursor-pointer" @click="showSetting">
                        <fv-icon-element value="Settings" :font-size="17" class="hover:opacity-50" />
                        <div class="absolute bottom-full mb-2 left-1/2 -translate-x-1/2 whitespace-nowrap
                            bg-white/70 backdrop-blur-2xl border border-white/30 rounded-sm px-2 py-1 text-xs text-gray-700
                            opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none">
                            {{ t('keke.settings') }}
                        </div>
                    </div>
                </div>
            </aside>

            <!-- “设置面板” -->
            <fv-panel v-model="canShowDrawer" :is-central-side="true" :teleport="true" width="calc(55% - 50px)"
                height="calc(70% - 50px)" content-padding="15px" :is-acrylic="true" :title="t('keke.panelSetting')">
                <template v-slot:container>
                    <SettingPage :bg-image-path="bgImagePath" @bg-change="onBgChange" />
                </template>
            </fv-panel>

            <!-- 朋友圈Panel -->
            <fv-panel v-model="canShowMomentsPanel" :is-near-side="true" :is-acrylic="true"
                :title="t('keke.panelMoments')" width="450" content-padding="15px">
                <template v-slot:container>
                    <MomentsPage />
                </template>
            </fv-panel>

            <!-- 加好友Panel -->
            <fv-panel v-model="canShowAddFriendsPanel" :is-near-side="true" :is-acrylic="true"
                :title="t('keke.panelAddFriend')" content-padding="15px">
                <template v-slot:container>
                    <AddFriendsPage />
                </template>
            </fv-panel>

            <!-- 好友申请记录Panel -->
            <fv-panel v-model="canShowFriendsRequest" :is-near-side="true" :is-acrylic="true"
                :title="t('keke.panelRequests')" content-padding="15px">
                <template v-slot:container>
                    <FriendsRequestHistory />
                </template>
            </fv-panel>

            <!-- 拖拽把手：按住左右拖动调整侧栏宽度 -->
            <div class="w-1.5 shrink-0 cursor-col-resize hover:bg-white/60 transition-colors"
                @mousedown.prevent="startResize" />

            <!-- 未选中：空态提示；选中：ChatPage。一个 Transition 处理淡出淡入 -->
            <Transition name="fade" appear>
                <div v-if="!selectedName" class="flex-1 flex items-center justify-center text-gray-100 text-xl">
                    {{ t('keke.empty') }}
                </div>
                <ChatPage v-else :name="selectedName" class="flex-1" />
            </Transition>
        </div>
    </Transition>
</template>

<style lang="css" scoped>
.fade-enter-from {
    opacity: 0;
}

.fade-enter-active {
    transition: opacity 0.5s;
}

.fade-enter-to {
    opacity: 1;
}

/* 好友卡片淡入：只改透明度 */
.bubble-enter-from {
    opacity: 0;
}

.bubble-enter-active {
    transition: opacity 0.3s ease;
}

.bubble-enter-to {
    opacity: 1;
}

/* 弹出菜单淡入：从上方 6px 滑下 */
.pop-enter-from {
    opacity: 0;
    transform: translateY(-6px);
}

.pop-enter-active {
    transition: opacity 0.2s ease, transform 0.2s ease;
}

.pop-enter-to {
    opacity: 1;
    transform: translateY(0);
}

/* Panel 关闭按钮：灰色（teleport 到 body，用 :deep 穿透）*/
:deep(.control-btn) {
    color: #6b7280 !important;
    /* gray-500 */
}

/* 侧栏立体边框：左上角高光 + 右下角阴影 */
.aside-3d {
    box-shadow:
        inset 1px 1px 0 rgba(255, 255, 255, 0.6),
        /* 左上高光 */
        inset -1px -1px 0 rgba(255, 255, 255, 0.6);
    /* 右下阴影 */
}
</style>

<script setup lang="ts">
import nekoUrl from '@/assets/neko.png';
import { useChatStore } from '@/stores/chat';
import ChatPage from './ChatPage.vue';
import SettingPage from './SettingPage.vue';
import MomentsPage from './MomentsPage.vue';
import { ref } from 'vue';
import AddFriendsPage from './AddFriendsPage.vue';
import FriendsRequestHistory from './FriendsRequestHistory.vue';
import { t } from '@/i18n';

const chat = useChatStore();

const selectedName = ref(''); // 点击选中的名字
const activeMenu = ref(''); // 当前展开操作菜单的好友名（'' = 无）
const bgImageUrl = ref(nekoUrl); // 背景图（默认用项目自带的 neko.png）
const bgImagePath = ref(''); // 背景图路径显示（用于显示框）
const canShowDrawer = ref(false) as any; // 是否显示设置
const canShowMomentsPanel = ref(false) as any; // 是否显示朋友圈
const canShowAddFriendsPanel = ref(false) as any; // 是否显示添加好友
const canShowFriendsRequest = ref(false) as any; // 是否显示好友申请记录

// 侧栏宽度
const asideWidth = ref(288);
const MIN_WIDTH = 270;
const MAX_WIDTH = 640;

// 点击好友行：选中会话 + 清红点
// （AI 人设的预发已移到注册页：preAiPerson 在 Register_V 注册成功时调用，结果存 chat.draft）
function sessionRowClicked(name: string) {
    selectedName.value = name;
    chat.setActiveChat(name) // 告诉 store 现在在看哪个会话：它的新消息不再算未读
    chat.clearUnread(name)
}

// 按住把手拖动
function startResize(_e: MouseEvent) {
    document.body.style.cursor = 'col-resize';
    document.body.style.userSelect = 'none';

    const onMove = (ev: MouseEvent) => {
        asideWidth.value = Math.min(Math.max(ev.clientX, MIN_WIDTH), MAX_WIDTH);
    };
    const onUp = () => {
        document.body.style.cursor = '';
        document.body.style.userSelect = '';
        document.removeEventListener('mousemove', onMove);
        document.removeEventListener('mouseup', onUp);
    };

    document.addEventListener('mousemove', onMove);
    document.addEventListener('mouseup', onUp);
}

// 接收 SettingPage 传来的背景图（路径 + data URL）
function onBgChange(payload: { path: string; url: string }) {
    bgImagePath.value = payload.path;   // 显示框：文件名
    bgImageUrl.value = payload.url;     // 替换背景
}

// 设置点击逻辑
const showSetting = () => canShowDrawer.value = true;
</script>
