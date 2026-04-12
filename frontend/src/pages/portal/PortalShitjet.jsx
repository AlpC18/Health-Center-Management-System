import { useEffect, useState } from 'react'
import { ShoppingCart } from 'lucide-react'
import { portalApi } from '../../api/portalApi'
import { PageLoader } from '../../components/ui'

export default function PortalShitjet() {
  const [shitjet, setShitjet] = useState([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    portalApi.getShitjet()
      .then((r) => setShitjet(r.data?.data || r.data || []))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <PageLoader />

  const total = shitjet.reduce((sum, s) => sum + Number(s.cmimiTotal), 0)
  const fmtDate = (d) => new Date(d).toLocaleDateString('sq-AL')

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-2">Blerjet e Mia</h1>
      <p className="text-sm text-gray-500 mb-6">Historiku i blerjeve tuaja</p>

      {shitjet.length === 0 ? (
        <div className="card p-8 text-center">
          <ShoppingCart className="w-10 h-10 text-gray-300 mx-auto mb-3" />
          <p className="text-sm text-gray-500">Nuk keni blerje ende.</p>
          <a href="/portal/produktet" className="btn-primary mt-3 inline-flex">Shiko Produktet</a>
        </div>
      ) : (
        <>
          <div className="card p-4 mb-4 bg-green-50 border border-green-200">
            <p className="text-sm text-gray-600">Totali i Shpenzimeve</p>
            <p className="text-2xl font-bold text-green-700">€{total.toFixed(2)}</p>
          </div>
          <div className="card overflow-hidden">
            <table className="w-full">
              <thead className="bg-gray-50">
                <tr>
                  <th className="table-th">Produkti</th>
                  <th className="table-th">Sasia</th>
                  <th className="table-th">Totali</th>
                  <th className="table-th">Data</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50">
                {shitjet.map((s) => (
                  <tr key={s.shitjeId} className="hover:bg-gray-50/50">
                    <td className="table-td font-medium">{s.produktEmri}</td>
                    <td className="table-td">{s.sasia} cope</td>
                    <td className="table-td font-semibold text-green-700">€{Number(s.cmimiTotal).toFixed(2)}</td>
                    <td className="table-td">{fmtDate(s.dataShitjes)}</td>
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
