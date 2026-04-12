import { useEffect, useState } from 'react'
import { CreditCard, CheckCircle } from 'lucide-react'
import { portalApi } from '../../api/portalApi'
import { PageLoader, StatusBadge } from '../../components/ui'

export default function PortalAnetaresimi() {
  const [anetaresimet, setAnetaresimet] = useState([])
  const [paketat, setPaketat] = useState([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    Promise.all([portalApi.getAnetaresimi(), portalApi.getPaketat()])
      .then(([a, p]) => {
        setAnetaresimet(a.data?.data || a.data || [])
        setPaketat(p.data?.data || p.data || [])
      })
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <PageLoader />

  const aktiv = anetaresimet.find((a) => a.statusi === 'Aktiv')
  const fmtDate = (d) => new Date(d).toLocaleDateString('sq-AL')

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Anëtarësimi Im</h1>

      {aktiv ? (
        <div className="card p-6 mb-6 border-2 border-green-500 bg-green-50">
          <div className="flex items-center gap-3 mb-4">
            <CheckCircle className="w-6 h-6 text-green-600" />
            <h2 className="font-bold text-green-800">Anëtarësimi Aktiv</h2>
          </div>
          <p className="text-2xl font-bold text-gray-900 mb-1">{aktiv.paketaEmri}</p>
          <p className="text-sm text-gray-600 mb-3">{fmtDate(aktiv.dataFillimit)} — {fmtDate(aktiv.dataMbarimit)}</p>
          <div className="flex items-center justify-between mb-4">
            <StatusBadge status={aktiv.statusi} />
            <p className="font-bold text-green-700">€{aktiv.cmimiPaguar}</p>
          </div>
          {(() => {
            const total = new Date(aktiv.dataMbarimit) - new Date(aktiv.dataFillimit)
            const passed = Date.now() - new Date(aktiv.dataFillimit)
            const pct = Math.min(100, Math.max(0, (passed / total) * 100))
            const remaining = Math.max(0, Math.ceil((new Date(aktiv.dataMbarimit) - Date.now()) / (1000 * 60 * 60 * 24)))
            return (
              <div>
                <div className="flex justify-between text-xs text-gray-500 mb-1">
                  <span>Progresi</span>
                  <span>{remaining} ditë mbetur</span>
                </div>
                <div className="h-2 bg-green-200 rounded-full overflow-hidden">
                  <div className="h-full bg-green-600 rounded-full" style={{ width: `${pct}%` }} />
                </div>
              </div>
            )
          })()}
        </div>
      ) : (
        <div className="card p-6 mb-6 text-center border-2 border-dashed border-gray-200">
          <CreditCard className="w-10 h-10 text-gray-300 mx-auto mb-3" />
          <p className="text-sm text-gray-500">Nuk keni anëtarësim aktiv.</p>
          <p className="text-xs text-gray-400">Kontaktoni recepcionin për të blerë anëtarësim.</p>
        </div>
      )}

      <h2 className="text-lg font-bold text-gray-900 mb-4">Paketat e Disponueshme</h2>
      <div className="grid md:grid-cols-2 gap-4 mb-6">
        {paketat.map((p) => (
          <div key={p.paketId} className="card p-5">
            <h3 className="font-bold text-gray-900 mb-2">{p.emriPaketes}</h3>
            <p className="text-sm text-gray-500 mb-3">{p.pershkrimi}</p>
            {p.sherbimiPerfshire && (
              <div className="flex flex-wrap gap-1 mb-3">
                {p.sherbimiPerfshire.split(',').map((s) => (
                  <span key={s} className="badge-green">{s.trim()}</span>
                ))}
              </div>
            )}
            <div className="flex items-center justify-between">
              <span className="text-xs text-gray-500">{p.kohezgjatjaMuaj} muaj</span>
              <span className="text-xl font-bold text-green-700">€{p.cmimi}</span>
            </div>
          </div>
        ))}
      </div>

      {anetaresimet.length > 0 && (
        <>
          <h2 className="text-lg font-bold text-gray-900 mb-4">Historiku</h2>
          <div className="card overflow-hidden">
            <table className="w-full">
              <thead className="bg-gray-50">
                <tr>
                  <th className="table-th">Paketa</th>
                  <th className="table-th">Fillimi</th>
                  <th className="table-th">Mbarimi</th>
                  <th className="table-th">Çmimi</th>
                  <th className="table-th">Statusi</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50">
                {anetaresimet.map((a) => (
                  <tr key={a.anetaresimId}>
                    <td className="table-td font-medium">{a.paketaEmri}</td>
                    <td className="table-td">{fmtDate(a.dataFillimit)}</td>
                    <td className="table-td">{fmtDate(a.dataMbarimit)}</td>
                    <td className="table-td">€{a.cmimiPaguar}</td>
                    <td className="table-td"><StatusBadge status={a.statusi} /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  )
}
