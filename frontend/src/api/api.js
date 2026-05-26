import axios from 'axios'
import useAuthStore from '../store/authStore'

const BASE_URL = (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5077/api').replace(/\/+$/, '')

if (import.meta.env.PROD && BASE_URL.startsWith('http://')) {
  throw new Error('Production ortaminda HTTPS API URL zorunludur (VITE_API_BASE_URL).')
}

const api = axios.create({
  baseURL: BASE_URL,
  withCredentials: true,
})

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

      const { setAccessToken, clearAuth } = useAuthStore.getState()

      try {
        const res = await api.post('/auth/refresh', {})
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
  logout: () => api.post('/auth/logout', {}),
  forgotPassword: (data) => api.post('/auth/forgot-password', data),
  validateResetToken: (token) => api.get(`/auth/reset-password/${encodeURIComponent(token)}/validate`),
  resetPassword: (data) => api.post('/auth/reset-password', data),
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
export const klientProgrametApi = crudApi('/klientprogramet')
export const produktetApi = crudApi('/produktet')
export const shitjetApi = {
  ...crudApi('/shitjet'),
  getMy: () => api.get('/shitjet/my'),
  updatePaymentStatus: (id, statusiPageses) => api.patch(`/shitjet/${id}/pagesa`, { statusiPageses }),
}
export const vlereiisimetApi = {
  ...crudApi('/vlereisimet'),
  getMy: () => api.get('/vlereisimet/my')
}

export const terminetApi = {
  ...crudApi('/terminet'),
  getMy: () => api.get('/terminet/my')
}

// Build a query string for optional date-range params.
const dateRangeQs = (from, to) => {
  const p = new URLSearchParams()
  if (from) p.set('from', from)
  if (to) p.set('to', to)
  const s = p.toString()
  return s ? `?${s}` : ''
}

export const dashboardApi = {
  getStats: ({ from, to } = {}) => api.get(`/dashboard/stats${dateRangeQs(from, to)}`),
  getAnalytics: ({ from, to } = {}) => api.get(`/dashboard/analytics${dateRangeQs(from, to)}`),
  getLowStock: (threshold = 10) => api.get(`/dashboard/low-stock?threshold=${threshold}`),
}

export const reportsApi = {
  getAnalytics: () => api.get('/reports/analytics'),
  getKlientetPdf: () => api.get('/reports/klientet-pdf', { responseType: 'blob' }),
}

export const advancedApi = {
  getAvailability: (params) => api.get('/advanced/appointments/availability', { params }),
  cancelAppointment: (id, data) => api.post(`/advanced/appointments/${id}/cancel`, data),
  getWaitingList: () => api.get('/advanced/waiting-list'),
  addWaitingList: (data) => api.post('/advanced/waiting-list', data),
  updatePackageServices: (id, data) => api.post(`/advanced/packages/${id}/services`, data),
  createSaleWithStock: (data) => api.post('/advanced/sales', data),
  updatePaymentStatus: (id, data) => api.patch(`/advanced/sales/${id}/payment-status`, data),
  mockPayment: (data) => api.post('/advanced/payments/mock', data),
  getLowStock: () => api.get('/advanced/inventory/low-stock'),
  getExpiringMemberships: (days = 14) => api.get(`/advanced/memberships/expiring?days=${days}`),
  getNotifications: () => api.get('/advanced/notifications'),
  createNotification: (data) => api.post('/advanced/notifications', data),
  moderateReview: (id, data) => api.patch(`/advanced/reviews/${id}/moderation`, data),
  getReports: () => api.get('/advanced/reports'),
  getTherapistPerformance: () => api.get('/advanced/reports/therapists'),
  getClientRetention: () => api.get('/advanced/reports/client-retention'),
  getRecommendations: (clientId) => api.get(`/advanced/recommendations/${clientId}`),
  createBackup: () => api.post('/advanced/backup', {}),
}

export const auditlogsApi = {
  getAll: (params) => api.get(params ? `/auditlogs?${params}` : '/auditlogs'),
  getRecent: (limit = 20) => api.get(`/auditlogs?limit=${limit}`),
}

// Additional CRUDs (Sallat, Furnizuesit, Lajmerimet, Zbritjet, Pushimet)
export const sallatApi = crudApi('/sallat')
export const furnizuesitApi = crudApi('/furnizuesit')
export const lajmerimetApi = crudApi('/lajmerimet')
export const zbritjetApi = crudApi('/zbritjet')
export const pushimetApi = crudApi('/pushimet')

// ── Phase-2 wellness extensions ─────────────────────────────────────────────
// Clinical notes (per-visit anamnesis log)
export const klientShenimeApi = {
  ...crudApi('/klientshenime'),
  forKlient: (klientId) => api.get(`/klientshenime?klientId=${klientId}&limit=200`),
  forTermin: (terminId) => api.get(`/klientshenime?terminId=${terminId}&limit=50`),
}

// Body measurements
export const klientMatjetApi = {
  ...crudApi('/klientmatjet'),
  forKlient: (klientId) => api.get(`/klientmatjet?klientId=${klientId}&limit=200`),
}

// Loyalty points ledger
export const klientPikatApi = {
  list: (klientId) => api.get(`/klientpikat?klientId=${klientId}&limit=200`),
  balance: (klientId) => api.get(`/klientpikat/balance/${klientId}`),
  create: (data) => api.post('/klientpikat', data),
}

// Therapist self-service portal
export const therapistPortalApi = {
  me: () => api.get('/terapist-portal/me'),
  mySchedule: (from, to) => {
    const p = new URLSearchParams()
    if (from) p.set('from', from)
    if (to) p.set('to', to)
    const qs = p.toString()
    return api.get(`/terapist-portal/my-schedule${qs ? '?' + qs : ''}`)
  },
  myClients: () => api.get('/terapist-portal/my-clients'),
  completeAppointment: (terminId) => api.post(`/terapist-portal/appointments/${terminId}/complete`),
}

// Recurring termin booking
export const recurringTerminetApi = {
  create: (data) => api.post('/terminet/recurring', data),
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

// Boot-time session restore using the httpOnly refresh cookie.
// Bare axios call (no interceptors) so a missing/expired cookie simply rejects
// instead of triggering the 401 refresh-and-redirect flow.
export const refreshSession = () =>
  axios.post(`${BASE_URL}/auth/refresh`, {}, { withCredentials: true })
