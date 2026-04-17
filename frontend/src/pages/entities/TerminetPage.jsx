import { useState, useEffect } from 'react'
import { Calendar, Printer, FileSpreadsheet } from 'lucide-react'
import CrudPage from '../../components/crud/CrudPage'
import { TerminForm } from '../../components/crud/Forms'
import { terminetApi, klientetApi, sherbiimetApi, terapistetApi } from '../../api/api'
import { StatusBadge } from '../../components/ui/index'

export function TerminetPage() {
  const [klientet, setKlientet] = useState([])
  const [sherbimet, setSherbimet] = useState([])
  const [terapistet, setTerapistet] = useState([])

  const toArray = (payload) => {
    if (Array.isArray(payload?.data)) return payload.data
    if (Array.isArray(payload)) return payload
    return []
  }

  useEffect(() => {
    klientetApi.getAll().then((r) => setKlientet(toArray(r.data))).catch(() => {})
    sherbiimetApi.getAll().then((r) => setSherbimet(toArray(r.data))).catch(() => {})
    terapistetApi.getAll().then((r) => setTerapistet(toArray(r.data))).catch(() => {})
  }, [])

  return (
    <div className="space-y-3">
      <div className="flex justify-end gap-3 print:hidden">
        <button
          onClick={() => window.print()}
          className="btn-secondary px-6"
        >
          <Printer className="h-4 w-4" />
          Printo listën
        </button>
        <button 
          onClick={() => window.open('http://localhost:5077/api/export/terminet/excel', '_blank')}
          className="flex items-center gap-2 px-6 py-2 bg-health-accent text-white rounded-lg hover:brightness-110 active:scale-95 transition-all text-sm font-bold shadow-lg shadow-health-accent/20"
        >
          <FileSpreadsheet className="h-4 w-4" />
          Excel
        </button>
      </div>
      <CrudPage
        title="Terminet"
        subtitle="Menaxhimi i termineve dhe rezervimeve"
        emptyIcon={Calendar}
        api={terminetApi}
        FormComponent={TerminForm}
        idKey="terminId"
        searchKeys={['statusi', 'klientId', 'sherbimId']}
        extraFormProps={{ klientet, sherbimet, terapistet }}
        columns={[
          {
            key: 'klientId',
            label: 'Klienti',
            render: (item) => {
              const k = klientet.find((c) => c.klientId === item.klientId)
              return k ? (
                <span className="font-bold text-health-primary">{k.emri} {k.mbiemri}</span>
              ) : item.klientId ?? '-'
            },
          },
          {
            key: 'sherbimId',
            label: 'Shërbimi',
            render: (item) => {
              const s = sherbimet.find((x) => x.sherbimId === item.sherbimId)
              return s?.emriSherbimit ?? item.sherbimId ?? '-'
            },
          },
          {
            key: 'dataTerminit',
            label: 'Data',
            render: (item) => (item.dataTerminit ? item.dataTerminit.slice(0, 10) : '-'),
          },
          {
            key: 'oraFillimit',
            label: 'Ora',
            render: (item) =>
              item.oraFillimit && item.oraMbarimit
                ? `${item.oraFillimit} – ${item.oraMbarimit}`
                : item.oraFillimit ?? '-',
          },
          {
            key: 'statusi',
            label: 'Statusi',
            render: (item) => <StatusBadge status={item.statusi} />,
          },
        ]}
      />
    </div>
  )
}
