import { UserCheck } from 'lucide-react'
import CrudPage from '../../components/crud/CrudPage'
import { TerapistForm } from '../../components/crud/Forms'
import { terapistetApi } from '../../api/api'
import { StatusBadge } from '../../components/ui/index'

export function TerapistetPage() {
  return (
    <CrudPage
      title="Terapistët"
      subtitle="Menaxhimi i stafit terapeutik"
      emptyIcon={UserCheck}
      api={terapistetApi}
      FormComponent={TerapistForm}
      idKey="terapistId"
      searchKeys={['emri', 'mbiemri', 'specializimi']}
      columns={[
        {
          key: 'emri',
          label: 'Emri & Mbiemri',
          render: (item) => (
            <span className="font-bold text-health-primary">
              {item.emri} {item.mbiemri}
            </span>
          ),
        },
        { key: 'specializimi', label: 'Specializimi' },
        { key: 'licenca', label: 'Licenca' },
        {
          key: 'aktiv',
          label: 'Statusi',
          render: (item) => <StatusBadge status={item.aktiv ? 'Aktiv' : 'Perfunduar'} />,
        },
      ]}
    />
  )
}
