import api from './api'

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
