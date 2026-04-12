import { useState, useEffect } from 'react'
import { 
  Calendar, 
  Package, 
  ShoppingBag, 
  Activity, 
  Clock, 
  CheckCircle2, 
  AlertCircle,
  TrendingUp,
  Dumbbell
} from 'lucide-react'
import { terminetApi, anetaresimetApi, shitjetApi } from '../api/api'
import useAuthStore from '../store/authStore'
import { StatusBadge, Spinner } from '../components/ui'

const formatDate = (date, options = { day: '2-digit', month: 'short', year: 'numeric' }) => {
  return new Intl.DateTimeFormat('sq-AL', options).format(new Date(date))
}

export default function ClientPortalPage() {
  const { user } = useAuthStore()
  const [loading, setLoading] = useState(true)
  const [data, setData] = useState({
    appointments: [],
    memberships: [],
    purchases: []
  })

  useEffect(() => {
    async function fetchData() {
      try {
        const [appts, membs, sales] = await Promise.all([
          terminetApi.getMy(),
          anetaresimetApi.getMy(),
          shitjetApi.getMy()
        ])
        setData({
          appointments: appts.data || [],
          memberships: membs.data || [],
          purchases: sales.data || []
        })
      } catch (err) {
        console.error("Failed to fetch portal data", err)
      } finally {
        setLoading(false)
      }
    }
    fetchData()
  }, [])

  if (loading) return (
    <div className="flex flex-col items-center justify-center h-[60vh] gap-4">
      <Spinner size="lg" />
      <p className="text-sm font-bold text-health-secondary uppercase tracking-widest animate-pulse">Duke përgatitur profilin tuaj...</p>
    </div>
  )

  const upcomingAppts = data.appointments.filter(a => a.statusi !== 'Anuluar' && a.statusi !== 'Përfunduar')
  const activeMemberships = data.memberships.filter(m => m.statusi === 'Aktiv')

  return (
    <div className="max-w-7xl mx-auto space-y-10 pb-20">
      {/* Welcome Section */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-end gap-6">
        <div className="space-y-2">
          <div className="flex items-center gap-3 mb-2">
            <div className="px-3 py-1 bg-health-brand/10 border border-health-brand/20 rounded-full">
              <span className="text-[10px] font-bold text-health-brand uppercase tracking-widest">Digital Health ID: #{user?.id?.slice(-6).toUpperCase()}</span>
            </div>
          </div>
          <h1 className="text-4xl font-bold text-health-primary tracking-tighter">
            Mirë se vini, <span className="bg-gradient-to-r from-health-brand to-health-accent bg-clip-text text-transparent">{user?.firstName}</span>
          </h1>
          <p className="text-health-secondary font-medium">Këtu është përmbledhja e aktivitetit tuaj në Wellness House.</p>
        </div>
        <div className="flex gap-4">
          <div className="bg-health-surface border border-health-border p-4 rounded-2xl shadow-xl flex items-center gap-4">
            <div className="w-12 h-12 rounded-xl bg-health-accent/10 flex items-center justify-center text-health-accent shadow-inner">
              <Activity className="w-6 h-6" />
            </div>
            <div>
              <p className="text-[10px] font-bold text-health-secondary uppercase tracking-wider">Statusi i Shëndetit</p>
              <p className="text-sm font-bold text-health-primary">I Shkëlqyer</p>
            </div>
          </div>
        </div>
      </div>

      {/* Grid Layout */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        {/* Appointments Section */}
        <div className="lg:col-span-2 space-y-6">
          <div className="flex items-center justify-between">
            <h3 className="text-lg font-bold text-health-primary tracking-tight flex items-center gap-2">
              <Calendar className="w-5 h-5 text-health-accent" /> Terminet e Ardhshme
            </h3>
            <span className="text-xs font-bold text-health-secondary uppercase">{upcomingAppts.length} Termine</span>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {upcomingAppts.length > 0 ? (
              upcomingAppts.map((appt) => (
                <div key={appt.terminId} className="card p-6 border-l-4 border-l-health-accent hover:translate-y-[-4px] transition-all">
                  <div className="flex justify-between items-start mb-4">
                    <div className="space-y-1">
                      <p className="text-xs font-bold text-health-secondary uppercase tracking-widest">{appt.sherbimEmri || 'Shërbim Wellness'}</p>
                      <h4 className="text-lg font-bold text-health-primary">{appt.terapistEmri || 'Terapisti'}</h4>
                    </div>
                    <StatusBadge status={appt.statusi} />
                  </div>
                  <div className="flex items-center gap-4 text-sm text-health-secondary font-medium">
                    <div className="flex items-center gap-1.5">
                      <Clock className="w-4 h-4" />
                      {appt.oraFillimit}
                    </div>
                    <div className="flex items-center gap-1.5">
                      <Calendar className="w-4 h-4" />
                      {formatDate(appt.dataTerminit)}
                    </div>
                  </div>
                </div>
              ))
            ) : (
              <div className="col-span-2 card p-10 flex flex-col items-center justify-center text-center gap-4 bg-health-surface/30 border-dashed">
                <div className="p-4 bg-health-bg rounded-full text-health-secondary/20">
                  <Calendar className="w-10 h-10" />
                </div>
                <div>
                  <p className="text-health-primary font-bold">Nuk keni termine të ardhshme.</p>
                  <p className="text-sm text-health-secondary mt-1">Rezervoni një seancë të re sot!</p>
                </div>
              </div>
            )}
          </div>

          {/* Activity Timeline */}
          <div className="space-y-6 pt-6">
            <h3 className="text-lg font-bold text-health-primary tracking-tight flex items-center gap-2">
              <TrendingUp className="w-5 h-5 text-health-brand" /> Aktiviteti i Fundit
            </h3>
            <div className="card divide-y divide-health-border/50">
              {data.purchases.slice(0, 5).map((sale) => (
                <div key={sale.shitjeId} className="p-4 flex items-center justify-between hover:bg-health-hover/30 transition-colors">
                  <div className="flex items-center gap-4">
                    <div className="w-10 h-10 rounded-xl bg-health-bg border border-health-border flex items-center justify-center text-health-secondary">
                      <ShoppingBag className="w-5 h-5" />
                    </div>
                    <div>
                      <p className="text-sm font-bold text-health-primary">{sale.produktEmri}</p>
                      <p className="text-[10px] text-health-secondary uppercase font-medium">{formatDate(sale.dataShitjes, { day: '2-digit', month: 'long' })}</p>
                    </div>
                  </div>
                  <p className="text-sm font-bold text-health-accent">+{sale.cmimiTotal}€</p>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Sidebar Sections */}
        <div className="space-y-8">
          {/* Active Membership */}
          <div className="space-y-4">
            <h3 className="text-sm font-bold text-health-secondary uppercase tracking-widest pl-1">Anëtarësimet Aktive</h3>
            {activeMemberships.map(memb => (
              <div key={memb.anetaresimId} className="card p-6 bg-gradient-to-br from-health-surface to-health-bg border-health-brand/30 ring-1 ring-health-brand/10">
                <div className="flex items-center gap-3 mb-6">
                  <div className="w-12 h-12 rounded-2xl bg-health-brand/10 flex items-center justify-center text-health-brand shadow-lg shadow-health-brand/10">
                    <Package className="w-6 h-6" />
                  </div>
                  <div>
                    <h4 className="text-lg font-bold text-health-primary tracking-tight">{memb.paketEmri}</h4>
                    <p className="text-[10px] font-bold text-health-brand uppercase tracking-widest">Plani VIP</p>
                  </div>
                </div>
                <div className="space-y-4 mb-6">
                  <div className="flex justify-between text-xs font-medium">
                    <span className="text-health-secondary">Skadon më:</span>
                    <span className="text-health-primary">{formatDate(memb.dataMbarimit)}</span>
                  </div>
                  <div className="w-full h-1.5 bg-health-bg rounded-full overflow-hidden border border-health-border">
                    <div className="h-full bg-health-brand w-3/4 rounded-full shadow-[0_0_8px_rgba(229,9,20,0.5)]" />
                  </div>
                </div>
                <button className="w-full py-2.5 bg-health-brand text-white text-xs font-bold rounded-xl hover:brightness-110 transition-all shadow-lg shadow-health-brand/20 uppercase tracking-widest">
                  Detajet e Paketës
                </button>
              </div>
            ))}
          </div>

          {/* Quick Stats */}
          <div className="card p-6 space-y-6">
            <h3 className="text-sm font-bold text-health-primary uppercase tracking-widest border-b border-health-border pb-4">Statistikat</h3>
            <div className="space-y-5">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <Dumbbell className="w-4 h-4 text-health-accent" />
                  <span className="text-xs font-medium text-health-secondary">Seanca të kryera</span>
                </div>
                <span className="text-sm font-bold text-health-primary">24</span>
              </div>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <CheckCircle2 className="w-4 h-4 text-green-500" />
                  <span className="text-xs font-medium text-health-secondary">Vlerësime Pozitive</span>
                </div>
                <span className="text-sm font-bold text-health-primary">12</span>
              </div>
            </div>
          </div>

          {/* Help Card */}
          <div className="card p-6 bg-health-accent shadow-[0_0_30px_rgba(88,166,255,0.15)] border-none">
            <h4 className="text-white font-bold mb-2">Keni nevojë për ndihmë?</h4>
            <p className="text-white/80 text-xs mb-4 leading-relaxed">Kontaktoni me asistencën tonë për çdo pyetje rreth termineve tuaja.</p>
            <button className="w-full py-2.5 bg-white text-health-accent text-xs font-bold rounded-xl hover:bg-opacity-90 transition-all uppercase tracking-widest">
              Mbështetja Live
            </button>
          </div>
        </div>

      </div>
    </div>
  )
}
