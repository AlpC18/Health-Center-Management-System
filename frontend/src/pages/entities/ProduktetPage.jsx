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
          render: (item) => (
            <span className={`px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-wider ${item.sasiaStok === 0 ? 'bg-health-brand/10 text-health-brand border border-health-brand/20' : 'bg-health-accent/10 text-health-accent border border-health-accent/20'}`}>
              {item.sasiaStok} copë
            </span>
          ),
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
