import { Star } from 'lucide-react'
import CrudPage from '../../components/crud/CrudPage'
import { VleresimForm } from '../../components/crud/Forms'
import { vlereiisimetApi } from '../../api/api'

export function VlereisiimetPage() {
  return (
    <CrudPage
      title="Vlerësimet"
      subtitle="Menaxhimi i vlerësimeve të klientëve"
      emptyIcon={Star}
      api={vlereiisimetApi}
      FormComponent={VleresimForm}
      idKey="vleresimId"
      searchKeys={['komenti']}
      columns={[
        { key: 'klientName', label: 'Klienti' },
        { key: 'sherbimName', label: 'Shërbimi' },
        { key: 'terapistName', label: 'Terapisti' },
        {
          key: 'nota',
          label: 'Nota',
          render: (item) => (
            <div className="flex items-center gap-1 text-yellow-500">
              <span className="font-bold">{item.nota}</span>
              <Star className="h-3 w-3 fill-current" />
            </div>
          ),
        },
        { key: 'komenti', label: 'Komenti' },
        {
          key: 'dataVleresimit',
          label: 'Data',
          render: (item) => (item.dataVleresimit ? item.dataVleresimit.slice(0, 10) : '-'),
        },
      ]}
    />
  )
}
