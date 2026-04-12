import { Stethoscope } from 'lucide-react'
import CrudPage from '../../components/crud/CrudPage'
import { SherbimForm } from '../../components/crud/Forms'
import { sherbiimetApi } from '../../api/api'
import { StatusBadge } from '../../components/ui/index'

export function SherbiimetPage() {
  return (
    <CrudPage
      title="Shërbimet"
      subtitle="Menaxhimi i shërbimeve të ofruara"
      emptyIcon={Stethoscope}
      api={sherbiimetApi}
      FormComponent={SherbimForm}
      idKey="sherbimId"
      searchKeys={['emriSherbimit', 'kategoria']}
      columns={[
        { key: 'emriSherbimit', label: 'Emri' },
        { key: 'kategoria', label: 'Kategoria' },
        {
          key: 'kohezgjatjaMin',
          label: 'Kohëzgjatja',
          render: (item) => (item.kohezgjatjaMin ? `${item.kohezgjatjaMin} min` : '-'),
        },
        {
          key: 'cmimi',
          label: 'Çmimi',
          render: (item) => (item.cmimi != null ? `${item.cmimi} €` : '-'),
        },
        {
          key: 'aktiv',
          label: 'Statusi',
          render: (item) => <StatusBadge status={item.aktiv ? 'Aktiv' : 'Perfunduar'} />,
        },
      ]}
    />
  )
}
