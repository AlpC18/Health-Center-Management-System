import { useEffect, useState } from 'react'
import { Activity } from 'lucide-react'
import CrudPage from '../../components/crud/CrudPage'
import { KlientProgramForm } from '../../components/crud/Forms'
import { klientProgrametApi, klientetApi, programetApi } from '../../api/api'

export function KlientProgrametPage() {
  const [klientet, setKlientet] = useState([])
  const [programet, setProgramet] = useState([])

  useEffect(() => {
    klientetApi.getAll().then((r) => setKlientet(r.data?.data ?? r.data ?? [])).catch(() => {})
    programetApi.getAll().then((r) => setProgramet(r.data?.data ?? r.data ?? [])).catch(() => {})
  }, [])

  return (
    <CrudPage
      title="Klient Programet"
      subtitle="Menaxhimi i programeve te caktuara per klientet"
      emptyIcon={Activity}
      api={klientProgrametApi}
      FormComponent={KlientProgramForm}
      idKey="kpId"
      searchKeys={[]}
      extraFormProps={{ klientet, programet }}
      columns={[
        {
          key: 'klientId',
          label: 'Klienti',
          render: (item) => item.klientEmri ?? klientet.find((k) => k.klientId === item.klientId)?.emri ?? '-',
        },
        {
          key: 'programId',
          label: 'Programi',
          render: (item) => item.programEmri ?? programet.find((p) => p.programId === item.programId)?.emriProgramit ?? '-',
        },
        { key: 'dataFillimit', label: 'Fillimi', render: (item) => item.dataFillimit?.slice(0, 10) ?? '-' },
        { key: 'dataMbarimit', label: 'Mbarimi', render: (item) => item.dataMbarimit ? item.dataMbarimit.slice(0, 10) : '-' },
        { key: 'progresi', label: 'Progresi', render: (item) => `${item.progresi ?? 0}%` },
        { key: 'statusi', label: 'Statusi' },
      ]}
    />
  )
}
