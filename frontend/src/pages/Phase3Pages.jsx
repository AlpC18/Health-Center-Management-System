import { useEffect, useMemo, useState } from 'react'
import toast from 'react-hot-toast'
import {
  Bell, Check, Database, FileText, KeyRound, MapPin, Save, ShieldCheck, Trash2,
} from 'lucide-react'
import {
  consentApi,
  lokacionetApi,
  notificationsApi,
  privacyApi,
  templatesApi,
  twoFactorApi,
} from '../api/api'
import { PageLoader } from '../components/ui'
import useAuthStore from '../store/authStore'

const fmtDate = (value) => (value ? new Date(value).toLocaleString('sq-AL') : '-')

export function NotificationsInboxPage() {
  const [items, setItems] = useState([])
  const [loading, setLoading] = useState(true)

  const load = async () => {
    setLoading(true)
    try {
      const res = await notificationsApi.getAll({ limit: 100 })
      setItems(res.data || [])
    } catch {
      toast.error('Njoftimet nuk mund te ngarkohen.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  const markRead = async (id) => {
    await notificationsApi.markRead(id)
    setItems((prev) => prev.map((n) => (n.notificationId === id ? { ...n, isRead: true, readAt: new Date().toISOString() } : n)))
  }

  if (loading) return <PageLoader />

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-health-primary">Njoftimet</h1>
          <p className="text-sm text-health-secondary mt-1">{items.filter((n) => !n.isRead).length} te palexuara</p>
        </div>
        <button onClick={load} className="btn-secondary px-4 py-2">Rifresko</button>
      </div>

      <div className="card overflow-hidden">
        {items.length === 0 ? (
          <div className="py-14 text-center text-sm text-health-secondary">
            <Bell className="w-9 h-9 mx-auto mb-3 opacity-40" />
            Nuk ka njoftime.
          </div>
        ) : (
          <div className="divide-y divide-health-border">
            {items.map((n) => (
              <div key={n.notificationId} className={`p-4 flex items-start gap-4 ${n.isRead ? '' : 'bg-health-brand/5'}`}>
                <div className="w-10 h-10 rounded-xl bg-health-accent/10 flex items-center justify-center flex-shrink-0">
                  <Bell className="w-5 h-5 text-health-accent" />
                </div>
                <div className="min-w-0 flex-1">
                  <div className="flex items-center gap-2">
                    <p className="font-bold text-health-primary">{n.title}</p>
                    {!n.isRead && <span className="badge-blue">E re</span>}
                  </div>
                  <p className="text-sm text-health-secondary mt-1">{n.message}</p>
                  <p className="text-xs text-health-secondary mt-2">{fmtDate(n.createdAt)}</p>
                </div>
                {!n.isRead && (
                  <button onClick={() => markRead(n.notificationId)} className="btn-secondary px-3 py-1.5 text-xs">
                    Lexuar
                  </button>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}

export function TwoFactorSetupPage() {
  const [status, setStatus] = useState(null)
  const [enrollment, setEnrollment] = useState(null)
  const [code, setCode] = useState('')
  const [loading, setLoading] = useState(true)

  const load = async () => {
    setLoading(true)
    try {
      const res = await twoFactorApi.status()
      setStatus(res.data)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  const enroll = async () => {
    const res = await twoFactorApi.enroll()
    setEnrollment(res.data)
    setStatus({ enabled: false, enrolled: true })
  }

  const verify = async () => {
    await twoFactorApi.verify(code)
    toast.success('2FA u aktivizua.')
    setCode('')
    setEnrollment(null)
    load()
  }

  const disable = async () => {
    await twoFactorApi.disable(code)
    toast.success('2FA u caktivizua.')
    setCode('')
    setEnrollment(null)
    load()
  }

  if (loading) return <PageLoader />

  return (
    <div className="max-w-2xl space-y-5">
      <div>
        <h1 className="text-2xl font-bold text-health-primary">2FA</h1>
        <p className="text-sm text-health-secondary mt-1">Statusi: {status?.enabled ? 'Aktiv' : 'Joaktiv'}</p>
      </div>

      <div className="card p-5 space-y-4">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-health-accent/10 flex items-center justify-center">
            <KeyRound className="w-5 h-5 text-health-accent" />
          </div>
          <div>
            <p className="font-bold text-health-primary">Authenticator app</p>
            <p className="text-sm text-health-secondary">Përdorni Google Authenticator, 1Password ose app të ngjashëm.</p>
          </div>
        </div>

        {!status?.enabled && !enrollment && (
          <button onClick={enroll} className="btn-primary px-5 py-2">Nis konfigurimin</button>
        )}

        {enrollment && (
          <div className="space-y-3">
            <div className="rounded-lg border border-health-border bg-health-bg p-3">
              <p className="text-xs font-bold text-health-secondary uppercase mb-1">Secret</p>
              <p className="font-mono text-sm text-health-primary break-all">{enrollment.secret}</p>
            </div>
            <div className="rounded-lg border border-health-border bg-health-bg p-3">
              <p className="text-xs font-bold text-health-secondary uppercase mb-1">OTP Auth URI</p>
              <p className="font-mono text-xs text-health-secondary break-all">{enrollment.otpauthUri}</p>
            </div>
          </div>
        )}

        {(enrollment || status?.enabled) && (
          <div className="flex flex-wrap items-end gap-3">
            <label className="flex-1 min-w-[180px]">
              <span className="label">Kodi</span>
              <input className="input" inputMode="numeric" value={code} onChange={(e) => setCode(e.target.value)} placeholder="123456" />
            </label>
            {!status?.enabled ? (
              <button onClick={verify} className="btn-primary px-5 py-2">Verifiko</button>
            ) : (
              <button onClick={disable} className="btn-danger px-5 py-2">Caktivizo</button>
            )}
          </div>
        )}
      </div>
    </div>
  )
}

export function ConsentPage() {
  const [items, setItems] = useState([])
  const [loading, setLoading] = useState(true)

  const load = async () => {
    setLoading(true)
    try {
      const res = await consentApi.mine()
      setItems(res.data || [])
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  const accept = async () => {
    await consentApi.accept({ consentType: 'PrivacyPolicy', version: 'v1', accepted: true })
    toast.success('Consent u ruajt.')
    load()
  }

  if (loading) return <PageLoader />

  return (
    <div className="space-y-5">
      <div>
        <h1 className="text-2xl font-bold text-health-primary">Consent</h1>
        <p className="text-sm text-health-secondary mt-1">Versioni aktual: v1</p>
      </div>
      <div className="card p-5 space-y-4">
        <div className="flex items-start gap-3">
          <ShieldCheck className="w-6 h-6 text-health-accent mt-1" />
          <div>
            <p className="font-bold text-health-primary">Politika e privatesise</p>
            <p className="text-sm text-health-secondary mt-1">Pranimi ruhet ne log me date, version dhe llogarine aktive.</p>
          </div>
        </div>
        <button onClick={accept} className="btn-primary px-5 py-2">
          <Check className="w-4 h-4" />
          Pranoj
        </button>
      </div>
      <div className="card overflow-hidden">
        <div className="px-5 py-3 border-b border-health-border font-bold text-health-primary">Historiku</div>
        <div className="divide-y divide-health-border">
          {items.map((item) => (
            <div key={item.consentLogId} className="p-4 flex items-center justify-between">
              <div>
                <p className="font-semibold text-health-primary">{item.consentType} {item.version}</p>
                <p className="text-xs text-health-secondary">{fmtDate(item.createdAt)}</p>
              </div>
              <span className={item.accepted ? 'badge-green' : 'badge-red'}>{item.accepted ? 'Pranuar' : 'Refuzuar'}</span>
            </div>
          ))}
          {items.length === 0 && <div className="p-6 text-sm text-health-secondary text-center">Nuk ka log consent-i.</div>}
        </div>
      </div>
    </div>
  )
}

export function PrivacySelfServicePage() {
  const clearAuth = useAuthStore((s) => s.clearAuth)
  const [exportData, setExportData] = useState(null)
  const [loading, setLoading] = useState(false)

  const exportMine = async () => {
    setLoading(true)
    try {
      const res = await privacyApi.exportMine()
      setExportData(res.data)
      toast.success('Eksporti u gjenerua.')
    } finally {
      setLoading(false)
    }
  }

  const eraseMine = async () => {
    if (!window.confirm('A jeni te sigurt? Llogaria do te caktivizohet dhe te dhenat personale do te anonimizohen.')) return
    await privacyApi.eraseMine()
    clearAuth()
    window.location.href = '/login'
  }

  return (
    <div className="space-y-5">
      <div>
        <h1 className="text-2xl font-bold text-health-primary">Te Dhenat e Mia</h1>
        <p className="text-sm text-health-secondary mt-1">Eksport JSON dhe kerkese per fshirje te bute.</p>
      </div>
      <div className="grid md:grid-cols-2 gap-4">
        <div className="card p-5 space-y-3">
          <Database className="w-7 h-7 text-health-accent" />
          <p className="font-bold text-health-primary">Eksporto te dhenat</p>
          <button onClick={exportMine} disabled={loading} className="btn-primary px-5 py-2">Gjenero eksportin</button>
        </div>
        <div className="card p-5 space-y-3">
          <Trash2 className="w-7 h-7 text-red-400" />
          <p className="font-bold text-health-primary">Fshirje GDPR</p>
          <button onClick={eraseMine} className="btn-danger px-5 py-2">Anonimizo llogarine</button>
        </div>
      </div>
      {exportData && (
        <pre className="card p-4 overflow-auto max-h-[520px] text-xs text-health-secondary">
{JSON.stringify(exportData, null, 2)}
        </pre>
      )}
    </div>
  )
}

const emptyTemplate = { key: 'appointment-reminder', name: '', channel: 'Email', subject: '', body: '', active: true }

export function TemplatesAdminPage() {
  const [items, setItems] = useState([])
  const [form, setForm] = useState(emptyTemplate)
  const [editingId, setEditingId] = useState(null)
  const [loading, setLoading] = useState(true)

  const load = async () => {
    setLoading(true)
    try {
      const res = await templatesApi.getAll()
      setItems(res.data || [])
    } finally {
      setLoading(false)
    }
  }
  useEffect(() => { load() }, [])

  const save = async (e) => {
    e.preventDefault()
    if (editingId) await templatesApi.update(editingId, form)
    else await templatesApi.create(form)
    toast.success('Template u ruajt.')
    setForm(emptyTemplate)
    setEditingId(null)
    load()
  }

  const edit = (item) => {
    setEditingId(item.templateId)
    setForm({ key: item.key, name: item.name, channel: item.channel, subject: item.subject || '', body: item.body, active: item.active })
  }

  const remove = async (id) => {
    await templatesApi.delete(id)
    load()
  }

  if (loading) return <PageLoader />

  return (
    <div className="space-y-5">
      <h1 className="text-2xl font-bold text-health-primary">Templates</h1>
      <form onSubmit={save} className="card p-5 grid md:grid-cols-2 gap-4">
        <label><span className="label">Key</span><input className="input" value={form.key} onChange={(e) => setForm((p) => ({ ...p, key: e.target.value }))} /></label>
        <label><span className="label">Emri</span><input className="input" value={form.name} onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))} /></label>
        <label><span className="label">Channel</span><select className="input" value={form.channel} onChange={(e) => setForm((p) => ({ ...p, channel: e.target.value }))}><option>Email</option><option>Sms</option></select></label>
        <label><span className="label">Subject</span><input className="input" value={form.subject} onChange={(e) => setForm((p) => ({ ...p, subject: e.target.value }))} /></label>
        <label className="md:col-span-2"><span className="label">Body</span><textarea rows={5} className="input" value={form.body} onChange={(e) => setForm((p) => ({ ...p, body: e.target.value }))} /></label>
        <label className="flex items-center gap-2 text-sm text-health-secondary"><input type="checkbox" checked={form.active} onChange={(e) => setForm((p) => ({ ...p, active: e.target.checked }))} /> Aktiv</label>
        <div className="md:col-span-2 flex gap-2">
          <button className="btn-primary px-5 py-2" type="submit"><Save className="w-4 h-4" />Ruaj</button>
          {editingId && <button type="button" className="btn-secondary px-5 py-2" onClick={() => { setEditingId(null); setForm(emptyTemplate) }}>Anulo</button>}
        </div>
      </form>
      <div className="card overflow-hidden">
        {items.map((item) => (
          <div key={item.templateId} className="p-4 border-b border-health-border flex items-start justify-between gap-4">
            <div>
              <p className="font-bold text-health-primary">{item.name} <span className="text-xs text-health-secondary">({item.key}/{item.channel})</span></p>
              <p className="text-sm text-health-secondary mt-1">{item.subject}</p>
            </div>
            <div className="flex gap-2">
              <button onClick={() => edit(item)} className="btn-secondary px-3 py-1.5 text-xs">Edit</button>
              <button onClick={() => remove(item.templateId)} className="btn-danger px-3 py-1.5 text-xs">Fshi</button>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}

const emptyLocation = { emri: '', adresa: '', telefoni: '', aktiv: true }

export function LocationsAdminPage() {
  const [items, setItems] = useState([])
  const [form, setForm] = useState(emptyLocation)
  const [editingId, setEditingId] = useState(null)

  const load = async () => {
    const res = await lokacionetApi.getAll()
    setItems(res.data || [])
  }
  useEffect(() => { load() }, [])

  const save = async (e) => {
    e.preventDefault()
    if (editingId) await lokacionetApi.update(editingId, form)
    else await lokacionetApi.create(form)
    setForm(emptyLocation)
    setEditingId(null)
    load()
  }

  const edit = (item) => {
    setEditingId(item.lokacioniId)
    setForm({ emri: item.emri, adresa: item.adresa || '', telefoni: item.telefoni || '', aktiv: item.aktiv })
  }

  const activeCount = useMemo(() => items.filter((i) => i.aktiv).length, [items])

  return (
    <div className="space-y-5">
      <div>
        <h1 className="text-2xl font-bold text-health-primary">Lokacionet</h1>
        <p className="text-sm text-health-secondary mt-1">{activeCount} aktive</p>
      </div>
      <form onSubmit={save} className="card p-5 grid md:grid-cols-3 gap-4">
        <label><span className="label">Emri</span><input required className="input" value={form.emri} onChange={(e) => setForm((p) => ({ ...p, emri: e.target.value }))} /></label>
        <label><span className="label">Adresa</span><input className="input" value={form.adresa} onChange={(e) => setForm((p) => ({ ...p, adresa: e.target.value }))} /></label>
        <label><span className="label">Telefoni</span><input className="input" value={form.telefoni} onChange={(e) => setForm((p) => ({ ...p, telefoni: e.target.value }))} /></label>
        <label className="flex items-center gap-2 text-sm text-health-secondary"><input type="checkbox" checked={form.aktiv} onChange={(e) => setForm((p) => ({ ...p, aktiv: e.target.checked }))} /> Aktiv</label>
        <div className="md:col-span-3 flex gap-2">
          <button className="btn-primary px-5 py-2"><MapPin className="w-4 h-4" />Ruaj</button>
          {editingId && <button type="button" className="btn-secondary px-5 py-2" onClick={() => { setEditingId(null); setForm(emptyLocation) }}>Anulo</button>}
        </div>
      </form>
      <div className="grid md:grid-cols-2 gap-4">
        {items.map((item) => (
          <div key={item.lokacioniId} className="card p-4 flex items-start justify-between gap-4">
            <div>
              <p className="font-bold text-health-primary">{item.emri}</p>
              <p className="text-sm text-health-secondary">{item.adresa || '-'}</p>
              <p className="text-xs text-health-secondary mt-1">{item.telefoni || '-'}</p>
            </div>
            <button onClick={() => edit(item)} className="btn-secondary px-3 py-1.5 text-xs">Edit</button>
          </div>
        ))}
      </div>
    </div>
  )
}

export default NotificationsInboxPage
