import { createApp } from 'vue'
import { createPinia } from 'pinia'
import './style.css'
import App from './App.vue'
import router from './router' // <-- Додали імпорт

const app = createApp(App)
const pinia = createPinia() // <-- Створили екземпляр Pinia

app.use(pinia)  // <-- Підключили Pinia
app.use(router) // <-- Підключили роутер
app.mount('#app')
