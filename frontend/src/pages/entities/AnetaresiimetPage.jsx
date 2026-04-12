import { useState, useEffect } from 'react'
import { CreditCard } from 'lucide-react'
import CrudPage from '../../components/crud/CrudPage'
import { AnetaresimForm } from '../../components/crud/Forms'
import { anetaresimetApi, klientetApi, paketaApi } from '../../api/api'
import { StatusBadge } from '../../components/ui/index'

export function AnetaresiimetPage() {
  const [klientet, setKlientet] = useState([])
  const [paketat, setPaketat] = useState([])

  useEffect(() => {
    klientetApi.getAll().then((r) => setKlientet(r.data ?? [])).catch(() => {})
    paketaApi.getAll().then((r) => setPaketat(r.data ?? [])).catch(() => {})
  }, [])

  return (
    <CrudPage
      title="Anëtarësimet"
      subtitle="Menaxhimi i anëtarësimeve të klientëve"
      emptyIcon={CreditCard}
      api={anetaresimetApi}
      FormComponent={AnetaresimForm}
      idKey="anetaresimId"
      searchKeys={['statusi']}
      extraFormProps={{ klientet, paketat }}
      columns={[
        {
          key: 'klientId',
          label: 'Klienti',
          render: (item) => {
            const k = klientet.find((c) => c.klientId === item.klientId)
            return k ? `${k.emri} ${k.mbiemri}` : item.klientId ?? '-'
          },
        },
        {
          key: 'paketId',
          label: 'Paketa',
          render: (item) => {
            const p = paketat.find((x) => x.paketId === item.paketId)
            return p?.emriPaketes ?? item.paketId ?? '-'
          },
        },
        {
          key: 'dataFillimit',
          label: 'Fillimi',
          render: (item) => (item.dataFillimit ? item.dataFillimit.slice(0, 10) : '-'),
        },
        {
          key: 'dataMbarimit',
          label: 'Mbarimi',
          render: (item) => (item.dataMbarimit ? item.dataMbarimit.slice(0, 10) : '-'),
        },
        {
          key: 'cmimiPaguar',
          label: 'Çmimi Paguar',
          render: (item) => (item.cmimiPaguar != null ? `${item.cmimiPaguar} €` : '-'),
        },
        {
          key: 'statusi',
          label: 'Statusi',
          render: (item) => <StatusBadge status={item.statusi} />,
        },
      ]}
    />
  )
}
