import { useEffect, useState } from 'react'
import { Calendar, CreditCard, TrendingUp, Clock } from 'lucide-react'
import { portalApi } from '../../api/portalApi'
import { PageLoader, StatusBadge } from '../../components/ui'
import useAuthStore from '../../store/authStore'

export default function PortalDashboard() {
  const { user } = useAuthStore()
  const [stats, setStats] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    portalApi.getDashboard()
      .then((r) => setStats(r.data))
      .catch(() => {})
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <PageLoader />

  const fmtDate = (d) => (d ? new Date(d).toLocaleDateString('sq-AL') : '—')
  const fmtTime = (t) => (typeof t === 'string' ? t.substring(0, 5) : '—')

  return (
    <div>
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Mirë se erdhët, {user?.firstName}! 👋</h1>
        <p className="text-sm text-gray-500 mt-1">Pasqyra e aktivitetit tuaj në Wellness House</p>
      </div>

      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        {[
          { icon: Calendar, label: 'Terminet Total', value: stats?.totalTerminet ?? 0, color: 'bg-blue-500' },
          { icon: Clock, label: 'Termine Aktive', value: stats?.terminetAktive ?? 0, color: 'bg-green-600' },
          { icon: CreditCard, label: 'Anëtarësimi', value: stats?.anetaresimiAktiv ? 'Aktiv' : 'Jo aktiv', color: 'bg-purple-500' },
          { icon: TrendingUp, label: 'Shpenzuar', value: `€${Number(stats?.totalShpenzuar ?? 0).toFixed(2)}`, color: 'bg-emerald-500' },
        ].map(({ icon: Icon, label, value, color }) => (
          <div key={label} className="card p-4 flex items-start gap-3">
            <div className={`w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0 ${color}`}>
              <Icon className="w-5 h-5 text-white" />
            </div>
            <div>
              <p className="text-xl font-bold text-gray-900">{value}</p>
              <p className="text-xs text-gray-500">{label}</p>
            </div>
          </div>
        ))}
      </div>

      {stats?.terminIArdhshem && (
        <div className="card p-5 mb-6 border-l-4 border-green-500">
          <h3 className="text-sm font-semibold text-gray-900 mb-3 flex items-center gap-2">
            <Clock className="w-4 h-4 text-green-600" />
            Termini i Ardhshëm
          </h3>
          <div className="flex items-center justify-between">
            <div>
              <p className="font-semibold text-gray-900">{stats.terminIArdhshem.sherbimEmri}</p>
              <p className="text-sm text-gray-500">Terapist: {stats.terminIArdhshem.terapistEmri}</p>
              <p className="text-sm text-gray-500">
                {fmtDate(stats.terminIArdhshem.dataTerminit)} • {fmtTime(stats.terminIArdhshem.oraFillimit)} - {fmtTime(stats.terminIArdhshem.oraMbarimit)}
              </p>
            </div>
            <StatusBadge status={stats.terminIArdhshem.statusi} />
          </div>
        </div>
      )}

      <div className="card p-5">
        <h3 className="text-sm font-semibold text-gray-900 mb-4">Veprime të Shpejta</h3>
        <div className="grid grid-cols-2 gap-3">
          {[
            { label: '📅 Rezervo Termin', path: '/portal/rezervo', color: 'bg-green-50 text-green-700 hover:bg-green-100' },
            { label: '⭐ Shto Vlerësim', path: '/portal/vlereisimet', color: 'bg-yellow-50 text-yellow-700 hover:bg-yellow-100' },
            { label: '🛒 Bli Produkt', path: '/portal/produktet', color: 'bg-blue-50 text-blue-700 hover:bg-blue-100' },
            { label: '👤 Profili Im', path: '/portal/profili', color: 'bg-purple-50 text-purple-700 hover:bg-purple-100' },
          ].map(({ label, path, color }) => (
            <a
              key={path}
              href={path}
              className={`flex items-center justify-center px-4 py-3 rounded-xl text-sm font-medium transition-all ${color}`}
            >
              {label}
            </a>
          ))}
        </div>
      </div>
    </div>
  )
}
