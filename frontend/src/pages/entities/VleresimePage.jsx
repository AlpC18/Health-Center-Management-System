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
            <div className="flex items-center gap-0.5">
              {[1, 2, 3, 4, 5].map((i) => (
                <Star
                  key={i}
                  className={`h-4 w-4 ${
                    i <= item.nota
                      ? 'text-yellow-400 fill-yellow-400'
                      : 'text-gray-300 fill-gray-300'
                  }`}
                />
              ))}
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
