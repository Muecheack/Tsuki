import { createRouter, createWebHashHistory } from 'vue-router'
import Register_V from '@/components/Register/Register_V.vue'

const routes = [
  {
    path: '/',
    name: 'Register_V',
    component: Register_V
  },
  {
    path: '/keke',
    name: 'KeKe',
    component: () => import('@/components/KeKe/KeKePage.vue')
  }
]

const router = createRouter({
  history: createWebHashHistory(),
  routes
})

// 免登录守卫：已有凭证（token）→ 跳过注册页直接进 Home；否则放行
router.beforeEach((to) => {
  if (to.name === 'Register_V' && localStorage.getItem('bot-face-token')) {
    return { name: 'KeKe' }
  }
  return true
})

export default router
