<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useChatStore } from '@/stores/chat';
import V3Emoji from 'vue3-emoji';
import 'vue3-emoji/dist/style.css';
import { t } from '@/i18n';
import CardMessage from './CardMessage.vue';
import CardMessageClicked from './CardMessageClicked.vue';
import nekoHead from '@/assets/avator/neko_head.png';
import { sendChatMessageAsync, uploadFileAsync } from "../../../../server/api/http"
import { formatFileSize, readTextPreview, TEXT_FILE_ACCEPT } from '@/utils/file';
import FileMessage from './FileMessage.vue';

const chat = useChatStore();
const user = JSON.parse(localStorage.getItem('bot-face-user') || '{}');
const userName = user.nickname || '';

// 当前好友名字
const prop = defineProps<{ name: string }>();

console.log(`当前好友名:${prop.name}`)

// 当前选中的好友
const selected = computed(() => chat.friends.find(x => x.name === prop.name));
// 当前会话的 sessionId：需要时用 selected.value?.sessionId（store 里就存着，不用从父组件传）
// 注：这里不要再起名叫 curSessionId —— 会和上面 prop 的同名字段撞，模板/脚本里看到的不是同一个东西
// 显示名：官方有 nameKey（走语言包，切语言实时跟随），其它好友直接显示 name
const displayName = computed(() => selected.value?.nameKey ? t(selected.value.nameKey) : prop.name);

// 卡片消息：点击后弹的中央面板开关 + 当前点的卡片数据
const showCardClicked = ref(false);
const clickedCard = ref<{ name: string; avator: string } | null>(null);


//卡片数据, 头像, 标签
const draftData = computed(() => chat.draft[0])
const tags = computed<string[]>(() => draftData.value?.[1] ?? [])
const avatarUrl = computed(() => draftData.value?.[2] ?? '')

/* 卡片是否可渲染：官方会话依赖 draft 人设数据（头像/标签），其它好友不依赖；
 未到达时整个消息区显示加载态（而不是单独给卡片加占位）*/
const chatReady = computed(() => prop.name !== chat.officialId || !!draftData.value)

/* 卡片头像：draft 带来的头像，没有就默认猫猫头
 ''会触发alt才用||
 注：必须定义在下面的 watch 之前 —— watch 开了 immediate，setup 期间就会同步执行回调*/
const cardAvator = computed(() => avatarUrl.value || nekoHead)

// 调试：draft 变化时打印读到的内容 + 最终给卡片的头像 URL（排查 alt 占位图用）
watch(
    () => chat.draft,
    (v) => {
        console.log('[draft] ChatPage 读到:', JSON.parse(JSON.stringify(v)))
        console.log('[draft] cardAvator =', cardAvator.value)
    },
    { deep: true, immediate: true }
)

function addTags(tags: string[]) {
    chat.personTages = (tags ?? []).map(x => ({ text: x, type: x }));
}
// 打开卡片详情
function openCard(name: string) {
    clickedCard.value = {
        name,
        avator: cardAvator.value
    }
    showCardClicked.value = true
}

const fileInput = ref<HTMLInputElement | null>(null);

// 输入框内容
const draft = ref('');

// 点选 emoji：追加到输入框
function onEmoji(val: string) {
    draft.value += val;
}

// 待发送的文件（显示在输入框上方）
// 注：这里必须留住 File 本体 —— 之前只存了 name/size 两个字符串，File 在 input 清空后就丢了，
// 所以"发送文件"永远只能往消息列表塞一句文本占位，根本没发出去
interface IPendingFile {
    id: number;
    file: File;
    name: string;
    size: string;
    state: 'waiting' | 'sending' | 'failed';
}
const pendingFiles = ref<IPendingFile[]>([]);
let pendingFileId = 0;

// 待发卡片上那行小字：上传中 / 失败原因 / 正常时显示体积
function pendingHint(f: IPendingFile): string {
    if (f.state === 'sending') return t('chat.uploading');
    if (f.state === 'failed') return t('chat.uploadFail');
    return f.size;
}

// 发送：文字 + 文件一起进消息列表（都是 user 发的）
// 文字走 text；文件先真上传（POST /api/File/upload）拿到下载地址，再以 type='file' 发出去 ——
// 后端把 Content 存成 JSON，并把这里截断好的 preview 翻成人话喂给 AI（见 cs 的 MapRoleContent）
async function sendMessage() {
    const content = draft.value.trim();
    // 快照待发列表：下面的 await 期间用户还能继续往里加文件，不该被这一次发送带走
    const queue = pendingFiles.value.slice();
    draft.value = '';
    if (!content && queue.length === 0) return;

    // 发请求前记下这次要发去哪个会话：等待期间用户切了好友，回复也得落回原来那个会话
    const targetName = prop.name;

    // 发送文字
    if (content) {
        chat.addChatList(targetName, 'user', content);
        // 注：消息落盘由 store 里 watch(friends, { deep: true }) 统一负责（addChatList 一追加就写回）
        try {
            const { replyMessage } = await sendChatMessageAsync(targetName, userName, content)
            chat.addChatList(targetName, targetName, replyMessage ?? t('chat.sendFail'));
        } catch (err) {
            console.error('发送消息失败:', err);
            chat.addChatList(targetName, targetName, t('chat.sendFail'));
        }
    }

    // 发送文件：一个一个来 —— 每发一条后端都会回一条 AI 消息，并发发出去回复顺序会乱
    for (const item of queue) {
        item.state = 'sending';
        try {
            const uploaded = await uploadFileAsync(item.file);
            // 文本类文件读一份截断预览：它既显示在气泡里，也是唯一会进 AI 上下文的内容
            const preview = await readTextPreview(item.file);
            const payload = JSON.stringify({
                name: uploaded.name,
                size: uploaded.size,
                url: uploaded.url,
                preview
            });
            chat.addChatList(targetName, 'user', `${t('chat.fileTag')} ${uploaded.name}`, 'file', {
                ...uploaded,
                preview
            });
            const { replyMessage } = await sendChatMessageAsync(targetName, userName, payload, 'file');
            chat.addChatList(targetName, targetName, replyMessage ?? t('chat.sendFail'));
            // 成功才从待发列表移除；失败的留在列表里标红，下次点发送会重试
            pendingFiles.value = pendingFiles.value.filter(f => f.id !== item.id);
        } catch (err) {
            console.error('发送文件失败:', err);
            item.state = 'failed';
        }
    }
}

// 资源管理器这一块：选文件 → 存进待发送列表（multiple，一次能选多个）
function onFileSelect(e: Event) {
    const input = e.target as HTMLInputElement;
    for (const file of Array.from(input.files ?? [])) {
        pendingFiles.value.push({
            id: ++pendingFileId,
            file,
            name: file.name,
            size: formatFileSize(file.size),
            state: 'waiting'
        });
    }
    input.value = '';
}

// 删除待发送文件
function removeFile(index: number) {
    pendingFiles.value.splice(index, 1);
}

// 消息区滚动容器
const msgContainer = ref<HTMLElement | null>(null);

// 新消息进来时自动滚到底部
function scrollToBottom() {
    if (msgContainer.value) {
        msgContainer.value.scrollTop = msgContainer.value.scrollHeight;
    }
}

// 监听当前好友的消息数量变化 → 自动滚动到底部
watch(
    () => selected.value?.message.length,
    () => {
        // 等 Vue 渲染完新气泡再滚
        requestAnimationFrame(scrollToBottom);
    }
);
</script>

<style scoped>
/* 气泡淡入：只改透明度，不位移 */
.bubble-enter-from {
    opacity: 0;
}

.bubble-enter-active {
    transition: opacity 0.4s ease;
}

.bubble-enter-to {
    opacity: 1;
}
</style>

<template>
    <!-- 单纯底板 -->
    <div class="flex-1  flex flex-col justify-between border rounded-sm">

        <!-- 顶部显示卡片 -->
        <div
            class="flex flex-row justify-between px-6 bg-white/5 backdrop-blur-2xl items-center border-b border-white h-11 border">
            <span class="font-[serif] text-[#1A1A1A]">{{ displayName }}</span>

            <!-- 更多icon -->
            <div class="group relative cursor-pointer">
                <fv-icon-element value="More" :font-size="17" class="cursor-pointer w-1" />
                <div class="absolute right-full mr-2 top-1/2 -translate-y-1/2 whitespace-nowrap
                            bg-white/70 backdrop-blur-2xl border border-white/30 rounded-sm px-2 py-1 text-xs text-gray-700
                            opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none">
                    {{ t('chat.more') }}
                </div>
            </div>
        </div>

        <!-- 消息展示区 -->
        <!-- 循环展示气泡，sender === 'user' 靠右，否则靠左 -->
        <!-- 加载态：只有官方会话依赖 draft 人设数据（（头像/标签），其它好友不依赖；
             数据未到达时整个消息区显示加载中，避免先渲染出空标签/默认头像再切换 -->
        <div v-if="!chatReady" class="flex-1 flex flex-col items-center justify-center gap-2 border-b">
            <fv-progress-ring :legacy="true" />
            <span class="text-gray-200 text-sm">{{ t('chat.loading') }}</span>
        </div>

        <div v-else ref="msgContainer"
            class="flex flex-col flex-1 backdrop-blur-xs border-b overflow-y-auto px-6 w-full">
            <TransitionGroup name="bubble" tag="div" class="flex flex-col">
                <div v-for="(msg, idx) in selected?.message" :key="idx"
                    :class="['flex flex-col w-fit p-2 max-w-2xl min-w-0', msg.sender === 'user' ? 'self-end' : 'self-start']">
                    <!-- 名字：自己显示"我"，对方显示好友名 -->
                    <label class="text-gray-500 text-xs">{{ msg.sender === 'user' ? t('chat.me') : displayName
                        }}</label>

                    <!-- 卡片消息：渲染卡片，点击弹出中央面板详情 -->
                    <!-- 开发阶段msg.sender是KeKe -->
                    <div v-if="msg.type === 'card'" class="cursor-pointer min-w-[20%]"
                        @click="addTags(tags); openCard(msg.sender)">
                        <CardMessage :name="msg.sender" :avator="cardAvator" />
                    </div>

                    <!-- 文件消息：库里存的是 JSON，气泡里渲染成文件卡片（可下载 + 可展开截断预览） -->
                    <FileMessage v-else-if="msg.file" :file="msg.file" />

                    <!-- 文本消息（有 i18nKey 走语言包，切语言跟随；普通消息走 text；老消息无 type 也走这里） -->
                    <div v-else
                        class="border-white/20 bg-white/90 backdrop-blur-2xl shadow-2xl  border w-fit rounded-sm px-1 py-0.5">
                        <span class="text-gray-800">{{ msg.i18nKey ? t(msg.i18nKey) : msg.text }}</span>
                    </div>
                </div>
            </TransitionGroup>
        </div>

        <!-- 待发送文件卡片区：上传中转圈，失败标红（失败项留在列表里，再点发送即重试） -->
        <div v-if="pendingFiles.length" class="flex flex-col gap-2 px-6 pt-2 border-t border-gray-200/80">
            <div v-for="(f, idx) in pendingFiles" :key="f.id"
                class="flex items-center justify-between bg-white/60 backdrop-blur-sm border border-white/30 rounded-sm px-3 py-2">
                <div class="flex items-center gap-2 min-w-0">
                    <fv-progress-ring v-if="f.state === 'sending'" :legacy="true" class="shrink-0" />
                    <fv-icon-element v-else value="Document" :font-size="16" class="shrink-0" />
                    <div class="flex flex-col min-w-0">
                        <span class="text-sm text-gray-800 truncate">{{ f.name }}</span>
                        <span class="text-xs" :class="f.state === 'failed' ? 'text-red-400' : 'text-gray-400'">
                            {{ pendingHint(f) }}
                        </span>
                    </div>
                </div>
                <fv-icon-element value="Cancel" :font-size="14" class="cursor-pointer hover:opacity-50 shrink-0"
                    @click="removeFile(idx)" />
            </div>
        </div>

        <!-- 底部输入框（官方账号的会话不显示输入框；用稳定标识 officialId 比较，不受语言影响） -->
        <div v-if="prop.name !== chat.officialId"
            class="flex flex-row items-center relative gap-2 px-20 py-3 border-t border-gray-200/80  backdrop-blur-lg">

            <fv-text-box v-model="draft" :placeholder="t('chat.placeholder')" class="flex-1 text-gray-500"
                @keyup.enter="sendMessage" />
            <fv-button icon="Send" class="text-gray-800 shrink-0 cursor-pointer" @click="sendMessage">{{ t('chat.send')
            }}</fv-button>

            <!-- 表情和文件icon -->
            <div class="flex flex-row gap-4 absolute right-3">
                <!-- 表情板：点图标弹出，选中追加到输入框 -->
                <V3Emoji size="small" :recent="true" @click-emoji="onEmoji" class="cursor-pointer">
                    <template #default>
                        <fv-icon-element value="Emoji" :font-size="17" class="hover:opacity-50" />
                    </template>
                </V3Emoji>
                <fv-icon-element value="FolderOpen" :font-size="17" class="cursor-pointer hover:opacity-50"
                    @click="fileInput?.click()" />
            </div>

            <!-- 文件资源管理器：扩展名白名单集中在 utils/file.ts，multiple 支持一次选多个 -->
            <input type="file" ref="fileInput" class="hidden" multiple :accept="TEXT_FILE_ACCEPT"
                @change="onFileSelect" />
        </div>

        <!-- ClickedCard -->
        <fv-panel v-model="showCardClicked" :is-central-side="true" :teleport="true" :is-acrylic="true"
            width="calc(20% - 50px)" height="calc(70% - 50px)" content-padding="15px" title="">
            <template v-slot:container>
                <CardMessageClicked v-if="clickedCard" :name="clickedCard.name" :avator="clickedCard.avator"
                    @close="showCardClicked = false" />
            </template>
        </fv-panel>
    </div>
</template>
