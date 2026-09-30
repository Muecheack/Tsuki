<script setup lang="ts">
import { ref, nextTick } from 'vue';
import nekoHead from '@/assets/avator/neko_head.png'
import { useChatStore } from '@/stores/chat';
import { t } from '@/i18n';

const chat = useChatStore();
const comment = ref('') // 输入框里的草稿
const canInput = ref(false); // 是否显示输入框
const commentBoxRef = ref<{ focus: () => void } | null>(null) // 输入框组件实例，用于聚焦
const liked = ref(false) // 是否已点赞：Heart ↔ HeartFill

// 点评论图标：显示输入框并聚焦
function openCommentBox() {
    canInput.value = true
    nextTick(() => commentBoxRef.value?.focus())
}

// 点赞
function like() {
    liked.value = true;
}

// 回车提交：搬到显示板、清空草稿、关闭输入框
function submitComment() {
    if (!comment.value.trim()) return

    // 存进列表
    chat.momentsAction.push({
        name: t('moments.me'),
        action: liked.value ? 'like' : null,
        comment: comment.value.trim(),
        time: new Date().toISOString()
    });

    // TODO: 在这添加发送后端的逻辑

    // 清空输入框
    comment.value = ''
    canInput.value = false
}

// 朋友圈内容（以后在这里加动态卡片）
</script>

<template>
    <div class="relative flex flex-col overflow-y-auto w-full h-full">
        <!-- 单条朋友圈卡片（占位）-->
        <div class="flex flex-col gap-2 p-4">
            <!-- 左边头像右边消息；上边动态下边评价，(右)上边昵称中间文案下边图片 -->
            <!-- 头像+昵称 -->
            <div class="flex flex-row gap-8">
                <div class="w-6 h-6 overflow-hidden rounded-full">
                    <img :src="nekoHead" alt="占位头像" class="object-center object-cover w-full h-full" />
                </div>
                <span class="text-gray-700 text-xs">{{ t('moments.placeholderName') }}</span>
            </div>

            <!-- 图片展示区域 -->
            <div class="w-full overflow-hidden">
                <img :src="nekoHead" alt="占位图片" class="object-cover w-full" />
            </div>

            <!-- 下方点赞和评论图标 -->
            <div class="relative flex">
                <div class="flex flex-row gap-5 absolute right-0">
                    <!-- 点赞：点击后 Heart → HeartFill -->
                    <div class="cursor-pointer" @click="like">
                        <Transition name="heart-pop" mode="out-in">
                            <fv-icon-element :key="liked ? 'fill' : 'outline'" :value="liked ? 'HeartFill' : 'Heart'"
                                :font-size="15" />
                        </Transition>
                    </div>
                    <div @click="openCommentBox" class="cursor-pointer">
                        <fv-icon-element value="Comment" :font-size="15" />
                    </div>
                </div>
            </div>
            <!-- 显示评论的板子：回车提交后才会更新 -->
            <span v-for="(v, index) in chat.momentsAction" :key="index" class="text-gray-700 text-xs">{{ v.name }} {{
                v.comment }} </span>

            <!-- 评论输入框：点评论图标显示并自动聚焦；回车发送后关闭 -->
            <fv-text-box ref="commentBoxRef" :placeholder="t('moments.commentPlaceholder')" v-model="comment"
                @keyup.enter="submitComment" class="border rounded text-gray-500" v-if="canInput" />
        </div>

        <!-- 底部发布按钮 -->

        <div class="absolute bottom-1 right-3 cursor-pointer ">
            <div class="relative group">
                <fv-icon-element value="Add" :font-size="25" />
                <div class="absolute bottom-full mb-2 left-1/2 -translate-x-1/2 whitespace-nowrap
                            bg-white/70 backdrop-blur-2xl border border-white/30 rounded-sm px-2 py-1 text-xs text-gray-700
                            opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none">
                    {{ t('moments.publish') }}
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
/* 点赞图标切换：淡入 + 从 0.3 弹到 1 */
.heart-pop-enter-active {
    transition: transform 0.25s ease, opacity 0.25s ease;
}

.heart-pop-enter-from {
    transform: scale(0.3);
    opacity: 0.4;
}
</style>
