import { useState, useEffect } from 'react'
import { ShoppingCart, Filter, X } from 'lucide-react'
import CrudPage from '../../components/crud/CrudPage'
import { ShitjeForm } from '../../components/crud/Forms'
import { shitjetApi, klientetApi, produktetApi } from '../../api/api'

export function ShitjetPage() {
  const [klientet, setKlientet] = useState([])
  const [produktet, setProduktet] = useState([])
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')

  useEffect(() => {
    klientetApi.getAll().then((r) => setKlientet(r.data ?? [])).catch(() => {})
    produktetApi.getAll().then((r) => setProduktet(r.data ?? [])).catch(() => {})
  }, [])

  const hasFilter = startDate || endDate

  const filterFn = (item) => {
    const d = item.dataShitjes?.slice(0, 10)
    if (startDate && d < startDate) return false
    if (endDate && d > endDate) return false
    return true
  }

  return (
    <div className="space-y-4">
      <div className="card p-4 flex flex-wrap items-end gap-4">
        <div className="flex items-center gap-2 self-end pb-2 text-gray-500">
          <Filter className="w-4 h-4" />
          <span className="text-xs font-semibold uppercase tracking-wide">Filter by Date</span>
        </div>
        <div className="flex flex-col gap-1">
          <label className="text-xs font-semibold text-gray-500">Data Fillimit</label>
          <input
            type="date"
            value={startDate}
            max={endDate || undefined}
            onChange={(e) => setStartDate(e.target.value)}
            className="input text-sm"
          />
        </div>
        <div className="flex flex-col gap-1">
          <label className="text-xs font-semibold text-gray-500">Data Mbarimit</label>
          <input
            type="date"
            value={endDate}
            min={startDate || undefined}
            onChange={(e) => setEndDate(e.target.value)}
            className="input text-sm"
          />
        </div>
        {hasFilter && (
          <button
            onClick={() => { setStartDate(''); setEndDate('') }}
            className="flex items-center gap-1.5 px-3 py-2 text-xs font-semibold text-red-600 hover:bg-red-50 rounded-lg transition-colors self-end"
          >
            <X className="w-3.5 h-3.5" />
            Pastro Filtrin
          </button>
        )}
      </div>

      <CrudPage
        title="Shitjet"
        subtitle="Menaxhimi i shitjeve të produkteve"
        emptyIcon={ShoppingCart}
        api={shitjetApi}
        FormComponent={ShitjeForm}
        idKey="shitjeId"
        searchKeys={[]}
        extraFormProps={{ klientet, produktet }}
        filterFn={hasFilter ? filterFn : undefined}
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
    </div>
  )
}
