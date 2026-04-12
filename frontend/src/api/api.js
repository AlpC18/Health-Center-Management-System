import axios from 'axios'
import useAuthStore from '../store/authStore'

const BASE_URL = 'http://localhost:5077/api'

const api = axios.create({ baseURL: BASE_URL })

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

let isRefreshing = false
let failedQueue = []

const processQueue = (error, token = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error)
    } else {
      prom.resolve(token)
    }
  })
  failedQueue = []
}

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        })
          .then((token) => {
            originalRequest.headers.Authorization = `Bearer ${token}`
            return api(originalRequest)
          })
          .catch((err) => Promise.reject(err))
      }

      originalRequest._retry = true
      isRefreshing = true

      const { refreshToken, setAccessToken, clearAuth } = useAuthStore.getState()

      try {
        const res = await axios.post(`${BASE_URL}/auth/refresh`, { refreshToken })
        const newToken = res.data.accessToken
        setAccessToken(newToken)
        processQueue(null, newToken)
        originalRequest.headers.Authorization = `Bearer ${newToken}`
        return api(originalRequest)
      } catch (refreshError) {
        processQueue(refreshError, null)
        clearAuth()
        window.location.href = '/login'
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }

    return Promise.reject(error)
  }
)

const crudApi = (endpoint) => ({
  getAll: (params) => api.get(params ? `${endpoint}?${params}` : endpoint),
  getById: (id) => api.get(`${endpoint}/${id}`),
  create: (data) => api.post(endpoint, data),
  update: (id, data) => api.put(`${endpoint}/${id}`, data),
  delete: (id) => api.delete(`${endpoint}/${id}`),
})

export const authApi = {
  login: (data) => api.post('/auth/login', data),
  register: (data) => api.post('/auth/register', data),
  logout: () => api.post('/auth/logout'),
}

export const klientetApi = {
  ...crudApi('/klientet'),
  uploadFoto: (id, file) => {
    const formData = new FormData()
    formData.append('file', file)
    return api.post(`/klientet/${id}/foto`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
  }
}
export const sherbiimetApi = crudApi('/sherbimet')
export const terapistetApi = crudApi('/terapistet')
export const anetaresimetApi = {
  ...crudApi('/anetaresimet'),
  getMy: () => api.get('/anetaresimet/my')
}
export const paketaApi = crudApi('/paketawellness')
export const programetApi = crudApi('/programet')
export const produktetApi = crudApi('/produktet')
export const shitjetApi = {
  ...crudApi('/shitjet'),
  getMy: () => api.get('/shitjet/my')
}
export const vlereiisimetApi = {
  ...crudApi('/vlereisimet'),
  getMy: () => api.get('/vlereisimet/my')
}

export const terminetApi = {
  ...crudApi('/terminet'),
  getMy: () => api.get('/terminet/my')
}

export const dashboardApi = {
  getStats: () => api.get('/dashboard/stats'),
  getAnalytics: () => api.get('/dashboard/analytics'),
}

export const auditlogsApi = {
  getAll: (params) => api.get(params ? `/auditlogs?${params}` : '/auditlogs'),
  getRecent: (limit = 20) => api.get(`/auditlogs?limit=${limit}`),
}

export default api

export const portalApi = {
  getDashboard: () => api.get('/portal/dashboard'),
  getProfili: () => api.get('/portal/profili'),
  updateProfili: (data) => api.put('/portal/profili', data),
  getTerminet: (statusi) => api.get(`/portal/terminet${statusi ? '?statusi=' + statusi : ''}`),
  createTermin: (data) => api.post('/portal/terminet', data),
  annulTermin: (id) => api.delete(`/portal/terminet/${id}`),
  getAnetaresimi: () => api.get('/portal/anetaresimi'),
  getSherbimet: () => api.get('/portal/sherbimet'),
  getTerapistet: () => api.get('/portal/terapistet'),
  getPaketat: () => api.get('/portal/paketat'),
  getProduktet: () => api.get('/portal/produktet'),
  blejProdukt: (data) => api.post('/portal/produktet/blej', data),
  getShitjet: () => api.get('/portal/shitjet'),
  getVlereisimet: () => api.get('/portal/vlereisimet'),
  addVleresim: (data) => api.post('/portal/vlereisimet', data),
  getProgramet: () => api.get('/portal/programet'),
}
