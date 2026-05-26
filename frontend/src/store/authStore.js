import { create } from 'zustand'
import { refreshSession } from '../api/api'

const useAuthStore = create((set, get) => ({
  user: null,
  accessToken: null,
  expiresAt: null,
  hydrated: false,
  _hydrating: false,

  setAuth: ({ user, accessToken, expiresAt }) =>
    set({ user, accessToken, expiresAt }),

  setAccessToken: (accessToken) => set({ accessToken }),

  clearAuth: () =>
    set({ user: null, accessToken: null, expiresAt: null }),

  // Restore the session on app boot using the httpOnly refresh cookie.
  // Keeps the access token in memory (XSS-safe) while surviving page refreshes.
  // Guarded so React StrictMode's double-effect can't rotate the token twice.
  hydrate: async () => {
    if (get().hydrated || get()._hydrating) return
    set({ _hydrating: true })
    try {
      const res = await refreshSession()
      set({
        user: res.data.user,
        accessToken: res.data.accessToken,
        expiresAt: res.data.expiresAt,
        hydrated: true,
        _hydrating: false,
      })
    } catch {
      set({ user: null, accessToken: null, expiresAt: null, hydrated: true, _hydrating: false })
    }
  },

  isAuthenticated: () => {
    const { accessToken } = get()
    return !!accessToken
  },
  isAdmin: () => get().user?.role === 'Admin',
  isKlient: () => get().user?.role === 'Klient',
}))

export default useAuthStore
