<script setup lang="ts">
import { t } from '@/i18n';

const prop = defineProps<{ name: string, avator: string }>()
</script>

<template>
    <div class="flex flex-col rounded border w-fit min-w-xs p-4 bg-white/90 backdrop-blur-2xl shadow">
        <!-- 卡片标题（i18n） -->
        <span class="text-gray-500 text-xs">{{ t('card.contactLabel') }}</span>
        <div class="flex flex-row gap-10 items-center border-t border-gray-500">
            <div class=" overflow-hidden rounded-full w-17 h-17">
                <!-- 头像 -->
                <img :src=prop.avator alt="占位头像" class="w-full h-full object-center object-cover" />
            </div>
            <span class="text-gray-700 text-xl">{{ prop.name }}</span>
        </div>
    </div>
</template>