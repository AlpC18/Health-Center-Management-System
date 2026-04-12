import { useState } from 'react'
import { Activity } from 'lucide-react'
import CrudPage from '../../components/crud/CrudPage'
import { auditlogsApi } from '../../api/api'

export function AuditLogsPage() {
  const [entityFilter, setEntityFilter] = useState('')
  const [actionFilter, setActionFilter] = useState('')

  const getActionColor = (action) => {
    switch (action) {
      case 'CREATE': return 'bg-health-accent/10 text-health-accent border border-health-accent/20'
      case 'UPDATE': return 'bg-orange-500/10 text-orange-400 border border-orange-500/20'
      case 'DELETE': return 'bg-health-brand/10 text-health-brand border border-health-brand/20'
      default: return 'bg-health-surface text-health-secondary border border-health-border'
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-col sm:flex-row gap-6 bg-health-surface p-6 rounded-xl border border-health-border shadow-xl">
        <div className="flex items-center gap-3">
          <label className="text-sm font-bold text-health-secondary uppercase tracking-wider">Entiteti:</label>
          <input 
            type="text"
            placeholder="Psh: Klient"
            className="input w-48 py-2"
            value={entityFilter}
            onChange={(e) => setEntityFilter(e.target.value)}
          />
        </div>
        <div className="flex items-center gap-3">
          <label className="text-sm font-bold text-health-secondary uppercase tracking-wider">Veprimi:</label>
          <select 
            className="input w-48 py-2"
            value={actionFilter}
            onChange={(e) => setActionFilter(e.target.value)}
          >
            <option value="">Të gjitha</option>
            <option value="CREATE">KRIJIM</option>
            <option value="UPDATE">PËRDITËSIM</option>
            <option value="DELETE">FSHIRJE</option>
          </select>
        </div>
      </div>

      <CrudPage
        title="Audit Logs"
        subtitle="Historia e veprimeve në sistem"
        emptyIcon={Activity}
        api={auditlogsApi}
        idKey="id"
        hideAdd={true}
        hideEdit={true}
        hideDelete={true}
        columns={[
          {
            key: 'action',
            label: 'Veprimi',
            render: (item) => (
              <span className={`px-2 py-1 rounded-full text-xs font-bold ${getActionColor(item.action)}`}>
                {item.action}
              </span>
            )
          },
          { key: 'entity', label: 'Tabela' },
          { key: 'entityId', label: 'ID e Rreshtit' },
          { key: 'userName', label: 'Përdoruesi' },
          { 
            key: 'createdAt', 
            label: 'Koha',
            render: (item) => new Date(item.createdAt).toLocaleString('sq-AL')
          },
        ]}
      />
    </div>
  )
}
