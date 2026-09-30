<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { t, currentLang, setLang } from '@/i18n';

// 父组件传进来的当前背景图信息
const props = defineProps<{
    bgImagePath: string;
}>();

// 背景图选择器 ref
const bgInput = ref<HTMLInputElement | null>(null);

// 语言选项表（语言名用各自自称，不翻译）
const setOptions = ref([
    { key: 'zh', text: '猫猫语(简体中文)' },
    { key: 'en', text: 'English' },
    { key: 'ja', text: '日本語' },
    { key: 'meow', text: '正经中文' }
]) as any

// 语言下拉当前值（初始化成上次选择的语言）
const setValue = ref<any>(setOptions.value.find((o: any) => o.key === currentLang.value));

// 语言切换：fv-combobox 的 v-model 是 option 对象，取 key 调 setLang
watch(setValue, (v: any) => {
    if (v?.key) setLang(v.key);
});

// 主题选项表（文案随语言切换，用 computed）
const themeOptions = computed(() => [
    { key: 'light', text: t('settings.light') },
    { key: 'dark', text: t('settings.dark') }
]) as any

// 点击设置的主题的值
const themeValue = ref('');

// 选择背景图：把文件名发给父组件，父组件负责替换背景
function onBgSelect(e: Event) {
    const input = e.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    // 用 FileReader 读成 base64（data: 开头的 URL，CSP 允许）
    const reader = new FileReader();
    reader.onload = () => {
        emit('bg-change', { path: file.name, url: reader.result as string });
    };
    reader.readAsDataURL(file);
    input.value = '';
}

const emit = defineEmits<{
    (e: 'bg-change', payload: { path: string; url: string }): void;
}>();
</script>

<template>
    <div class="flex flex-col gap-2 overflow-y-auto h-full">
        <!-- 设置语言 -->
        <div class="flex flex-row items-center gap-8">
            <span class="text-gray-700">{{ t('settings.language') }}</span>
            <fv-combobox v-model="setValue" :options="setOptions" :placeholder="t('settings.langPlaceholder')" />
        </div>

        <!-- 设置主题 -->
        <div class="flex flex-row items-center gap-8">
            <span class="text-gray-700">{{ t('settings.theme') }}</span>
            <fv-combobox v-model="themeValue" :options="themeOptions" :placeholder="t('settings.themePlaceholder')" />
        </div>

        <!-- 背景图像切换 -->
        <div class="flex flex-row gap-2">
            <span class="text-gray-700">{{ t('settings.background') }}</span>
            <div class="flex flex-row items-center gap-2">
                <!-- 显示框：显示选中的图片路径 -->
                <div
                    class="flex-1 bg-white/50 backdrop-blur-sm border border-white/30 rounded-sm px-2 py-1 text-xs text-gray-600 truncate">
                    {{ props.bgImagePath || t('settings.defaultBg') }}
                </div>
                <!-- 按钮：点击打开资源管理器 -->
                <div
                    class="flex border bg-white/70 backdrop-blur-2xl border-white/30 p-1 rounded cursor-pointer hover:opacity-50">
                    <fv-icon-element value="More" :font-size="17" class="shrink-0"
                        @click="bgInput?.click()"></fv-icon-element>
                </div>
            </div>
            <!-- 隐藏的文件选择器（只选图片）-->
            <input ref="bgInput" type="file" accept="image/*" class="hidden" @change="onBgSelect" />
        </div>

        <!-- 吐槽闲话，所有要添加的组件在这上面写 -->
        <span class="text-gray-700 text-lg">{{ t('settings.footer') }}</span>
    </div>
</template>
