import { create } from 'zustand'

const useAuthStore = create((set, get) => ({
  user: null,
  accessToken: null,
  expiresAt: null,

  setAuth: ({ user, accessToken, expiresAt }) =>
    set({ user, accessToken, expiresAt }),

  setAccessToken: (accessToken) => set({ accessToken }),

  clearAuth: () =>
    set({ user: null, accessToken: null, expiresAt: null }),

  isAuthenticated: () => {
    const { accessToken } = get()
    return !!accessToken
  },
  isAdmin: () => get().user?.role === 'Admin',
  isKlient: () => get().user?.role === 'Klient',
}))

export default useAuthStore
