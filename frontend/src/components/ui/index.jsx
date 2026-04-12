import { useEffect, useRef } from 'react'
import { AlertCircle, AlertTriangle, CheckCircle, Info, X } from 'lucide-react'
import toast from 'react-hot-toast'
import { t } from '../../i18n'

export const notify = {
  success: (msg) => toast.success(msg, {
    duration: 3000,
    style: { background: '#161B22', color: '#F0F6FC', border: '1px solid #30363D' },
    iconTheme: { primary: '#58A6FF', secondary: '#161B22' }
  }),
  error: (msg) => toast.error(msg, {
    duration: 4000,
    style: { background: '#161B22', color: '#F0F6FC', border: '1px solid #E50914' },
    iconTheme: { primary: '#E50914', secondary: '#161B22' }
  }),
  loading: (msg) => toast.loading(msg, {
    style: { background: '#161B22', color: '#F0F6FC', border: '1px solid #30363D' }
  }),
  promise: (promise, msgs) => toast.promise(promise, {
    loading: msgs.loading || 'Duke u ruajtur...',
    success: msgs.success || 'U ruajt me sukses!',
    error: msgs.error || 'Ndodhi një gabim.'
  }),
  undo: (msg, onUndo) => toast(
    (t) => (
      <div className="flex items-center gap-3">
        <span className="text-sm">{msg}</span>
        <button
          onClick={() => { onUndo(); toast.dismiss(t.id) }}
          className="text-xs font-semibold text-blue-600 hover:underline"
        >
          Kthe
        </button>
      </div>
    ),
    { duration: 4000 }
  )
}

// Spinner
export function Spinner({ size = 'md' }) {
  const sizes = { sm: 'h-4 w-4', md: 'h-6 w-6', lg: 'h-10 w-10' }
  return (
    <div
      className={`${sizes[size]} animate-spin rounded-full border-2 border-health-border border-t-health-brand`}
    />
  )
}

// PageLoader
export function PageLoader() {
  return (
    <div className="flex items-center justify-center h-64">
      <Spinner size="lg" />
    </div>
  )
}

// Modal
export function Modal({ isOpen, onClose, title, children, size = 'md' }) {
  const modalRef = useRef(null)
  const previousFocusRef = useRef(null)

  const sizes = {
    sm: 'max-w-sm',
    md: 'max-w-lg',
    lg: 'max-w-2xl',
    xl: 'max-w-4xl',
  }

  useEffect(() => {
    if (isOpen) {
      previousFocusRef.current = document.activeElement
      modalRef.current?.focus()
    } else {
      previousFocusRef.current?.focus()
    }
  }, [isOpen])

  useEffect(() => {
    const handleKey = (e) => {
      if (e.key === 'Escape') onClose()
    }
    if (isOpen) document.addEventListener('keydown', handleKey)
    return () => document.removeEventListener('keydown', handleKey)
  }, [isOpen, onClose])

  if (!isOpen) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div
        className="absolute inset-0 bg-black/70 backdrop-blur-md"
        onClick={onClose}
      />
      <div
        ref={modalRef}
        tabIndex={-1}
        role="dialog"
        aria-modal="true"
        className={`relative w-full ${sizes[size]} bg-health-surface rounded-2xl border border-health-border shadow-2xl outline-none overflow-hidden transition-all scale-100`}
      >
        <div className="flex items-center justify-between px-8 py-6 border-b border-health-border">
          <h2 className="text-lg font-bold text-health-primary tracking-tight">{title}</h2>
          <button
            onClick={onClose}
            className="p-2 rounded-xl text-health-secondary hover:text-health-primary hover:bg-health-hover transition-all"
          >
            <X className="h-5 w-5" />
          </button>
        </div>
        <div className="px-8 py-8">{children}</div>
      </div>
    </div>
  )
}

// ConfirmDialog
export function ConfirmDialog({ isOpen, onClose, onConfirm, loading, itemName, lang = 'sq' }) {
  return (
    <Modal isOpen={isOpen} onClose={onClose} title={t(lang, 'confirmDelete')} size="sm">
      <p className="text-sm text-gray-600 dark:text-gray-400 mb-6">
        {t(lang, 'confirmText')}{' '}
        <span className="font-medium text-gray-900 dark:text-white">{itemName}</span>? {t(lang, 'cannotUndo')}
      </p>
      <div className="flex gap-3 justify-end">
        <button className="btn-secondary" onClick={onClose} disabled={loading}>
          {t(lang, 'cancel')}
        </button>
        <button className="btn-danger" onClick={onConfirm} disabled={loading}>
          {loading ? <Spinner size="sm" /> : null}
          {t(lang, 'delete')}
        </button>
      </div>
    </Modal>
  )
}

// EmptyState
export function EmptyState({ icon: Icon, title, description, action }) {
  return (
    <div className="flex flex-col items-center justify-center py-20 text-center">
      {Icon && (
        <div className="p-6 bg-health-surface border border-health-border rounded-2xl mb-6 shadow-xl">
          <Icon className="h-10 w-10 text-health-secondary/40" />
        </div>
      )}
      <h3 className="text-base font-bold text-health-primary mb-2">{title}</h3>
      {description && <p className="text-sm text-health-secondary max-w-sm mx-auto mb-8 leading-relaxed">{description}</p>}
      {action}
    </div>
  )
}

// StatusBadge
const statusMap = {
  Aktiv: 'badge-green',
  Planifikuar: 'badge-blue',
  Konfirmuar: 'badge-green',
  Anuluar: 'badge-red',
  Perfunduar: 'badge-gray',
  Skaduar: 'badge-yellow',
}

export function StatusBadge({ status }) {
  const cls = statusMap[status] ?? 'badge-gray'
  return <span className={cls}>{status}</span>
}

// Field
export function Field({ label, error, required, children }) {
  return (
    <div>
      {label && (
        <label className="label">
          {label}
          {required && <span className="text-red-500 ml-0.5">*</span>}
        </label>
      )}
      {children}
      {error && <p className="mt-1 text-xs text-red-500">{error}</p>}
    </div>
  )
}

// Alert
const alertStyles = {
  success: {
    cls: 'bg-health-accent/10 border-health-accent/20 text-health-accent',
    Icon: CheckCircle,
  },
  error: { cls: 'bg-health-brand/10 border-health-brand/20 text-health-brand', Icon: AlertCircle },
  warning: {
    cls: 'bg-orange-500/10 border-orange-500/20 text-orange-400',
    Icon: AlertTriangle,
  },
  info: { cls: 'bg-health-accent/10 border-health-accent/20 text-health-accent', Icon: Info },
}

export function Alert({ type = 'info', message }) {
  const { cls, Icon } = alertStyles[type] ?? alertStyles.info
  return (
    <div className={`flex items-start gap-4 px-6 py-4 rounded-xl border text-sm font-medium ${cls}`}>
      <Icon className="h-5 w-5 flex-shrink-0" />
      <span className="leading-relaxed">{message}</span>
    </div>
  )
}

// Skeleton
export function Skeleton({ className, count = 1 }) {
  return (
    <>
      {Array.from({ length: count }).map((_, i) => (
        <div
          key={i}
          className={`bg-health-surface border border-health-border animate-pulse rounded-xl ${className}`}
        />
      ))}
    </>
  )
}
