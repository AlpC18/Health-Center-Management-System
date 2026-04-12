import { create } from 'zustand'
import { persist } from 'zustand/middleware'

const useNotificationStore = create(
  persist(
    (set) => ({
      notifications: [],
      unreadCount: 0,
      
      addNotification: (notification) => set((state) => {
        const newNotification = {
          id: Date.now(),
          timestamp: new Date(),
          isRead: false,
          ...notification
        }
        return {
          notifications: [newNotification, ...state.notifications].slice(0, 50),
          unreadCount: state.unreadCount + 1
        }
      }),

      markAsRead: (id) => set((state) => ({
        notifications: state.notifications.map(n => 
          n.id === id ? { ...n, isRead: true } : n
        ),
        unreadCount: Math.max(0, state.unreadCount - 1)
      })),

      markAllAsRead: () => set((state) => ({
        notifications: state.notifications.map(n => ({ ...n, isRead: true })),
        unreadCount: 0
      })),

      clearAll: () => set({ notifications: [], unreadCount: 0 })
    }),
    {
      name: 'health-notifications'
    }
  )
)

export default useNotificationStore
