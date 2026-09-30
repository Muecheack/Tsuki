<script setup lang="ts">
import { useChatStore } from '@/stores/chat';
import { t } from '@/i18n';

const chat = useChatStore();
</script>

<template>
    <div class="flex flex-col w-full h-full">
        <!-- 空态：还没有申请记录，居中显示 -->
        <div v-if="!chat.friendsRequest.length"
            class="flex flex-1 flex-col gap-2 items-center justify-center text-gray-500">
            <span>{{ t('request.empty') }}</span>
        </div>

        <!-- 有记录：逐条渲染 -->
        <div v-else class="flex flex-col gap-4">
            <div v-for="(name, index) in chat.friendsRequest" :key="index" class="flex flex-col gap-4 items-center">
                <img alt="占位头像" />
                <span class="text-gray-500 text-xl">{{ name }}</span>
                <fv-icon-element value="Add" :font-size="17" />
            </div>
        </div>
    </div>
</template>