import { useEffect } from 'react'
import * as signalR from '@microsoft/signalr'
import toast from 'react-hot-toast'
import useAuthStore from '../store/authStore'
import useNotificationStore from '../store/notificationStore'

const SignalRListener = () => {
  const token = useAuthStore((s) => s.accessToken)
  const addNotification = useNotificationStore((s) => s.addNotification)

  useEffect(() => {
    if (!token) return

    const connection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5077/notificationHub', {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build()

    connection
      .start()
      .then(() => {
        connection.on('ReceiveNotification', (message) => {
          addNotification({ message, type: 'info' })
          toast.success(message, { icon: '🔔', duration: 5000 })
        })

        connection.on('NewAppointment', (data) => {
          const msg = data?.message ?? 'Termin i ri u shtua.'
          addNotification({ message: msg, type: 'appointment' })
          toast.success(msg, { icon: '📅', duration: 4000 })
        })

        connection.on('NewReview', (data) => {
          const msg = data?.message ?? 'Vlerësim i ri u shtua.'
          addNotification({ message: msg, type: 'review' })
          toast.success(msg, { icon: '⭐', duration: 4000 })
        })

        connection.on('LowStock', (data) => {
          const msg = data?.message ?? 'Stok i ulët!'
          addNotification({ message: msg, type: 'warning' })
          toast(msg, { icon: '⚠️', duration: 6000, style: { background: '#78350f', color: '#fef3c7' } })
        })
      })
      .catch((err) => console.error('SignalR Connection Error: ', err))

    return () => {
      connection.stop()
    }
  }, [token, addNotification])

  return null
}

export default SignalRListener
