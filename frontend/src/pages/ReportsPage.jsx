import { useCallback, useState, useEffect } from 'react'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, PieChart, Pie, Cell, Legend } from 'recharts'
import { TrendingUp, ShoppingBag, Calendar, Star, Download, RefreshCw } from 'lucide-react'
import { reportsApi } from '../api/api'
import { Spinner } from '../components/ui/index'

const COLORS = ['#16a34a', '#2563eb', '#d97706', '#9333ea', '#db2777', '#6b7280']

export default function ReportsPage() {
  const [data, setData] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  const fetchData = useCallback(async () => {
    await Promise.resolve()
    setLoading(true)
    setError(null)
    try {
      const r = await reportsApi.getAnalytics()
      setData(r.data)
    } catch {
      setError('Nuk mund të ngarkohen raportet.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { void fetchData() }, [fetchData])

  const handleDownloadPdf = () => {
    reportsApi.getKlientetPdf().then((r) => {
      const url = URL.createObjectURL(r.data)
      const a = document.createElement('a')
      a.href = url
      a.download = `Raporti_Klienteve_${new Date().toISOString().slice(0, 10)}.pdf`
      a.click()
      URL.revokeObjectURL(url)
    }).catch(() => {
      // Export failures are non-blocking for the reports page.
    })
  }

  return (
    <div className="space-y-8">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-black text-health-primary tracking-tighter">Raporte</h1>
          <p className="text-sm text-health-secondary font-medium">Analiza dhe statistika të sistemit</p>
        </div>
        <div className="flex gap-3">
          <button onClick={fetchData} className="btn-secondary flex items-center gap-2 px-4 py-2 rounded-xl">
            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
            Rifresko
          </button>
          <button onClick={handleDownloadPdf} className="btn-primary flex items-center gap-2 px-4 py-2 rounded-xl">
            <Download className="w-4 h-4" />
            Eksporto PDF
          </button>
        </div>
      </div>

      {loading ? (
        <div className="flex items-center justify-center py-20">
          <Spinner size="lg" />
        </div>
      ) : error ? (
        <div className="card p-8 text-center text-health-secondary">{error}</div>
      ) : (
        <>
          {/* Summary Cards */}
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
            <div className="card p-6">
              <div className="flex items-center gap-3 mb-3">
                <div className="p-2 rounded-xl bg-health-brand/10">
                  <TrendingUp className="w-5 h-5 text-health-brand" />
                </div>
                <span className="text-xs font-bold text-health-secondary uppercase tracking-wide">Të Ardhura Totale</span>
              </div>
              <p className="text-2xl font-black text-health-primary">€{Number(data.totalRevenue ?? 0).toFixed(2)}</p>
            </div>
            <div className="card p-6">
              <div className="flex items-center gap-3 mb-3">
                <div className="p-2 rounded-xl bg-yellow-400/10">
                  <Star className="w-5 h-5 text-yellow-400" />
                </div>
                <span className="text-xs font-bold text-health-secondary uppercase tracking-wide">Nota Mesatare</span>
              </div>
              <p className="text-2xl font-black text-health-primary">{Number(data.avgRating ?? 0).toFixed(1)} / 5</p>
            </div>
            <div className="card p-6">
              <div className="flex items-center gap-3 mb-3">
                <div className="p-2 rounded-xl bg-health-accent/10">
                  <ShoppingBag className="w-5 h-5 text-health-accent" />
                </div>
                <span className="text-xs font-bold text-health-secondary uppercase tracking-wide">Produkte Top</span>
              </div>
              <p className="text-2xl font-black text-health-primary">{data.topProducts?.length ?? 0}</p>
            </div>
            <div className="card p-6">
              <div className="flex items-center gap-3 mb-3">
                <div className="p-2 rounded-xl bg-blue-500/10">
                  <Calendar className="w-5 h-5 text-blue-500" />
                </div>
                <span className="text-xs font-bold text-health-secondary uppercase tracking-wide">Shërbime Populare</span>
              </div>
              <p className="text-2xl font-black text-health-primary">{data.popularServices?.length ?? 0}</p>
            </div>
          </div>

          {/* Charts Row */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Revenue by Month */}
            <div className="card p-6">
              <h3 className="text-sm font-bold text-health-primary mb-4 uppercase tracking-wide">Të Ardhurat Mujore (€)</h3>
              <div className="h-[220px]">
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={data.revenueByMonth} margin={{ top: 0, right: 10, left: -10, bottom: 0 }}>
                    <XAxis dataKey="month" tick={{ fontSize: 11 }} />
                    <YAxis tick={{ fontSize: 11 }} />
                    <Tooltip formatter={(v) => [`€${Number(v).toFixed(2)}`, 'Të Ardhura']} />
                    <Bar dataKey="revenue" fill="#16a34a" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </div>

            {/* Popular Services */}
            <div className="card p-6">
              <h3 className="text-sm font-bold text-health-primary mb-4 uppercase tracking-wide">Shërbime Popullore</h3>
              <div className="h-[220px]">
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie data={data.popularServices} dataKey="count" nameKey="name" cx="50%" cy="50%" innerRadius={50} outerRadius={80}>
                      {data.popularServices.map((_, i) => (
                        <Cell key={i} fill={COLORS[i % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip formatter={(v) => [`${v} termin`, 'Numri']} />
                    <Legend iconSize={10} wrapperStyle={{ fontSize: 11 }} />
                  </PieChart>
                </ResponsiveContainer>
              </div>
            </div>
          </div>

          {/* Top Products Table */}
          <div className="card p-6">
            <h3 className="text-sm font-bold text-health-primary mb-4 uppercase tracking-wide">Produktet më të Shitura</h3>
            {data.topProducts?.length === 0 ? (
              <p className="text-sm text-health-secondary text-center py-4">Nuk ka të dhëna.</p>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-health-border">
                      <th className="text-left py-2 px-3 text-xs font-bold text-health-secondary uppercase tracking-wide">#</th>
                      <th className="text-left py-2 px-3 text-xs font-bold text-health-secondary uppercase tracking-wide">Produkti</th>
                      <th className="text-right py-2 px-3 text-xs font-bold text-health-secondary uppercase tracking-wide">Sasia</th>
                      <th className="text-right py-2 px-3 text-xs font-bold text-health-secondary uppercase tracking-wide">Të Ardhura</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.topProducts.map((p, i) => (
                      <tr key={p.name} className="border-b border-health-border/50 hover:bg-health-hover transition-colors">
                        <td className="py-2.5 px-3 text-health-secondary font-bold">{i + 1}</td>
                        <td className="py-2.5 px-3 text-health-primary font-medium">{p.name}</td>
                        <td className="py-2.5 px-3 text-right text-health-secondary">{p.sasia}</td>
                        <td className="py-2.5 px-3 text-right font-bold text-health-brand">€{Number(p.revenue).toFixed(2)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </>
      )}
    </div>
  )
}
