import { useState, useRef, useEffect } from 'react'
import { Bell, CheckCircle2, Trash2, Clock, Inbox } from 'lucide-react'
import useNotificationStore from '../../store/notificationStore'

const formatTimeAgo = (date) => {
  const diff = new Date() - new Date(date)
  const seconds = Math.floor(diff / 1000)
  if (seconds < 60) return 'tani'
  const minutes = Math.floor(seconds / 60)
  if (minutes < 60) return `${minutes}m më parë`
  const hours = Math.floor(minutes / 60)
  if (hours < 24) return `${hours}h më parë`
  const days = Math.floor(hours / 24)
  return `${days}d më parë`
}

export default function NotificationCenter() {
  const [isOpen, setIsOpen] = useState(false)
  const dropdownRef = useRef(null)
  const { notifications, unreadCount, markAsRead, markAllAsRead, clearAll } = useNotificationStore()

  useEffect(() => {
    function handleClickOutside(event) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        setIsOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  return (
    <div className="relative" ref={dropdownRef}>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="relative p-2 text-health-secondary hover:text-health-primary hover:bg-health-hover rounded-xl transition-all group"
      >
        <Bell className="w-5 h-5 group-hover:rotate-12 transition-transform" />
        {unreadCount > 0 && (
          <span className="absolute top-1.5 right-1.5 w-4 h-4 bg-health-brand text-white text-[10px] font-bold flex items-center justify-center rounded-full border-2 border-health-surface animate-bounce-short">
            {unreadCount > 9 ? '9+' : unreadCount}
          </span>
        )}
      </button>

      {isOpen && (
        <div className="absolute right-0 mt-3 w-80 md:w-96 bg-health-surface border border-health-border rounded-2xl shadow-2xl z-[100] overflow-hidden animate-in fade-in slide-in-from-top-2 duration-200">
          {/* Header */}
          <div className="px-5 py-4 border-b border-health-border bg-health-surface/50 flex items-center justify-between">
            <div>
              <h3 className="text-sm font-bold text-health-primary uppercase tracking-wider">Njoftimet</h3>
              <p className="text-[10px] text-health-secondary font-medium tracking-tight mt-0.5">
                Keni {unreadCount} njoftime të paoxuara
              </p>
            </div>
            {notifications.length > 0 && (
              <div className="flex gap-2">
                <button
                  onClick={markAllAsRead}
                  className="p-1.5 text-health-accent hover:bg-health-accent/10 rounded-lg transition-colors"
                  title="Lexo të gjitha"
                >
                  <CheckCircle2 className="w-4 h-4" />
                </button>
                <button
                  onClick={clearAll}
                  className="p-1.5 text-health-brand/60 hover:text-health-brand hover:bg-health-brand/10 rounded-lg transition-colors"
                  title="Pastro të gjitha"
                >
                  <Trash2 className="w-4 h-4" />
                </button>
              </div>
            )}
          </div>

          {/* List */}
          <div className="max-h-[400px] overflow-y-auto">
            {notifications.length === 0 ? (
              <div className="py-12 flex flex-col items-center justify-center text-health-secondary">
                <Inbox className="w-10 h-10 opacity-20 mb-3" />
                <p className="text-xs font-bold uppercase tracking-widest opacity-40">Nuk ka njoftime</p>
              </div>
            ) : (
              <div className="divide-y divide-health-border/50">
                {notifications.map((n) => (
                  <div
                    key={n.id}
                    onClick={() => markAsRead(n.id)}
                    className={`px-5 py-4 cursor-pointer transition-all hover:bg-health-hover relative group ${
                      !n.isRead ? 'bg-health-accent/5' : ''
                    }`}
                  >
                    {!n.isRead && (
                      <div className="absolute left-0 top-0 bottom-0 w-1 bg-health-accent" />
                    )}
                    <div className="flex gap-3">
                      <div className={`w-8 h-8 rounded-full flex items-center justify-center flex-shrink-0 ${
                        !n.isRead ? 'bg-health-accent/20 text-health-accent' : 'bg-health-bg text-health-secondary'
                      }`}>
                        <Bell className="w-4 h-4" />
                      </div>
                      <div className="flex-1">
                        <p className={`text-xs leading-relaxed ${
                          !n.isRead ? 'text-health-primary font-bold' : 'text-health-secondary'
                        }`}>
                          {n.message}
                        </p>
                        <div className="flex items-center gap-1.5 mt-2 text-[10px] text-health-secondary/60">
                          <Clock className="w-3 h-3" />
                          <span>
                            {formatTimeAgo(n.timestamp)}
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Footer */}
          {notifications.length > 0 && (
            <div className="px-5 py-3 border-t border-health-border bg-health-bg/50 text-center">
              <button
                onClick={() => setIsOpen(false)}
                className="text-[10px] font-bold text-health-secondary uppercase tracking-widest hover:text-health-primary transition-colors"
              >
                Mbyll listën
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
