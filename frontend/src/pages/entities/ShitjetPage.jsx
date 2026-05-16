import { useState, useEffect } from 'react'
import { ShoppingCart, Filter, X, CreditCard, CheckCircle, AlertCircle, RefreshCw } from 'lucide-react'
import CrudPage from '../../components/crud/CrudPage'
import { ShitjeForm } from '../../components/crud/Forms'
import { shitjetApi, klientetApi, produktetApi } from '../../api/api'
import toast from 'react-hot-toast'

const PAYMENT_STATUSES = ['Paguar', 'Papaguar', 'Kthyer']

const paymentStatusBadge = (status) => {
  const classes = {
    Paguar: 'bg-green-500/10 text-green-400 border-green-500/20',
    Papaguar: 'bg-orange-500/10 text-orange-400 border-orange-500/20',
    Kthyer: 'bg-red-500/10 text-red-400 border-red-500/20',
  }
  return (
    <span className={`px-2 py-0.5 rounded-lg border text-[10px] font-bold uppercase tracking-wide ${classes[status] ?? 'bg-gray-500/10 text-gray-400 border-gray-500/20'}`}>
      {status ?? '-'}
    </span>
  )
}

export function ShitjetPage() {
  const [klientet, setKlientet] = useState([])
  const [produktet, setProduktet] = useState([])
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')
  const [updatingId, setUpdatingId] = useState(null)
  const [refreshKey, setRefreshKey] = useState(0)

  useEffect(() => {
    klientetApi.getAll().then((r) => setKlientet(r.data?.data ?? r.data ?? [])).catch(() => {})
    produktetApi.getAll().then((r) => setProduktet(r.data?.data ?? r.data ?? [])).catch(() => {})
  }, [])

  const handlePaymentStatusChange = async (id, newStatus) => {
    setUpdatingId(id)
    try {
      await shitjetApi.updatePaymentStatus(id, newStatus)
      toast.success(`Statusi u ndryshua në "${newStatus}"`)
      setRefreshKey((k) => k + 1)
    } catch {
      toast.error('Ndryshimi dështoi.')
    } finally {
      setUpdatingId(null)
    }
  }

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
        key={refreshKey}
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
              return k ? `${k.emri} ${k.mbiemri}` : item.klientEmri ?? item.klientId ?? '-'
            },
          },
          {
            key: 'produktId',
            label: 'Produkti',
            render: (item) => {
              const p = produktet.find((x) => x.produktId === item.produktId)
              return p?.emriProduktit ?? item.produktEmri ?? item.produktId ?? '-'
            },
          },
          { key: 'sasia', label: 'Sasia' },
          {
            key: 'cmimiTotal',
            label: 'Totali',
            render: (item) => (item.cmimiTotal != null ? `${item.cmimiTotal} €` : '-'),
          },
          {
            key: 'tipiPageses',
            label: 'Tipi',
            render: (item) => (
              <span className="flex items-center gap-1 text-xs">
                <CreditCard className="w-3 h-3 text-health-secondary" />
                {item.tipiPageses ?? '-'}
              </span>
            ),
          },
          {
            key: 'statusiPageses',
            label: 'Pagesa',
            render: (item) => (
              <div className="flex items-center gap-2">
                {paymentStatusBadge(item.statusiPageses)}
                <select
                  value={item.statusiPageses ?? 'Paguar'}
                  disabled={updatingId === item.shitjeId}
                  onChange={(e) => handlePaymentStatusChange(item.shitjeId, e.target.value)}
                  className="text-[10px] bg-health-surface border border-health-border rounded-lg px-1.5 py-0.5 text-health-secondary cursor-pointer hover:border-health-accent/40 transition-colors disabled:opacity-50"
                >
                  {PAYMENT_STATUSES.map((s) => (
                    <option key={s} value={s}>{s}</option>
                  ))}
                </select>
              </div>
            ),
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
