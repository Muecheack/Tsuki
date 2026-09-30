import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import VueFluent from "@creatorsn/vfluent3";
import '@creatorsn/vfluent3/style.css';
import App from './App.vue'
import router from './router'

createApp(App).use(createPinia()).use(VueFluent).use(router).mount('#app')
