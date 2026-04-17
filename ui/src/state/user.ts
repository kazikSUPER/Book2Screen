import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useUserStore = defineStore('user', () => {
  // Стан (State)
  const isAuthenticated = ref(false)
  const email = ref('')

  // Дії (Actions)
  function login(userEmail: string) {
    isAuthenticated.value = true
    email.value = userEmail
  }

  function logout() {
    isAuthenticated.value = false
    email.value = ''
  }

  return { isAuthenticated, email, login, logout }
})

