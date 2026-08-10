import client from './client'
import mockApi from './mockApi'

// Toggle the real backend vs the mock with VITE_USE_MOCK (default: mock).
const USE_MOCK = import.meta.env.VITE_USE_MOCK !== 'false'

const realApi = {
  async register(payload) {
    const { data } = await client.post('/auth/register', payload)
    return data
  },
  async login(payload) {
    const { data } = await client.post('/auth/login', payload)
    return data
  },
  async getMe() {
    const { data } = await client.get('/users/me')
    return data
  },
  async getTasks(filters) {
    const { data } = await client.get('/tasks', { params: filters })
    return data
  },
  async getTask(id) {
    const { data } = await client.get(`/tasks/${id}`)
    return data
  },
  async createTask(payload) {
    const { data } = await client.post('/tasks', payload)
    return data
  },
  async updateTask(id, payload) {
    const { data } = await client.put(`/tasks/${id}`, payload)
    return data
  },
  async deleteTask(id) {
    await client.delete(`/tasks/${id}`)
  },
  async getDashboardStats() {
    const { data } = await client.get('/tasks/dashboard/stats')
    return data
  },
  async getCategories() {
    const { data } = await client.get('/categories')
    return data
  },
  async getUsers() {
    const { data } = await client.get('/users')
    return data
  },
}

export default USE_MOCK ? mockApi : realApi
