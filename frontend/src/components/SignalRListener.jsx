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
        const pushToast = (data, fallback, type = 'info') => {
          const msg = data?.message ?? data?.Message ?? fallback
          addNotification({ message: msg, type })
          toast(
            (t) => (
              <div className="flex items-start gap-3">
                <div className="min-w-0">
                  <p className="text-sm font-semibold">{data?.title ?? data?.Title ?? fallback}</p>
                  <p className="text-xs opacity-80 mt-1">{msg}</p>
                </div>
                {(data?.link || data?.Link) && (
                  <button
                    onClick={() => {
                      toast.dismiss(t.id)
                      window.location.href = data.link || data.Link
                    }}
                    className="text-xs font-bold text-blue-300 hover:text-blue-200"
                  >
                    Hap
                  </button>
                )}
              </div>
            ),
            { duration: 7000 }
          )
        }

        connection.on('NotificationCreated', (data) => {
          const msg = data?.message ?? data?.Message ?? 'Njoftim i ri'
          addNotification({ message: msg, type: data?.type ?? 'info' })
        })

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

        connection.on('RescheduleProposed', (data) => {
          pushToast(data, 'Propozim i ri per ndryshim termini', 'reschedule')
        })

        connection.on('RescheduleApproved', (data) => {
          pushToast(data, 'Ndryshimi i terminit u aprovua', 'reschedule')
        })

        connection.on('RescheduleDeclined', (data) => {
          pushToast(data, 'Ndryshimi i terminit u refuzua', 'reschedule')
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
