<template>
    <div class="w-full h-screen flex flex-col items-center justify-center bg-cover bg-center"
        :style="{ backgroundImage: `url(${nekoUrl})` }">

        <!-- 注册卡片 -->
        <transition name="card-fade" appear>
            <div
                class="flex flex-col rounded-2xl py-8 px-50 justify-center items-center gap-2 bg-white/70 border shadow border-white/30">

                <!-- 猫猫头像 -->
                <div class="w-12 h-12 overflow-hidden rounded-full">
                    <img :src="nekoHeadUrl" alt="avatar" class="w-full h-full object-cover object-center" />
                </div>

                <span class="text-[#1A1A1A] text-2xl">{{ t('register.title') }}</span>
                <span class="text-red-300 text-sm">{{ t('register.subtitleBefore') }}<span
                        class="font-serif italic">KeKe</span>{{ t('register.subtitleAfter') }}</span>

                <!-- 性别：圆角方框单选卡，横向排列；默认男孩 -->
                <div class="flex flex-col gap-1 w-full">
                    <label class="text-[#1A1A1A] text-sm">{{ t('register.gender') }}</label>
                    <div class="flex flex-row justify-between items-center">
                        <div @click="gender = 'boy'"
                            class="px-4 py-1.5 rounded-md border cursor-pointer select-none transition-colors"
                            :class="gender === 'boy' ? 'bg-blue-200/70 border-blue-300' : 'bg-white/50 border-white/30 hover:bg-white/70'">
                            {{ t('register.boy') }}
                        </div>
                        <span class="text-gray-700">{{ t('register.canNotSay') }}</span>
                        <div @click="gender = 'girl'"
                            class="px-4 py-1.5 rounded-md border cursor-pointer select-none transition-colors"
                            :class="gender === 'girl' ? 'bg-pink-200/70 border-pink-300' : 'bg-white/50 border-white/30 hover:bg-white/70'">
                            {{ t('register.girl') }}
                        </div>
                    </div>
                </div>

                <!-- 昵称 -->
                <div class="flex flex-col gap-1 w-full">
                    <label class="text-[#1A1A1A] text-sm">{{ t('register.nickname') }}</label>
                    <fv-text-box :placeholder="t('register.nicknamePlaceholder')" v-model="nameRegister"
                        class="w-full text-gray-500" />
                    <span v-if="nameIsNullOrWhiteSpace" class="text-red-300 text-xs">{{ t('register.nicknameEmpty')
                    }}</span>
                    <span v-if="userExist" class="text-red-300 text-xs">{{ t('register.userExist') }}</span>
                </div>



                <!-- 称呼：下拉框，选项随性别切换（男孩→哥哥/正太；女孩→姐姐/萝莉） -->
                <div class="flex flex-col gap-1 w-full">
                    <label class="text-[#1A1A1A] text-sm">{{ t('register.call') }}</label>
                    <fv-combobox v-model="callValue" :options="callOptions" :placeholder="t('register.placeholder')"
                        class="w-full" />
                    <span v-if="callValueNull" class="text-xs text-red-300">{{ t('register.callValueNull') }}</span>
                </div>

                <!-- api_key输入框 -->
                <div class="flex flex-col gap-1 w-full">
                    <label class="text-[#1A1A1A] text-sm">{{ t('register.apiKey') }}</label>
                    <fv-text-box type="password" :placeholder="t('register.apiKeyInputPlaceholder')"
                        class="w-full text-gray-500" v-model="apiKeyInput" />
                    <span v-if="apiKeyNull" class="text-red-300 text-xs">{{ t('register.apiKeyNull') }}</span>
                </div>

                <!-- 注册按钮 -->
                <!-- 测试阶段用goHome -->
                <!-- 点一次就禁用（submitting）：防重复提交；请求结束（失败）后恢复可点，成功则跳走 -->
                <fv-button :boderRadius="2" :disabled="submitting" @click="register"
                    :class="['text-[#1A1A1A]', submitting ? 'cursor-not-allowed opacity-60' : 'cursor-pointer']">{{
                    t('register.submit') }}</fv-button>


            </div>
        </transition>
    </div>
</template>

<style scoped>
.card-fade-enter-active {
    transition: opacity 0.35s ease, transform 0.35s ease;
}

.card-fade-enter-from {
    opacity: 0;
    transform: translateY(8px);
}
</style>

<script setup lang="ts">
import nekoUrl from '@/assets/neko.png?url'
import nekoHeadUrl from '@/assets/avator/neko_head.png'
import { useRouter } from 'vue-router'
import { ref, computed, watch } from 'vue'
import { t } from '@/i18n'
import { sendUserInfoAsync, ApiError, preAiPerson } from "../../../../server/api/http"
import { useChatStore, AI_PERSON_NAME } from '@/stores/chat'

const chat = useChatStore()

const router = useRouter()
const nameRegister = ref('')
const nameIsNullOrWhiteSpace = ref(false)
const userExist = ref(false) // 昵称已被占用（后端返回 code 400）
const callValueNull = ref(false)
const apiKeyInput = ref('')
const apiKeyNull = ref(false)
// 注册请求进行中：按钮置灰禁用，点了就不再响应（防重复提交）
const submitting = ref(false)

// 性别：默认男孩；称呼下拉框的选项随它切换
const gender = ref<'boy' | 'girl'>('boy')
const callValue = ref<any>(null) // 称呼下拉当前值
const callOptions = computed(() =>
    gender.value === 'boy'
        ? [
            { key: 'olderBrother', text: t('register.olderBrother') },
            { key: 'shota', text: t('register.shota') }
        ]
        : [
            { key: 'olderSister', text: t('register.olderSister') },
            { key: 'loli', text: t('register.loli') }
        ]
) as any // fv-combobox 的 options 类型推导为 never[]，与 SettingPage 一致用 any 兜底

// 切性别时清掉已选称呼，避免残留上一个性别的选项
watch(gender, () => {
    callValue.value = null
})

// 判空逻辑
const isNullOrWhiteSpace = (str: string) => {
    return !str || str.trim().length === 0
}

// 注册
const register = async () => {
    /*
    判空，获取姓名，性别与称呼，发送后端
    */
    // 已在请求中：直接忽略（按钮已禁用，这里兜底防连点）
    if (submitting.value) return

    // 昵称判空
    nameIsNullOrWhiteSpace.value = isNullOrWhiteSpace(nameRegister.value)
    if (nameIsNullOrWhiteSpace.value) return

    // 称呼判空（callValue 是 fv-combobox 绑定的 option 对象 {key, text}，取 .key；直接对对象调
    // isNullOrWhiteSpace 会在 .trim() 处抛 TypeError）
    if (isNullOrWhiteSpace(callValue.value?.key)) {
        callValueNull.value = true
        return
    }
    // apiKey判空
    if (isNullOrWhiteSpace(apiKeyInput.value)) {
        apiKeyNull.value = true;
        return
    }

    // 校验都过了才真正发请求：先把按钮锁上
    submitting.value = true
    try {
        // 发送注册信息 → 拿 { id, token }（token 是免登录凭证，id 是新用户 Id）
        const { id, token } = await sendUserInfoAsync(
            nameRegister.value.trim(),
            gender.value,
            callValue.value.key,
            apiKeyInput.value.trim()
        )

        // 存凭证：免登录的依据（第二次打开应用直接进 Home）
        localStorage.setItem('bot-face-token', token ?? '')
        localStorage.setItem('bot-face-user', JSON.stringify({
            userId: id,
            nickname: nameRegister.value.trim()
        }))

        // 预发 AI 人设（官方好友的开场白/标签/头像）, pinia 供聊天卡片用
        try {
            const [aiReply, tags, avatarUrl] = await preAiPerson(nameRegister.value.trim(), AI_PERSON_NAME)
            chat.setDraft([aiReply, tags, avatarUrl])
            console.log('[draft] 注册时预发人设:', { aiReply, tags, avatarUrl })
        } catch (e) {
            console.error('[draft] 预发人设失败（不影响注册）:', e)
        }

        goHome()
    } catch (err) {
        // 后端明确返回的业务错误（带 code）
        if (err instanceof ApiError) {
            if (err.code === 400) {
                userExist.value = true // 昵称已被占用，在输入框下方提示
            } else {
                alert(`注册失败喵~：${err.message}`)
            }
            return
        }
        // 后端没起来 / 网络错误等
        alert(`注册失败喵~${err instanceof Error ? ':' + err.message : ''}`)
    } finally {
        // 失败/异常都要解锁，否则用户改完信息也没法再点；成功时已跳转，解锁无害
        submitting.value = false
    }
}

// 前往首页
const goHome = () => {
    router.push({ name: `KeKe` })
}
</script>