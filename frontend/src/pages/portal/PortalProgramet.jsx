import { useEffect, useState } from 'react'
import { Activity } from 'lucide-react'
import { portalApi } from '../../api/portalApi'
import { PageLoader, StatusBadge } from '../../components/ui'

export default function PortalProgramet() {
  const [programet, setProgramet] = useState([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    portalApi.getProgramet()
      .then((r) => setProgramet(r.data?.data || r.data || []))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <PageLoader />

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Programet e Mia</h1>
      {programet.length === 0 ? (
        <div className="card p-8 text-center">
          <Activity className="w-10 h-10 text-gray-300 mx-auto mb-3" />
          <p className="text-sm text-gray-500">Nuk jeni regjistruar në asnjë program.</p>
          <p className="text-xs text-gray-400 mt-1">Kontaktoni recepcionin për t'u regjistruar.</p>
        </div>
      ) : (
        <div className="space-y-4">
          {programet.map((kp) => (
            <div key={kp.kpId} className="card p-5">
              <div className="flex items-start justify-between mb-3">
                <div>
                  <h3 className="font-bold text-gray-900">{kp.program.emriProgramit}</h3>
                  <p className="text-sm text-gray-500">{kp.program.qellimi}</p>
                </div>
                <StatusBadge status={kp.statusi} />
              </div>
              <div className="mb-3">
                <div className="flex justify-between text-xs text-gray-500 mb-1">
                  <span>Progresi</span>
                  <span>{kp.progresi}%</span>
                </div>
                <div className="h-2 bg-gray-200 rounded-full overflow-hidden">
                  <div className="h-full bg-green-600 rounded-full" style={{ width: `${kp.progresi}%` }} />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-3 text-sm">
                {kp.program.ushtrimet && (
                  <div className="bg-blue-50 rounded-lg p-3">
                    <p className="font-medium text-blue-700 mb-1">💪 Ushtrimet</p>
                    <p className="text-xs text-blue-600">{kp.program.ushtrimet}</p>
                  </div>
                )}
                {kp.program.dieta && (
                  <div className="bg-green-50 rounded-lg p-3">
                    <p className="font-medium text-green-700 mb-1">🥗 Dieta</p>
                    <p className="text-xs text-green-600">{kp.program.dieta}</p>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
