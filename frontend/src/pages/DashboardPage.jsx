import { useState, useEffect, useCallback } from 'react'
import { Link } from 'react-router-dom'
import {
  Users,
  Calendar,
  CreditCard,
  TrendingUp,
  UserCheck,
  ShoppingBag,
  Star,
  Activity,
  Plus,
  RefreshCw,
  Heart,
  ChevronLeft,
  ChevronRight,
  Phone,
  Settings,
} from 'lucide-react'
import {
  BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer,
  PieChart, Pie, Cell, Legend,
} from 'recharts'
import { dashboardApi, terminetApi, klientetApi } from '../api/api'
import useActivityFeed from '../hooks/useActivityFeed'
import { Spinner } from '../components/ui/index'
import useLangStore from '../store/langStore'
import { t } from '../i18n'

const COLORS = ['#16a34a', '#2563eb', '#d97706', '#9333ea', '#db2777', '#6b7280']

const statCards = [
  {
    key: 'totalKlientet',
    labelKey: 'totalClients',
    icon: Users,
    color: 'text-health-accent',
    bg: 'bg-health-accent/10',
  },
  {
    key: 'terminetSot',
    labelKey: 'appointmentsToday',
    icon: Calendar,
    color: 'text-orange-400',
    bg: 'bg-orange-400/10',
    sub: 'totalTerminet',
  },
  {
    key: 'anetaresimiAktiv',
    labelKey: 'activeMemberships',
    icon: CreditCard,
    color: 'text-health-accent',
    bg: 'bg-health-accent/10',
  },
  {
    key: 'teDheratMujore',
    labelKey: 'monthlyRevenue',
    icon: TrendingUp,
    color: 'text-health-brand',
    bg: 'bg-health-brand/10',
    format: (v) => (v != null ? `€${Number(v).toFixed(2)}` : '-'),
  },
  {
    key: 'terapistetAktiv',
    labelKey: 'activeTherapists',
    icon: UserCheck,
    color: 'text-health-accent',
    bg: 'bg-health-accent/10',
  },
  {
    key: 'produktetNeStok',
    labelKey: 'productsInStock',
    icon: ShoppingBag,
    color: 'text-health-accent',
    bg: 'bg-health-accent/10',
  },
  {
    key: 'notaMesatare',
    labelKey: 'averageRating',
    icon: Star,
    color: 'text-yellow-400',
    bg: 'bg-yellow-400/10',
    format: (v) => (v != null ? `${Number(v).toFixed(1)} / 5` : '-'),
  },
  {
    key: 'sistemAktiv',
    labelKey: 'systemActive',
    icon: Activity,
    color: 'text-health-accent',
    bg: 'bg-health-accent/10',
  },
]

const quickLinks = [
  { to: '/klientet', labelKey: 'newClient', icon: Users, color: 'text-health-accent', bg: 'bg-health-accent/10' },
  { to: '/terminet', labelKey: 'newAppointment', icon: Calendar, color: 'text-health-accent', bg: 'bg-health-accent/10' },
  { to: '/anetaresimet', labelKey: 'newMembership', icon: CreditCard, color: 'text-health-accent', bg: 'bg-health-accent/10' },
  { to: '/shitjet', labelKey: 'newSale', icon: ShoppingBag, color: 'text-health-accent', bg: 'bg-health-accent/10' },
]

function StatusBadge({ statusi }) {
  const colors = {
    Konfirmuar: 'badge-accent',
    Planifikuar: 'badge-accent',
    Anuluar: 'badge-brand',
    Përfunduar: 'text-health-secondary bg-health-surface border-health-border border px-3 py-1 rounded-full text-[10px] uppercase font-bold',
  }
  return <span className={colors[statusi] ?? 'badge-accent'}>{statusi}</span>
}

export default function DashboardPage() {
  const [stats, setStats] = useState(null)
  const [loading, setLoading] = useState(true)
  const [analytics, setAnalytics] = useState({ trends: [], services: [] })
  const [terminet, setTerminet] = useState([])
  const [klientet, setKlientet] = useState([])
  const { lang } = useLangStore()

  const fetchAll = useCallback(() => {
    setLoading(true)
    Promise.allSettled([
      dashboardApi.getStats(),
      dashboardApi.getAnalytics(),
      terminetApi.getAll(),
      klientetApi.getAll(),
    ]).then(([statsRes, analyticsRes, terminetRes, klientetRes]) => {
      if (statsRes.status === 'fulfilled') setStats(statsRes.value.data)
      if (analyticsRes.status === 'fulfilled') setAnalytics(analyticsRes.value.data)

      if (terminetRes.status === 'fulfilled') {
        const items = terminetRes.value.data?.data ?? terminetRes.value.data ?? []
        setTerminet(items.slice(-5).reverse())
      }

      if (klientetRes.status === 'fulfilled') {
        const items = klientetRes.value.data?.data ?? klientetRes.value.data ?? []
        setKlientet(items.slice(-5).reverse())
      }
    }).finally(() => setLoading(false))
  }, [])

  useEffect(() => { fetchAll() }, [fetchAll])

  return (
    <div className="space-y-8">
      {/* Header Toolbar */}
      <div className="flex items-center justify-between mb-4">
        <div>
          <h1 className="text-3xl font-black text-health-primary tracking-tighter">Dashboard</h1>
          <p className="text-sm text-health-secondary font-medium tracking-tight">Mirë se vini në qendrën tuaj të menaxhimit.</p>
        </div>
        <button onClick={fetchAll} className="btn-secondary flex items-center gap-2 px-6 py-3 rounded-2xl shadow-sm hover:shadow-md transition-all">
          <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
          Rifresko Dashboard
        </button>
      </div>

      {/* NEW: Organ Donation & Appointment Sections */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Left Column: Hero Banner */}
        <div className="lg:col-span-8 flex flex-col gap-6">
          <div className="relative overflow-hidden rounded-[2rem] bg-gradient-to-br from-[#120404] via-[#1a0606] to-[#0B0E14] border border-health-brand/10 p-10 min-h-[280px] flex flex-col justify-center shadow-2xl">
            <div className="relative z-10 max-w-lg">
              <h2 className="text-5xl font-extrabold text-white mb-4 tracking-tighter leading-none">
                Organ <br />
                <span className="text-health-brand">Donation</span>
              </h2>
              <p className="text-health-secondary text-base mb-8 leading-relaxed font-medium">
                Organ Donation is Unconditional Love. Let's Donate <br /> Organs..!
              </p>
              <button className="bg-white text-black px-10 py-3.5 rounded-2xl font-bold text-sm hover:translate-y-[-2px] hover:shadow-xl hover:shadow-health-brand/20 active:translate-y-0 transition-all flex items-center gap-3 w-fit group">
                 <Heart className="w-5 h-5 fill-health-brand text-health-brand group-hover:scale-110 transition-transform" />
                 Donate Now
              </button>
            </div>
            {/* Decorative elements */}
            <div className="absolute top-0 right-0 w-[400px] h-[400px] bg-health-brand/5 blur-[120px] -mr-32 -mt-32 rounded-full" />
            <div className="absolute bottom-0 right-0 w-32 h-32 bg-health-brand/10 blur-[60px] rounded-full mr-20 mb-10" />
          </div>

          <div className="card p-8 min-h-[250px] relative transition-all hover:border-health-accent/20">
            <div className="flex items-center justify-between mb-8">
              <h3 className="text-xl font-bold text-health-primary">Visits</h3>
              <div className="flex gap-2">
                 <button className="p-2 bg-health-bg border border-health-border rounded-lg hover:bg-health-hover transition-colors">
                   <ChevronLeft className="w-4 h-4 text-health-secondary" />
                 </button>
                 <button className="p-2 bg-health-bg border border-health-border rounded-lg hover:bg-health-hover transition-colors">
                   <ChevronRight className="w-4 h-4 text-health-secondary" />
                 </button>
              </div>
            </div>
            <div className="absolute inset-0 flex flex-col items-center justify-center pointer-events-none opacity-40">
              <div className="w-16 h-16 rounded-3xl bg-health-surface flex items-center justify-center border border-health-border mb-4">
                 <Plus className="w-8 h-8 text-health-secondary" />
              </div>
              <p className="text-xs font-bold text-health-secondary uppercase tracking-widest">No recent visits to display</p>
            </div>
          </div>
        </div>

        {/* Right Column: Mini Cards & Calendar */}
        <div className="lg:col-span-4 flex flex-col gap-6">
          {/* Quick Info Grid */}
          <div className="grid grid-cols-2 gap-4">
            <div className="card p-5 group hover:bg-health-hover cursor-pointer transition-all">
               <div className="w-10 h-10 rounded-xl bg-red-500/10 flex items-center justify-center mb-3 group-hover:scale-110 transition-transform">
                 <Heart className="w-5 h-5 text-health-brand" />
               </div>
               <h4 className="text-xs font-bold text-health-primary mb-1">Organ Donation</h4>
               <p className="text-[10px] text-health-secondary leading-tight opacity-60">Organ Donation is Unconditional Love...</p>
            </div>
            <div className="card p-5 group hover:bg-health-hover cursor-pointer transition-all">
               <div className="w-10 h-10 rounded-xl bg-orange-400/10 flex items-center justify-center mb-3 group-hover:scale-110 transition-transform">
                 <Star className="w-5 h-5 text-orange-400" />
               </div>
               <h4 className="text-xs font-bold text-health-primary mb-1">My Favorites</h4>
               <p className="text-[10px] text-health-secondary leading-tight opacity-60">Your frequently used menus.</p>
            </div>
            <div className="card p-5 group hover:bg-health-hover cursor-pointer transition-all">
               <div className="w-10 h-10 rounded-xl bg-blue-500/10 flex items-center justify-center mb-3 group-hover:scale-110 transition-transform">
                 <Phone className="w-5 h-5 text-blue-500" />
               </div>
               <h4 className="text-xs font-bold text-health-primary mb-1">Add Person</h4>
               <p className="text-[10px] text-health-secondary leading-tight opacity-60">Priority people for emergency.</p>
            </div>
            <div className="card p-5 group hover:bg-health-hover cursor-pointer transition-all">
               <div className="w-10 h-10 rounded-xl bg-gray-500/10 flex items-center justify-center mb-3 group-hover:scale-110 transition-transform">
                 <Settings className="w-5 h-5 text-health-secondary" />
               </div>
               <h4 className="text-xs font-bold text-health-primary mb-1">Settings</h4>
               <p className="text-[10px] text-health-secondary leading-tight opacity-60">Review Security Settings.</p>
            </div>
          </div>

          {/* Appointment Calendar */}
          <div className="card p-6 flex-1 flex flex-col border-health-accent/10">
            <div className="flex items-center justify-between mb-8">
              <h3 className="text-lg font-bold text-health-primary tracking-tight">Appointment</h3>
              <button className="p-2.5 bg-health-surface border border-health-border rounded-xl text-health-secondary hover:brightness-125 transition-all">
                <Calendar className="w-4 h-4" />
              </button>
            </div>
            
            <div className="mb-8">
              <div className="flex items-center justify-between mb-5">
                <span className="text-sm font-bold text-health-primary">March 2026</span>
                <div className="flex gap-2">
                   <ChevronLeft className="w-4 h-4 text-health-secondary cursor-pointer hover:text-health-primary transition-colors" />
                   <ChevronRight className="w-4 h-4 text-health-secondary cursor-pointer hover:text-health-primary transition-colors" />
                </div>
              </div>
              
              <div className="flex justify-between items-center bg-health-bg/40 p-3 rounded-2xl border border-health-border">
                {['MON 23', 'TUE 24', 'WED 25', 'THU 26', 'FRI 27', 'SAT 28', 'SUN 29'].map((day, i) => {
                  const [name, num] = day.split(' ');
                  const isActive = num === '25';
                  return (
                    <div key={i} className={`flex flex-col items-center gap-1.5 ${isActive ? 'relative' : ''}`}>
                      <span className={`text-[9px] font-black ${isActive ? 'text-health-brand' : 'text-health-secondary/60'} uppercase tracking-tighter`}>{name}</span>
                      <span className={`text-sm font-black transition-all duration-300 ${isActive ? 'text-white bg-health-brand w-9 h-9 flex items-center justify-center rounded-xl shadow-lg shadow-health-brand/30 scale-110' : 'text-health-secondary/80 hover:text-health-primary cursor-pointer'}`}>
                        {num}
                      </span>
                    </div>
                  )
                })}
              </div>
            </div>
            
            <div className="mt-auto bg-health-surface/30 border border-dashed border-health-border rounded-2xl p-8 flex flex-col items-center justify-center text-center">
              <div className="w-10 h-10 rounded-full bg-health-surface border border-health-border flex items-center justify-center mb-3">
                 <Calendar className="w-4 h-4 text-health-secondary/30" />
              </div>
              <p className="text-xs font-bold text-health-secondary/60 italic tracking-wide">You do not have any appointment.</p>
            </div>
          </div>
        </div>
      </div>

      {/* Stat Cards */}
      {loading ? (
        <div className="flex items-center justify-center py-16">
          <Spinner size="lg" />
        </div>
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
          {statCards.map(({ key, labelKey, icon: Icon, color, bg, format, sub }) => {
            const value = stats?.[key]
            const display = format ? format(value) : (value ?? '-')
            return (
              <div key={key} className="card p-6 flex flex-col justify-between min-h-[140px] hover:border-health-accent/30 transition-all">
                <div className="flex items-center justify-between">
                  <div className={`p-2.5 rounded-xl ${bg}`}>
                    <Icon className={`h-5 w-5 ${color}`} />
                  </div>
                  {stats?.[sub] != null && (
                    <span className="text-[10px] font-bold text-health-secondary uppercase tracking-wider bg-health-bg px-2 py-1 rounded-lg border border-health-border">
                      {t(lang, 'total')}: {stats[sub]}
                    </span>
                  )}
                </div>
                <div>
                  <p className="text-2xl font-bold text-health-primary tracking-tight">{display}</p>
                  <p className="text-xs font-bold text-health-secondary uppercase tracking-widest mt-1.5">{t(lang, labelKey)}</p>
                </div>
              </div>
            )
          })}
        </div>
      )}

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <div className="card">
          <h2 className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-4">Tendenca e të Ardhurave (€)</h2>
          <div className="h-[200px]">
            {loading ? (
              <div className="w-full h-full bg-gray-50 animate-pulse rounded" />
            ) : (
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={analytics.trends} margin={{ top: 0, right: 10, left: -10, bottom: 0 }}>
                  <XAxis dataKey="month" tick={{ fontSize: 12 }} />
                  <YAxis tick={{ fontSize: 12 }} />
                  <Tooltip formatter={(v) => [`€${v}`, 'Të Ardhura']} />
                  <Bar dataKey="revenue" fill="#16a34a" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            )}
          </div>
        </div>

        <div className="card">
          <h2 className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-4">Shitjet sipas Kategorive</h2>
          <div className="h-[200px]">
            {loading ? (
              <div className="w-full h-full bg-gray-50 animate-pulse rounded-full" />
            ) : (
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={analytics.services}
                    dataKey="value"
                    nameKey="name"
                    cx="50%"
                    cy="50%"
                    innerRadius={50}
                    outerRadius={80}
                  >
                    {analytics.services.map((entry, index) => (
                      <Cell key={entry.name} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip formatter={(v) => [`${v} herë`, 'Përdorur']} />
                  <Legend iconSize={10} wrapperStyle={{ fontSize: 12 }} />
                </PieChart>
              </ResponsiveContainer>
            )}
          </div>
        </div>
      </div>

      {/* Activity Feed */}
      <ActivityFeed />

      {/* Quick Links */}
      <div>
        <h2 className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-3">{t(lang, 'addNew')}</h2>
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
          {quickLinks.map(({ to, labelKey, icon: Icon, color, bg }) => (
            <Link
              key={to}
              to={to}
              className="card p-4 flex items-center gap-4 hover:bg-health-hover transition-all group"
            >
              <div className={`p-3 rounded-xl ${bg} group-hover:scale-110 transition-transform`}>
                <Icon className={`h-5 w-5 ${color}`} />
              </div>
              <div className="min-w-0">
                <p className="text-sm font-bold text-health-primary truncate">{t(lang, labelKey)}</p>
                <div className="flex items-center gap-1.5 mt-1">
                  <Plus className="h-3.5 w-3.5 text-health-accent" />
                  <span className="text-[10px] font-bold text-health-secondary uppercase tracking-wider">{t(lang, 'add')}</span>
                </div>
              </div>
            </Link>
          ))}
        </div>
      </div>

      {/* Last 5 Terminet */}
      <div className="card">
        <h2 className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-3">Terminet e Fundit</h2>
        {terminet.length === 0 ? (
          <p className="text-sm text-gray-400 py-4 text-center">{t(lang, 'noRecords')}</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-100 dark:border-gray-700">
                  <th className="text-left py-2 px-3 text-xs font-medium text-gray-500">Klienti</th>
                  <th className="text-left py-2 px-3 text-xs font-medium text-gray-500">Shërbimi</th>
                  <th className="text-left py-2 px-3 text-xs font-medium text-gray-500">Terapisti</th>
                  <th className="text-left py-2 px-3 text-xs font-medium text-gray-500">Data</th>
                  <th className="text-left py-2 px-3 text-xs font-medium text-gray-500">Statusi</th>
                </tr>
              </thead>
              <tbody>
                {terminet.map((t) => (
                  <tr key={t.terminId} className="border-b border-gray-50 dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800/50">
                    <td className="py-2 px-3 text-gray-900 dark:text-white">{t.klientEmri}</td>
                    <td className="py-2 px-3 text-gray-600 dark:text-gray-400">{t.sherbimEmri}</td>
                    <td className="py-2 px-3 text-gray-600 dark:text-gray-400">{t.terapistEmri}</td>
                    <td className="py-2 px-3 text-gray-600 dark:text-gray-400">
                      {new Date(t.dataTerminit).toLocaleDateString('sq-AL')}
                    </td>
                    <td className="py-2 px-3"><StatusBadge statusi={t.statusi} /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Last 5 Klientet */}
      <div className="card">
        <h2 className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-3">Klientët e Fundit</h2>
        {klientet.length === 0 ? (
          <p className="text-sm text-gray-400 py-4 text-center">{t(lang, 'noRecords')}</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-100 dark:border-gray-700">
                  <th className="text-left py-2 px-3 text-xs font-medium text-gray-500">Ad Soyad</th>
                  <th className="text-left py-2 px-3 text-xs font-medium text-gray-500">Email</th>
                  <th className="text-left py-2 px-3 text-xs font-medium text-gray-500">Kayıt Tarihi</th>
                </tr>
              </thead>
              <tbody>
                {klientet.map((k) => (
                  <tr key={k.klientId} className="border-b border-gray-50 dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800/50">
                    <td className="py-2 px-3 text-gray-900 dark:text-white">{k.emri} {k.mbiemri}</td>
                    <td className="py-2 px-3 text-gray-600 dark:text-gray-400">{k.email}</td>
                    <td className="py-2 px-3 text-gray-600 dark:text-gray-400">
                      {k.dataRegjistrimit
                        ? new Date(k.dataRegjistrimit).toLocaleDateString('sq-AL')
                        : '-'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}

function ActivityFeed() {
  const { entries, loading, error, refresh } = useActivityFeed(10)

  const actionColor = (action) => {
    if (!action) return 'bg-gray-400'
    const a = action.toLowerCase()
    if (a.includes('create') || a.includes('post')) return 'bg-green-500'
    if (a.includes('update') || a.includes('put')) return 'bg-blue-500'
    if (a.includes('delete')) return 'bg-red-500'
    return 'bg-gray-400'
  }

  return (
    <div className="card p-4">
      <div className="flex items-center justify-between mb-3">
        <h2 className="text-sm font-semibold text-gray-700 dark:text-gray-300 flex items-center gap-2">
          <Activity className="h-4 w-4 text-green-600" />
          Aktiviteti i Fundit
        </h2>
        <button
          onClick={refresh}
          className="p-1.5 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
          title="Rifresko"
        >
          <RefreshCw className="h-3.5 w-3.5" />
        </button>
      </div>

      {loading && (
        <div className="space-y-3">
          {[...Array(4)].map((_, i) => (
            <div key={i} className="flex items-start gap-3 animate-pulse">
              <div className="w-2 h-2 rounded-full bg-gray-200 mt-1.5 flex-shrink-0" />
              <div className="flex-1 space-y-1">
                <div className="h-3 bg-gray-200 rounded w-3/4" />
                <div className="h-3 bg-gray-100 rounded w-1/2" />
              </div>
            </div>
          ))}
        </div>
      )}

      {error && (
        <p className="text-sm text-gray-400 text-center py-4">{error}</p>
      )}

      {!loading && !error && entries.length === 0 && (
        <p className="text-sm text-gray-400 text-center py-4">Nuk ka aktivitet.</p>
      )}

      {!loading && !error && entries.length > 0 && (
        <div className="space-y-3">
          {entries.map((entry, i) => (
            <div key={entry.id ?? i} className="flex items-start gap-3">
              <span className={`w-2 h-2 rounded-full mt-1.5 flex-shrink-0 ${actionColor(entry.action)}`} />
              <div className="min-w-0 flex-1">
                <p className="text-sm text-gray-700 dark:text-gray-300 truncate">
                  <span className="font-medium">{entry.userName ?? 'Përdorues'}</span>
                  {' — '}
                  {entry.action ?? ''}{' '}
                  <span className="text-gray-500">{entry.entityName ?? entry.tableName ?? ''}</span>
                </p>
                <p className="text-xs text-gray-400 mt-0.5">
                  {entry.timestamp
                    ? new Date(entry.timestamp).toLocaleString('sq-AL')
                    : entry.createdAt
                    ? new Date(entry.createdAt).toLocaleString('sq-AL')
                    : ''}
                </p>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
