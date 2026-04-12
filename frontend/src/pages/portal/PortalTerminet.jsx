import { useEffect, useState } from 'react'
import { Calendar, X } from 'lucide-react'
import { portalApi } from '../../api/portalApi'
import { PageLoader, StatusBadge, ConfirmDialog } from '../../components/ui'
import toast from 'react-hot-toast'

export default function PortalTerminet() {
  const [terminet, setTerminet] = useState([])
  const [loading, setLoading] = useState(true)
  const [filter, setFilter] = useState('')
  const [cancelling, setCancelling] = useState(null)
  const [cancelLoading, setCancelLoading] = useState(false)

  const fetchTerminet = () => {
    setLoading(true)
    portalApi.getTerminet(filter)
      .then((r) => setTerminet(r.data?.data || r.data || []))
      .catch(() => toast.error('Terminet nuk mund të ngarkohen.'))
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    fetchTerminet()
  }, [filter])

  const handleCancel = async () => {
    setCancelLoading(true)
    try {
      await portalApi.annulTermin(cancelling.terminId)
      toast.success('Termini u anulua.')
      setCancelling(null)
      fetchTerminet()
    } catch (err) {
      toast.error(err.response?.data?.message || 'Anulimi dështoi.')
    } finally {
      setCancelLoading(false)
    }
  }

  const fmtDate = (d) => new Date(d).toLocaleDateString('sq-AL')
  const fmtTime = (t) => (typeof t === 'string' ? t.substring(0, 5) : '—')

  if (loading) return <PageLoader />

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Terminet e Mia</h1>
          <p className="text-sm text-gray-500 mt-1">Historiku i termineve tuaja</p>
        </div>
        <a href="/portal/rezervo" className="btn-primary">
          <Calendar className="w-4 h-4" />
          Rezervo Termin
        </a>
      </div>

      <div className="flex gap-2 mb-4 flex-wrap">
        {['', 'Planifikuar', 'Konfirmuar', 'Perfunduar', 'Anuluar'].map((s) => (
          <button
            key={s}
            onClick={() => setFilter(s)}
            className={`px-3 py-1.5 text-xs font-medium rounded-lg transition-all ${
              filter === s
                ? 'bg-green-600 text-white'
                : 'bg-white text-gray-600 border border-gray-200 hover:bg-gray-50'
            }`}
          >
            {s || 'Të gjitha'}
          </button>
        ))}
      </div>

      <div className="space-y-3">
        {terminet.length === 0 ? (
          <div className="card p-8 text-center">
            <Calendar className="w-10 h-10 text-gray-300 mx-auto mb-3" />
            <p className="text-sm text-gray-500">Nuk keni termine.</p>
            <a href="/portal/rezervo" className="btn-primary mt-3 inline-flex">
              Rezervo Termin të Ri
            </a>
          </div>
        ) : (
          terminet.map((t) => (
            <div key={t.terminId} className="card p-4">
              <div className="flex items-start justify-between">
                <div className="flex items-start gap-4">
                  <div className="w-12 h-12 rounded-xl bg-green-50 flex items-center justify-center flex-shrink-0">
                    <Calendar className="w-6 h-6 text-green-600" />
                  </div>
                  <div>
                    <p className="font-semibold text-gray-900">{t.sherbimEmri}</p>
                    <p className="text-sm text-gray-500">Terapist: {t.terapistEmri}</p>
                    <p className="text-sm text-gray-500">
                      {fmtDate(t.dataTerminit)} • {fmtTime(t.oraFillimit)} - {fmtTime(t.oraMbarimit)}
                    </p>
                    {t.shenimet && <p className="text-xs text-gray-400 mt-1 italic">"{t.shenimet}"</p>}
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <StatusBadge status={t.statusi} />
                  {(t.statusi === 'Planifikuar' || t.statusi === 'Konfirmuar') && (
                    <button
                      onClick={() => setCancelling(t)}
                      className="p-1.5 text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors"
                    >
                      <X className="w-4 h-4" />
                    </button>
                  )}
                </div>
              </div>
            </div>
          ))
        )}
      </div>

      <ConfirmDialog
        isOpen={!!cancelling}
        onClose={() => setCancelling(null)}
        onConfirm={handleCancel}
        loading={cancelLoading}
        itemName={
          cancelling
            ? `${cancelling.sherbimEmri} - ${new Date(cancelling.dataTerminit).toLocaleDateString('sq-AL')}`
            : ''
        }
      />
    </div>
  )
}
