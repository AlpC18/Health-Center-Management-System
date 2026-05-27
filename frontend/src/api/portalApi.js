import api from './api'

export const portalApi = {
  getDashboard: () => api.get('/portal/dashboard'),
  getProfili: () => api.get('/portal/profili'),
  updateProfili: (data) => api.put('/portal/profili', data),
  getTerminet: (statusi) => api.get(`/portal/terminet${statusi ? '?statusi=' + statusi : ''}`),
  quoteTermin: (sherbimId) => api.get(`/portal/terminet/quote?sherbimId=${sherbimId}`),
  createTermin: (data) => api.post('/portal/terminet', data),
  annulTermin: (id) => api.delete(`/portal/terminet/${id}`),
  approveReschedule: (id) => api.post(`/portal/terminet/${id}/reschedule/approve`, {}),
  declineReschedule: (id) => api.post(`/portal/terminet/${id}/reschedule/decline`, {}),
  getAnetaresimi: () => api.get('/portal/anetaresimi'),
  getSherbimet: () => api.get('/portal/sherbimet'),
  getTerapistet: () => api.get('/portal/terapistet'),
  getPaketat: () => api.get('/portal/paketat'),
  quotePaketa: (paketId) => api.get(`/portal/paketat/${paketId}/quote`),
  getProduktet: () => api.get('/portal/produktet'),
  blejProdukt: (data) => api.post('/portal/produktet/blej', data),
  getShitjet: () => api.get('/portal/shitjet'),
  getVlereisimet: () => api.get('/portal/vlereisimet'),
  addVleresim: (data) => api.post('/portal/vlereisimet', data),
  getProgramet: () => api.get('/portal/programet'),
}
