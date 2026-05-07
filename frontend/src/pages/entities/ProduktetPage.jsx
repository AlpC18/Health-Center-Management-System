import { ShoppingBag } from 'lucide-react'
import CrudPage from '../../components/crud/CrudPage'
import { ProduktForm } from '../../components/crud/Forms'
import { produktetApi } from '../../api/api'
import { StatusBadge } from '../../components/ui/index'

export function ProduktetPage() {
  return (
    <CrudPage
      title="Produktet"
      subtitle="Menaxhimi i produkteve të shitjes"
      emptyIcon={ShoppingBag}
      api={produktetApi}
      FormComponent={ProduktForm}
      idKey="produktId"
      searchKeys={['emriProduktit', 'kategoria']}
      columns={[
        { key: 'emriProduktit', label: 'Emri' },
        { key: 'kategoria', label: 'Kategoria' },
        {
          key: 'cmimi',
          label: 'Çmimi',
          render: (item) => (item.cmimi != null ? `${item.cmimi} €` : '-'),
        },
        {
          key: 'sasiaStok',
          label: 'Stoku',
          render: (item) => {
            const qty = item.sasiaStok ?? 0
            const badge =
              qty === 0
                ? { label: 'Out of Stock', className: 'bg-red-100 text-red-700 border border-red-200' }
                : qty <= 10
                ? { label: 'Low Stock', className: 'bg-orange-100 text-orange-700 border border-orange-200' }
                : { label: 'In Stock', className: 'bg-green-100 text-green-700 border border-green-200' }
            return (
              <div className="flex items-center gap-2">
                <span className="text-sm text-gray-600">{qty} copë</span>
                <span className={`px-2 py-0.5 rounded-md text-[10px] font-bold uppercase tracking-wider ${badge.className}`}>
                  {badge.label}
                </span>
              </div>
            )
          },
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
