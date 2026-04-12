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
        console.log('SignalR Connected')
        connection.on('ReceiveNotification', (message) => {
          // Add to store
          addNotification({ 
            message, 
            type: 'info' 
          })

          // Show toast
          toast.success(message, {
            icon: '🔔',
            duration: 5000
          })
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
