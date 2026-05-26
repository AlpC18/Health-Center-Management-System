import { createElement, useCallback, useEffect, useState } from 'react'
import {
  AlertTriangle,
  BarChart3,
  Bell,
  CalendarClock,
  CheckCircle2,
  CreditCard,
  DatabaseBackup,
  LineChart,
  PackageCheck,
  RefreshCw,
  ShieldCheck,
  Sparkles,
  Star,
  Users,
} from 'lucide-react'
import { advancedApi } from '../api/api'
import { Spinner } from '../components/ui'

const featureGroups = [
  {
    title: 'Appointment Intelligence',
    icon: CalendarClock,
    items: [
      'Smart therapist availability slots',
      'Appointment conflict detection',
      'Recurring appointment metadata',
      'Cancellation reasons and status history',
      'Waiting list entries',
    ],
  },
  {
    title: 'Security & Governance',
    icon: ShieldCheck,
    items: [
      'Role-protected admin operations',
      'Client data isolation endpoints',
      'Audit log IP and user-agent tracking',
      'Login rate-limit hardening',
      'Soft-delete fields for core records',
    ],
  },
  {
    title: 'Commerce & Inventory',
    icon: CreditCard,
    items: [
      'Stock decreases when a sale is created',
      'Low-stock alerts',
      'Payment method and payment status',
      'Mock online payment flow',
      'Invoice records for sales',
    ],
  },
  {
    title: 'Analytics & Growth',
    icon: LineChart,
    items: [
      'Revenue reports',
      'Top product and popular service reports',
      'Therapist performance analytics',
      'Client retention analytics',
      'Service recommendations',
    ],
  },
  {
    title: 'Client Experience',
    icon: Star,
    items: [
      'Client medical notes fields',
      'Secure document metadata',
      'Review moderation workflow',
      'Membership expiry monitoring',
      'Real-time notification records',
    ],
  },
  {
    title: 'Operations',
    icon: DatabaseBackup,
    items: [
      'Backup tracking records',
      'Package-to-service mapping',
      'Activity feed data source',
      'Theme/language-ready frontend shell',
      'Admin advanced feature center',
    ],
  },
]

function MetricCard({ icon: Icon, label, value }) {
  return (
    <div className="card p-5">
      <div className="flex items-center gap-3">
        <div className="h-10 w-10 rounded-xl bg-health-brand/10 text-health-brand flex items-center justify-center">
          {createElement(Icon, { className: 'h-5 w-5' })}
        </div>
        <div>
          <p className="text-xs uppercase tracking-widest text-health-secondary font-bold">{label}</p>
          <p className="text-2xl font-black text-health-primary">{value}</p>
        </div>
      </div>
    </div>
  )
}

export default function AdvancedFeaturesPage() {
  const [loading, setLoading] = useState(true)
  const [reports, setReports] = useState(null)
  const [retention, setRetention] = useState(null)
  const [therapists, setTherapists] = useState([])
  const [lowStock, setLowStock] = useState([])
  const [notifications, setNotifications] = useState([])

  const fetchData = useCallback(async () => {
    await Promise.resolve()
    setLoading(true)
    const [reportsRes, retentionRes, therapistRes, stockRes, notificationsRes] = await Promise.allSettled([
      advancedApi.getReports(),
      advancedApi.getClientRetention(),
      advancedApi.getTherapistPerformance(),
      advancedApi.getLowStock(),
      advancedApi.getNotifications(),
    ])

    if (reportsRes.status === 'fulfilled') setReports(reportsRes.value.data)
    if (retentionRes.status === 'fulfilled') setRetention(retentionRes.value.data)
    if (therapistRes.status === 'fulfilled') setTherapists(therapistRes.value.data ?? [])
    if (stockRes.status === 'fulfilled') setLowStock(stockRes.value.data ?? [])
    if (notificationsRes.status === 'fulfilled') setNotifications(notificationsRes.value.data ?? [])
    setLoading(false)
  }, [])

  useEffect(() => {
    const timer = window.setTimeout(() => {
      void fetchData()
    }, 0)
    return () => window.clearTimeout(timer)
  }, [fetchData])

  if (loading) {
    return (
      <div className="flex min-h-[50vh] items-center justify-center">
        <Spinner />
      </div>
    )
  }

  return (
    <div className="space-y-8">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div>
          <h1 className="text-3xl font-black tracking-tight text-health-primary">Advanced Features</h1>
          <p className="text-sm text-health-secondary">Management center for the new platform-level modules.</p>
        </div>
        <button onClick={fetchData} className="btn-secondary inline-flex items-center gap-2 px-5 py-3">
          <RefreshCw className="h-4 w-4" />
          Refresh
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-4">
        <MetricCard icon={BarChart3} label="Revenue" value={`€${Number(reports?.revenue ?? 0).toFixed(2)}`} />
        <MetricCard icon={AlertTriangle} label="Low Stock" value={reports?.lowStockCount ?? lowStock.length} />
        <MetricCard icon={Users} label="Returning Clients" value={retention?.returningClients ?? 0} />
        <MetricCard icon={Bell} label="Notifications" value={notifications.length} />
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-6">
        <section className="xl:col-span-2 card p-6">
          <div className="flex items-center gap-3 mb-5">
            <Sparkles className="h-5 w-5 text-health-brand" />
            <h2 className="text-xl font-black text-health-primary">Implemented Feature Map</h2>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {featureGroups.map(({ title, icon: Icon, items }) => (
              <div key={title} className="rounded-xl border border-health-border bg-health-surface/60 p-5">
                <div className="flex items-center gap-3 mb-4">
                  {createElement(Icon, { className: 'h-5 w-5 text-health-brand' })}
                  <h3 className="font-bold text-health-primary">{title}</h3>
                </div>
                <div className="space-y-2">
                  {items.map((item) => (
                    <div key={item} className="flex items-start gap-2 text-sm text-health-secondary">
                      <CheckCircle2 className="mt-0.5 h-4 w-4 flex-shrink-0 text-health-accent" />
                      <span>{item}</span>
                    </div>
                  ))}
                </div>
              </div>
            ))}
          </div>
        </section>

        <section className="card p-6 space-y-5">
          <div>
            <h2 className="text-xl font-black text-health-primary">Operational Snapshot</h2>
            <p className="text-sm text-health-secondary">Live summary from advanced endpoints.</p>
          </div>
          <div className="rounded-xl border border-health-border p-4">
            <p className="text-xs uppercase tracking-widest text-health-secondary font-bold">Top product</p>
            <p className="font-bold text-health-primary">{reports?.topProduct ?? 'N/A'}</p>
          </div>
          <div className="rounded-xl border border-health-border p-4">
            <p className="text-xs uppercase tracking-widest text-health-secondary font-bold">Popular service</p>
            <p className="font-bold text-health-primary">{reports?.mostPopularService ?? 'N/A'}</p>
          </div>
          <div className="rounded-xl border border-health-border p-4">
            <p className="text-xs uppercase tracking-widest text-health-secondary font-bold">Top therapist</p>
            <p className="font-bold text-health-primary">{reports?.topTherapist ?? 'N/A'}</p>
          </div>
          <div className="rounded-xl border border-health-border p-4">
            <p className="text-xs uppercase tracking-widest text-health-secondary font-bold">Renewal rate</p>
            <p className="font-bold text-health-primary">{Number(retention?.renewalRate ?? 0).toFixed(1)}%</p>
          </div>
        </section>
      </div>

      <section className="card p-6">
        <div className="flex items-center gap-3 mb-5">
          <PackageCheck className="h-5 w-5 text-health-brand" />
          <h2 className="text-xl font-black text-health-primary">Therapist Performance</h2>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-health-border text-left text-health-secondary">
                <th className="py-3">Therapist</th>
                <th className="py-3">Appointments</th>
                <th className="py-3">Revenue</th>
                <th className="py-3">Rating</th>
              </tr>
            </thead>
            <tbody>
              {therapists.map((therapist) => (
                <tr key={therapist.terapistId} className="border-b border-health-border/60">
                  <td className="py-3 font-semibold text-health-primary">{therapist.terapistEmri}</td>
                  <td className="py-3 text-health-secondary">{therapist.appointments}</td>
                  <td className="py-3 text-health-secondary">€{Number(therapist.revenue ?? 0).toFixed(2)}</td>
                  <td className="py-3 text-health-secondary">{Number(therapist.averageRating ?? 0).toFixed(1)}</td>
                </tr>
              ))}
              {therapists.length === 0 && (
                <tr>
                  <td className="py-6 text-health-secondary" colSpan="4">No therapist analytics yet.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  )
}
