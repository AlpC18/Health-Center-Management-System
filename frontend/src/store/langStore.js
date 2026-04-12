import { create } from 'zustand'
import { persist } from 'zustand/middleware'

const useLangStore = create(
  persist(
    (set) => ({
      lang: 'sq',
      toggleLang: () => set((s) => ({ lang: s.lang === 'sq' ? 'en' : 'sq' })),
      setLang: (lang) => set({ lang }),
    }),
    { name: 'wellness-lang' }
  )
)

export default useLangStore
