import { useState, useEffect } from 'react'
import { ShoppingCart } from 'lucide-react'
import CrudPage from '../../components/crud/CrudPage'
import { ShitjeForm } from '../../components/crud/Forms'
import { shitjetApi, klientetApi, produktetApi } from '../../api/api'

export function ShitjetPage() {
  const [klientet, setKlientet] = useState([])
  const [produktet, setProduktet] = useState([])

  useEffect(() => {
    klientetApi.getAll().then((r) => setKlientet(r.data ?? [])).catch(() => {})
    produktetApi.getAll().then((r) => setProduktet(r.data ?? [])).catch(() => {})
  }, [])

  return (
    <CrudPage
      title="Shitjet"
      subtitle="Menaxhimi i shitjeve të produkteve"
      emptyIcon={ShoppingCart}
      api={shitjetApi}
      FormComponent={ShitjeForm}
      idKey="shitjeId"
      searchKeys={[]}
      extraFormProps={{ klientet, produktet }}
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
          key: 'produktId',
          label: 'Produkti',
          render: (item) => {
            const p = produktet.find((x) => x.produktId === item.produktId)
            return p?.emriProduktit ?? item.produktId ?? '-'
          },
        },
        { key: 'sasia', label: 'Sasia' },
        {
          key: 'cmimiTotal',
          label: 'Totali',
          render: (item) => (item.cmimiTotal != null ? `${item.cmimiTotal} €` : '-'),
        },
        {
          key: 'dataShitjes',
          label: 'Data',
          render: (item) => (item.dataShitjes ? item.dataShitjes.slice(0, 10) : '-'),
        },
      ]}
    />
  )
}
