import { useState, useEffect } from 'react'
import { Calendar, Printer, FileSpreadsheet, Repeat } from 'lucide-react'
import toast from 'react-hot-toast'
import CrudPage from '../../components/crud/CrudPage'
import { TerminForm } from '../../components/crud/Forms'
import { terminetApi, klientetApi, sherbiimetApi, terapistetApi, recurringTerminetApi } from '../../api/api'
import { StatusBadge, Modal } from '../../components/ui/index'

function RecurringDialog({ open, onClose, klientet, sherbimet, terapistet, onCreated }) {
  const [form, setForm] = useState({
    klientId: '', sherbimId: '', terapistId: '',
    dataFillimit: '', oraFillimit: '09:00', oraMbarimit: '10:00',
    intervaliJave: 1, hereNumri: 4, shenimet: '',
  })
  const [submitting, setSubmitting] = useState(false)
  const [result, setResult] = useState(null)

  useEffect(() => { if (open) { setResult(null) } }, [open])

  const set = (k, v) => setForm(s => ({ ...s, [k]: v }))

  const submit = async (e) => {
    e?.preventDefault?.()
    if (!form.klientId || !form.sherbimId || !form.terapistId || !form.dataFillimit) {
      return toast.error('Plotësoni të gjitha fushat e kërkuara.')
    }
    setSubmitting(true)
    try {
      const payload = {
        klientId: Number(form.klientId),
        sherbimId: Number(form.sherbimId),
        terapistId: Number(form.terapistId),
        dataFillimit: form.dataFillimit,
        oraFillimit: form.oraFillimit + ':00',
        oraMbarimit: form.oraMbarimit + ':00',
        intervaliJave: Number(form.intervaliJave),
        hereNumri: Number(form.hereNumri),
        statusi: 'Planifikuar',
        shenimet: form.shenimet || null,
      }
      const res = await recurringTerminetApi.create(payload)
      setResult(res.data)
      toast.success(`U krijuan ${res.data.krijuar} termine.`)
      onCreated?.()
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Krijimi dështoi.')
    } finally { setSubmitting(false) }
  }

  return (
    <Modal isOpen={open} onClose={onClose} title="Termin i përsëritur (recurring)" size="lg">
      <form onSubmit={submit} className="space-y-4">
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <div>
            <label className="block text-xs font-bold text-health-secondary uppercase tracking-wider mb-1">Klienti</label>
            <select value={form.klientId} onChange={(e) => set('klientId', e.target.value)} className="input w-full" required>
              <option value="">— Zgjidh —</option>
              {klientet.map(k => <option key={k.klientId} value={k.klientId}>{k.emri} {k.mbiemri}</option>)}
            </select>
          </div>
          <div>
            <label className="block text-xs font-bold text-health-secondary uppercase tracking-wider mb-1">Shërbimi</label>
            <select value={form.sherbimId} onChange={(e) => set('sherbimId', e.target.value)} className="input w-full" required>
              <option value="">— Zgjidh —</option>
              {sherbimet.map(s => <option key={s.sherbimId} value={s.sherbimId}>{s.emriSherbimit}</option>)}
            </select>
          </div>
          <div>
            <label className="block text-xs font-bold text-health-secondary uppercase tracking-wider mb-1">Terapisti</label>
            <select value={form.terapistId} onChange={(e) => set('terapistId', e.target.value)} className="input w-full" required>
              <option value="">— Zgjidh —</option>
              {terapistet.map(t => <option key={t.terapistId} value={t.terapistId}>{t.emri} {t.mbiemri}</option>)}
            </select>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <div>
            <label className="block text-xs font-bold text-health-secondary uppercase tracking-wider mb-1">Data fillestare</label>
            <input type="date" value={form.dataFillimit} onChange={(e) => set('dataFillimit', e.target.value)} className="input w-full" required />
          </div>
          <div>
            <label className="block text-xs font-bold text-health-secondary uppercase tracking-wider mb-1">Ora fillimit</label>
            <input type="time" value={form.oraFillimit} onChange={(e) => set('oraFillimit', e.target.value)} className="input w-full" required />
          </div>
          <div>
            <label className="block text-xs font-bold text-health-secondary uppercase tracking-wider mb-1">Ora mbarimit</label>
            <input type="time" value={form.oraMbarimit} onChange={(e) => set('oraMbarimit', e.target.value)} className="input w-full" required />
          </div>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-bold text-health-secondary uppercase tracking-wider mb-1">Çdo (javë)</label>
            <select value={form.intervaliJave} onChange={(e) => set('intervaliJave', e.target.value)} className="input w-full">
              {[1,2,3,4].map(n => <option key={n} value={n}>{n} javë</option>)}
            </select>
          </div>
          <div>
            <label className="block text-xs font-bold text-health-secondary uppercase tracking-wider mb-1">Numri i seancave</label>
            <input type="number" min="1" max="52" value={form.hereNumri} onChange={(e) => set('hereNumri', e.target.value)} className="input w-full" />
          </div>
        </div>

        <div>
          <label className="block text-xs font-bold text-health-secondary uppercase tracking-wider mb-1">Shënime</label>
          <textarea value={form.shenimet} onChange={(e) => set('shenimet', e.target.value)} rows={2} className="input w-full" />
        </div>

        {result && (
          <div className={`p-4 rounded-xl border ${result.krijuar > 0 ? 'bg-emerald-500/10 border-emerald-500/30' : 'bg-orange-500/10 border-orange-500/30'}`}>
            <p className="text-sm font-bold text-health-primary">
              ✓ U krijuan: {result.krijuar} · Anashkaluar: {result.anashkaluar}
            </p>
            {result.mesazhet?.length > 0 && (
              <ul className="mt-2 text-xs text-health-secondary space-y-0.5 list-disc pl-5">
                {result.mesazhet.map((m, i) => <li key={i}>{m}</li>)}
              </ul>
            )}
          </div>
        )}

        <div className="flex justify-end gap-2 pt-2">
          <button type="button" onClick={onClose} className="btn-secondary px-4 py-2">Mbyll</button>
          <button type="submit" disabled={submitting} className="btn-primary px-6 py-2 disabled:opacity-50">
            {submitting ? 'Duke krijuar...' : 'Krijo termine të përsëritura'}
          </button>
        </div>
      </form>
    </Modal>
  )
}

export function TerminetPage() {
  const [klientet, setKlientet] = useState([])
  const [sherbimet, setSherbimet] = useState([])
  const [terapistet, setTerapistet] = useState([])
  const [recurringOpen, setRecurringOpen] = useState(false)
  const [reloadKey, setReloadKey] = useState(0)

  const toArray = (payload) => {
    if (Array.isArray(payload?.data)) return payload.data
    if (Array.isArray(payload)) return payload
    return []
  }

  useEffect(() => {
    klientetApi.getAll().then((r) => setKlientet(toArray(r.data))).catch(() => {})
    sherbiimetApi.getAll().then((r) => setSherbimet(toArray(r.data))).catch(() => {})
    terapistetApi.getAll().then((r) => setTerapistet(toArray(r.data))).catch(() => {})
  }, [])

  return (
    <div className="space-y-3">
      <div className="flex justify-end gap-3 print:hidden">
        <button
          onClick={() => setRecurringOpen(true)}
          className="btn-secondary flex items-center gap-2 px-6 py-2 bg-health-surface border border-health-border rounded-lg hover:bg-health-hover text-sm font-bold"
        >
          <Repeat className="h-4 w-4" />
          Recurring
        </button>
        <button onClick={() => window.print()} className="btn-secondary px-6">
          <Printer className="h-4 w-4" />
          Printo listën
        </button>
        <button
          onClick={() => window.open('http://localhost:5077/api/export/terminet/excel', '_blank')}
          className="flex items-center gap-2 px-6 py-2 bg-health-accent text-white rounded-lg hover:brightness-110 active:scale-95 transition-all text-sm font-bold shadow-lg shadow-health-accent/20"
        >
          <FileSpreadsheet className="h-4 w-4" />
          Excel
        </button>
      </div>
      <CrudPage
        key={reloadKey}
        title="Terminet"
        subtitle="Menaxhimi i termineve dhe rezervimeve"
        emptyIcon={Calendar}
        api={terminetApi}
        FormComponent={TerminForm}
        idKey="terminId"
        searchKeys={['statusi', 'klientId', 'sherbimId']}
        extraFormProps={{ klientet, sherbimet, terapistet }}
        columns={[
          {
            key: 'klientId',
            label: 'Klienti',
            render: (item) => {
              const k = klientet.find((c) => c.klientId === item.klientId)
              return k ? (
                <span className="font-bold text-health-primary">{k.emri} {k.mbiemri}</span>
              ) : item.klientId ?? '-'
            },
          },
          {
            key: 'sherbimId',
            label: 'Shërbimi',
            render: (item) => {
              const s = sherbimet.find((x) => x.sherbimId === item.sherbimId)
              return s?.emriSherbimit ?? item.sherbimId ?? '-'
            },
          },
          {
            key: 'dataTerminit',
            label: 'Data',
            render: (item) => (item.dataTerminit ? item.dataTerminit.slice(0, 10) : '-'),
          },
          {
            key: 'oraFillimit',
            label: 'Ora',
            render: (item) =>
              item.oraFillimit && item.oraMbarimit
                ? `${item.oraFillimit} – ${item.oraMbarimit}`
                : item.oraFillimit ?? '-',
          },
          {
            key: 'statusi',
            label: 'Statusi',
            render: (item) => <StatusBadge status={item.statusi} />,
          },
        ]}
      />

      <RecurringDialog
        open={recurringOpen}
        onClose={() => setRecurringOpen(false)}
        klientet={klientet}
        sherbimet={sherbimet}
        terapistet={terapistet}
        onCreated={() => setReloadKey(k => k + 1)}
      />
    </div>
  )
}
