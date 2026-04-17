<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '../state/user'

const router = useRouter()
const userStore = useUserStore()

// Змінні для полів вводу
const email = ref('')
const password = ref('')

const handleLogin = () => {
  // Поки що без реального бекенду просто імітуємо логін
  if (email.value && password.value) {
    userStore.login(email.value) // Зберігаємо в Pinia
    router.push('/')             // Перекидаємо на Головну сторінку
  }
}
</script>

<template>
  <div>
    <h2>Вхід в систему</h2>
    <form @submit.prevent="handleLogin">
      <div>
        <label>Email: </label>
        <input v-model="email" type="email" required />
      </div>
      <br>
      <div>
        <label>Пароль: </label>
        <input v-model="password" type="password" required />
      </div>
      <br>
      <button type="submit">Увійти</button>
    </form>
  </div>
</template>
