<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useChatStore } from '@/stores/chat';
import V3Emoji from 'vue3-emoji';
import 'vue3-emoji/dist/style.css';
import { t } from '@/i18n';
import CardMessage from './CardMessage.vue';
import CardMessageClicked from './CardMessageClicked.vue';
import nekoHead from '@/assets/avator/neko_head.png';
import { sendChatMessageAsync, uploadFileAsync, ApiError } from "../../../../server/api/http"

const chat = useChatStore();
const user = JSON.parse(localStorage.getItem('bot-face-user') || '{}');
const userName = user.nickname || '';

// 当前好友名字
const prop = defineProps<{ name: string }>();

// 当前选中的好友
const selected = computed(() => chat.friends.find(x => x.name === prop.name));
// 当前会话的 sessionId：需要时用 selected.value?.sessionId（store 里就存着，不用从父组件传）
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
const pendingFiles = ref<{ name: string; size: string; file: File }[]>([]);

// 发送中标记：Enter 和点击按钮可能连点，防重复发送
const sending = ref(false);

// 发送：文字 + 文件一起进消息列表（都是 user 发的）
async function sendMessage() {
    if (sending.value) return;
    const content = draft.value.trim();
    // 关键：先做快照再清空。pendingFiles.value = [] 是「换一个新数组」，
    // 之后再去读 pendingFiles.value 就永远是空数组了（原来的 bug：文件既不渲染也不上传）
    const files = pendingFiles.value.slice();
    // 发请求前记下这次要发去哪个会话：等待期间用户切了好友，回复也得落回原来那个会话
    const targetName = prop.name;
    if (!content && files.length === 0) return;

    sending.value = true;
    // 清空输入框和待发列表（文件本体已经在上面的 files 快照里，不会丢）
    draft.value = '';
    pendingFiles.value = [];

    try {
        // ① 先传文件：/api/File/file1 只把文件正文拼进消息池，不调 AI。
        //    必须先传完再触发下面那轮 chat1，AI 才能在同一轮里看到「文件 + 文字」
        let uploaded = 0;
        for (const item of files) {
            // 先把「我发的这条文件」放进消息列表 —— 界面上立刻能看到文件气泡，
            // 之后再等上传结果；失败则额外追加一条提示，气泡本身不会丢
            chat.addChatList(targetName, 'user', item.name, 'file', item.size);
            try {
                const code = await uploadFileAsync(targetName, userName, item.file);
                if (code === 200) {
                    uploaded++;
                } else {
                    console.error('上传文件失败:', code);
                    chat.addChatList(targetName, targetName, '上传文件失败，请稍后再试');
                }
            } catch (err) {
                console.error('上传文件失败:', err);
                // 把后端给的具体原因带出来（缺字段/会话无效/用户不存在…），
                // 不然界面上永远只有一句「请稍后再试」，日志里也只有 undefined
                const reason = err instanceof ApiError ? err.message : '请稍后再试';
                chat.addChatList(targetName, targetName, `上传文件失败：${reason}`);
            }
        }

        // 只有文件、且一个都没进池子 → 没有能触发的东西，到此为止
        if (!content && uploaded === 0) return;

        // ② 文字气泡（文件之后，顺序和用户操作意图一致）
        if (content) chat.addChatList(targetName, 'user', content);

        // ③ 整个回合只触发这一次 AI：
        //    有文字 → 正常发文字；没文字 → type='file' + 空正文（后端对空正文不落库，只当触发器）
        //    这样文件+文字只有一条回复，AI 也能一次看全
        try {
            const { replyMessage } = await sendChatMessageAsync(
                targetName,
                userName,
                content,
                content ? 'text' : 'file'
            );
            // 注：消息落盘由 store 里 watch(friends, { deep: true }) 统一负责（addChatList 一追加就写回）
            chat.addChatList(targetName, targetName, replyMessage || '出现错误，请稍后再试');
        } catch (err) {
            console.error('发送消息失败:', err);
            chat.addChatList(targetName, targetName, '出现错误，请稍后再试');
        }
    } finally {
        sending.value = false;
    }
}

// 资源管理器这一块：选文件 → 存进待发送列表
function onFileSelect(e: Event) {
    const input = e.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
        pendingFiles.value.push({
            name: file.name,
            size: file.size > 1024 * 1024
                ? (file.size / 1024 / 1024).toFixed(1) + ' MB'
                : (file.size / 1024).toFixed(1) + ' KB',
            file: file
        });
    }
    input.value = '';
}

// 删除待发送文件
function removeFile(index: number) {
    pendingFiles.value.splice(index, 1);
}

// 切换好友：清掉上一个会话还没发出去的文件，避免「选中了文件 → 切好友 → 发给了另一个人」
// （ChatPage 没有按 name 加 key，切好友时是同一个实例，pendingFiles 会留着）
watch(() => prop.name, () => {
    pendingFiles.value = [];
});

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

                    <!-- 文件消息：卡片样式（text 是文件名，fileSize 是体积；内容只在后端/AI 侧） -->
                    <div v-else-if="msg.type === 'file'"
                        class="flex items-center gap-2 border-white/20 bg-white/90 backdrop-blur-2xl shadow-2xl border w-fit max-w-full rounded-sm px-2 py-1">
                        <fv-icon-element value="Document" :font-size="16" />
                        <div class="flex flex-col min-w-0">
                            <span class="text-sm text-gray-800 break-all">{{ msg.text }}</span>
                            <span class="text-xs text-gray-400">{{ t('chat.fileTag')
                                }}<template v-if="msg.fileSize"> · {{ msg.fileSize }}</template></span>
                        </div>
                    </div>

                    <!-- 文本消息（有 i18nKey 走语言包，切语言跟随；普通消息走 text；老消息无 type 也走这里） -->
                    <div v-else
                        class="border-white/20 bg-white/90 backdrop-blur-2xl shadow-2xl  border w-fit rounded-sm px-1 py-0.5">
                        <span class="text-gray-800">{{ msg.i18nKey ? t(msg.i18nKey) : msg.text }}</span>
                    </div>
                </div>
            </TransitionGroup>
        </div>

        <!-- 待发送文件卡片区 -->
        <div v-if="pendingFiles.length" class="flex flex-col gap-2 px-6 pt-2 border-t border-gray-200/80">
            <div v-for="(f, idx) in pendingFiles" :key="idx"
                class="flex items-center justify-between bg-white/80 backdrop-blur-3xl border border-white/30 rounded-sm px-3 py-2">
                <div class="flex items-center gap-2">
                    <fv-icon-element value="Document" :font-size="16" />
                    <div class="flex flex-col">
                        <span class="text-sm text-gray-800">{{ f.name }}</span>
                        <span class="text-xs text-gray-400">{{ f.size }}</span>
                    </div>
                </div>
                <fv-icon-element value="Cancel" :font-size="14" class="cursor-pointer hover:opacity-50"
                    @click="removeFile(idx)" />
            </div>
        </div>

        <!-- 底部输入框（官方账号的会话不显示输入框；用稳定标识 officialId 比较，不受语言影响） -->
        <div v-if="prop.name !== chat.officialId"
            class="flex flex-row items-center relative gap-2 px-20 py-3 border-t border-gray-200/80  backdrop-blur-lg">

            <!-- 文本输入框和发送按钮 -->
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

            <!-- 文件资源管理器-->
            <input type="file" ref="fileInput" class="hidden" @change="onFileSelect" accept="
                .txt,.text,text/plain,
                .cs,.java,.js,.ts,.py,.cpp,.c,.h,.php,.rb,.go,.rs,.swift,.kt,.dart,.scala,.pl,.lua,.r,.m,.sql,.sh,.bash,.ps1,
                .html,.htm,.css,.scss,.sass,.less,.vue,.jsx,.tsx,.json,.xml,.svg,
                .json,.xml,.yaml,.yml,.toml,.ini,.cfg,.conf,.config,.properties,.env,.gitignore,.dockerignore,.npmrc,.babelrc,.eslintrc,.prettierrc,
                .md,.markdown,.rst,.adoc,.asciidoc,.tex,.latex,
                .log,.csv,.tsv,.dat,.out
            " />
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
