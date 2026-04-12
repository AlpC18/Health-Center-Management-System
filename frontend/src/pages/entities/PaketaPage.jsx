import { Package } from 'lucide-react'
import CrudPage from '../../components/crud/CrudPage'
import { PaketaForm } from '../../components/crud/Forms'
import { paketaApi } from '../../api/api'
import { StatusBadge } from '../../components/ui/index'

export function PaketaPage() {
  return (
    <CrudPage
      title="Paketat"
      subtitle="Menaxhimi i paketave wellness"
      emptyIcon={Package}
      api={paketaApi}
      FormComponent={PaketaForm}
      idKey="paketId"
      searchKeys={['emriPaketes']}
      columns={[
        { key: 'emriPaketes', label: 'Emri' },
        {
          key: 'kohezgjatjaMuaj',
          label: 'Kohëzgjatja',
          render: (item) => (item.kohezgjatjaMuaj ? `${item.kohezgjatjaMuaj} muaj` : '-'),
        },
        {
          key: 'cmimi',
          label: 'Çmimi',
          render: (item) => (item.cmimi != null ? `${item.cmimi} €` : '-'),
        },
        {
          key: 'aktive',
          label: 'Statusi',
          render: (item) => <StatusBadge status={item.aktive ? 'Aktiv' : 'Perfunduar'} />,
        },
      ]}
    />
  )
}
