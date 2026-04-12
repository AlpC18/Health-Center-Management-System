import { create } from 'zustand'
import { persist } from 'zustand/middleware'

const useAuthStore = create(
  persist(
    (set, get) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      expiresAt: null,

      setAuth: ({ user, accessToken, refreshToken, expiresAt }) =>
        set({ user, accessToken, refreshToken, expiresAt }),

      setAccessToken: (accessToken) => set({ accessToken }),

      clearAuth: () =>
        set({ user: null, accessToken: null, refreshToken: null, expiresAt: null }),

      isAuthenticated: () => {
        const { accessToken } = get()
        return !!accessToken
      },
      isAdmin: () => get().user?.role === 'Admin',
      isKlient: () => get().user?.role === 'Klient',
    }),
    { name: 'wellness-auth' }
  )
)

export default useAuthStore
