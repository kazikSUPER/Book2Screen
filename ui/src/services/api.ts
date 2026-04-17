import axios from 'axios';

// Створюємо базовий клієнт
export const apiClient = axios.create({
  // Заміни на URL твого локального або тестового бекенду
  baseURL: 'http://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Інтерцептор запитів (тут пізніше будемо додавати токен)
apiClient.interceptors.request.use(
  (config) => {
    // Наприклад: const token = localStorage.getItem('token');
    // if (token) { config.headers.Authorization = `Bearer ${token}`; }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Інтерцептор відповідей (для глобальної обробки помилок)
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      console.error('Неавторизовано! Треба на сторінку логіну.');
      // Тут пізніше додамо редірект
    }
    return Promise.reject(error);
  }
);
