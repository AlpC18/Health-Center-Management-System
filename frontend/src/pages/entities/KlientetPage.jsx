import { useState } from 'react'
import { Users, Printer, FileSpreadsheet } from 'lucide-react'
import CrudPage from '../../components/crud/CrudPage'
import { KlientForm } from '../../components/crud/Forms'
import { klientetApi } from '../../api/api'

export function KlientetPage() {
  const [gjiniaFilter, setGjiniaFilter] = useState('')

  const handleDownloadPdf = () => {
    window.open('http://localhost:5077/api/reports/klientet-pdf', '_blank')
  }

  const handleKlientSaved = async (savedItem, formData) => {
    if (!formData?._newFoto) return
    const id = savedItem?.klientId
    if (!id) return
    await klientetApi.uploadFoto(id, formData._newFoto)
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-health-surface p-6 rounded-xl border border-health-border shadow-xl">
        <div className="flex items-center gap-4">
          <label className="text-sm font-bold text-health-secondary uppercase tracking-wider">Filtro sipas Gjinisë:</label>
          <select 
            className="input w-40 py-1.5"
            value={gjiniaFilter}
            onChange={(e) => setGjiniaFilter(e.target.value)}
          >
            <option value="">Të gjithë</option>
            <option value="M">Meshkuj (M)</option>
            <option value="F">Femra (F)</option>
          </select>
        </div>
        
        <div className="flex items-center gap-3">
          <button 
            onClick={handleDownloadPdf}
            className="flex items-center gap-2 px-6 py-2 bg-health-brand text-white rounded-lg hover:brightness-110 active:scale-95 transition-all text-sm font-bold shadow-lg shadow-health-brand/20"
          >
            <Printer className="h-4 w-4" />
            PDF
          </button>
          <button 
            onClick={() => window.open('http://localhost:5077/api/export/klientet/excel', '_blank')}
            className="flex items-center gap-2 px-6 py-2 bg-health-accent text-white rounded-lg hover:brightness-110 active:scale-95 transition-all text-sm font-bold shadow-lg shadow-health-accent/20"
          >
            <FileSpreadsheet className="h-4 w-4" />
            Excel
          </button>
        </div>
      </div>

      <CrudPage
        title="Klientët"
        subtitle="Menaxhimi i klientëve të qendrës"
        emptyIcon={Users}
        api={klientetApi}
        FormComponent={KlientForm}
        idKey="klientId"
        searchKeys={['emri', 'mbiemri', 'email']}
        filterFn={(item) => !gjiniaFilter || item.gjinia === gjiniaFilter}
        onSaved={handleKlientSaved}
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
          { key: 'email', label: 'Email' },
          { key: 'telefoni', label: 'Telefoni' },
          {
            key: 'gjinia',
            label: 'Gjinia',
            render: (item) => (
              <span className={`px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-wider ${item.gjinia === 'M' ? 'bg-health-accent/10 text-health-accent border border-health-accent/20' : 'bg-pink-500/10 text-pink-400 border border-pink-500/20'}`}>
                {item.gjinia === 'M' ? 'Mashkull' : 'Femër'}
              </span>
            ),
          },
        ]}
      />
    </div>
  )
}
