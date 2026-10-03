<script setup lang="ts">
import { getCurrentInstance } from 'vue';
import { createMessageBar } from '@creatorsn/vfluent3';
import { t } from '@/i18n';
import { resolveServerUrl } from '../../../../server/api/http';
import { formatFileSize } from '@/utils/file';
import type { IFileAttachment } from '@/stores/chat';

const prop = defineProps<{ file: IFileAttachment }>()

// appContext 必须在 setup 顶层取（createMessageBar 需要它继承主题）；setup 内实例必然存在
const appContext = getCurrentInstance()!.appContext

// 下载：先抓成 blob 再触发下载
// 直接给 <a href download> 是跨域的，浏览器会忽略 download 属性变成"打开新窗口"；
// blob URL 是同源的才拦得住，而且在 Electron 里也不会弹新窗口
async function download() {
    if (!prop.file.url) return
    try {
        const response = await fetch(resolveServerUrl(prop.file.url))
        if (!response.ok) throw new Error(`HTTP ${response.status}`)
        const blobUrl = URL.createObjectURL(await response.blob())
        const a = document.createElement('a')
        a.href = blobUrl
        a.download = prop.file.name || 'download'
        document.body.appendChild(a)
        a.click()
        a.remove()
        URL.revokeObjectURL(blobUrl)
    } catch (err) {
        console.error('下载文件失败:', err)
        createMessageBar(t('chat.downloadFail'), { status: 'error', once: true, autoClose: 2000 }, appContext)
    }
}
</script>

<template>
    <div
        class="flex flex-col gap-1 border border-white/20 bg-white/90 backdrop-blur-2xl shadow-2xl rounded-sm px-2 py-1.5 w-fit max-w-md">
        <div class="flex flex-row items-center gap-2">
            <fv-icon-element value="Document" :font-size="20" class="shrink-0 text-gray-600" />

            <div class="flex flex-col min-w-0">
                <span class="text-sm text-gray-800 truncate">{{ prop.file.name }}</span>
                <span class="text-xs text-gray-400">{{ formatFileSize(prop.file.size) }}</span>
            </div>

            <!-- 有下载地址才给下载图标（没传完/上传失败时没有 url） -->
            <fv-icon-element v-if="prop.file.url" value="Download" :font-size="16"
                class="cursor-pointer hover:opacity-50 shrink-0" @click="download" />
        </div>

        <!-- 截断预览：就是发给 AI 的那份内容，方便用户核对 AI 到底"看"到了什么 -->
        <details v-if="prop.file.preview" class="border-t border-gray-200 pt-1">
            <summary class="text-xs text-gray-500 cursor-pointer">{{ t('chat.viewFileContent') }}</summary>
            <pre
                class="text-xs text-gray-600 whitespace-pre-wrap break-all max-h-40 overflow-y-auto">{{ prop.file.preview }}</pre>
        </details>
    </div>
</template>
