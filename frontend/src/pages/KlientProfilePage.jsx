// Per-client clinical profile: notes + body measurements + loyalty points.
// Visible to Admin & Therapist (full access) and to the Klient themselves
// (read-only, non-private notes; full access to their own measurements/points).
import { useEffect, useState, useCallback } from 'react'
import { useParams, Link } from 'react-router-dom'
import toast from 'react-hot-toast'
import {
  ArrowLeft, FileText, Activity, Award, Plus, Trash2, User, Scale,
} from 'lucide-react'
import {
  LineChart, Line, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid,
} from 'recharts'
import {
  klientetApi, klientShenimeApi, klientMatjetApi, klientPikatApi,
} from '../api/api'
import { Spinner, Modal } from '../components/ui/index'
import useAuthStore from '../store/authStore'

const TABS = [
  { id: 'shenime', label: 'Shënime klinike', icon: FileText },
  { id: 'matjet', label: 'Matjet trupore', icon: Activity },
  { id: 'pikat', label: 'Pikët e besnikërisë', icon: Award },
]

function fmtDate(s) {
  return s ? new Date(s).toLocaleDateString('sq-AL') : '—'
}

function NoteFormModal({ open, onClose, klientId, onSaved }) {
  const [tipi, setTipi] = useState('Vezhgim')
  const [permbajtja, setPermbajtja] = useState('')
  const [privat, setPrivat] = useState(false)
  const [saving, setSaving] = useState(false)
  useEffect(() => { if (open) { setTipi('Vezhgim'); setPermbajtja(''); setPrivat(false) } }, [open])

  const submit = async (e) => {
    e?.preventDefault?.()
    if (!permbajtja.trim()) return toast.error('Përmbajtja nuk mund të jetë bosh.')
    setSaving(true)
    try {
      await klientShenimeApi.create({ klientId, tipi, permbajtja, privat })
      toast.success('Shënimi u ruajt.')
      onSaved?.()
      onClose()
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Ruajtja dështoi.')
    } finally { setSaving(false) }
  }

  return (
    <Modal isOpen={open} onClose={onClose} title="Shënim i ri klinik" size="lg">
      <form onSubmit={submit} className="space-y-4">
        <div>
          <label className="block text-xs font-bold text-health-secondary uppercase tracking-wider mb-1.5">Tipi</label>
          <select value={tipi} onChange={(e) => setTipi(e.target.value)} className="input w-full">
            <option value="Anamnese">Anamnezë</option>
            <option value="Trajtim">Trajtim</option>
            <option value="Vezhgim">Vëzhgim</option>
            <option value="Plan">Plan</option>
            <option value="Tjeter">Tjetër</option>
          </select>
        </div>
        <div>
          <label className="block text-xs font-bold text-health-secondary uppercase tracking-wider mb-1.5">Përmbajtja</label>
          <textarea value={permbajtja} onChange={(e) => setPermbajtja(e.target.value)} rows={6} className="input w-full" required />
        </div>
        <label className="flex items-center gap-2 text-sm cursor-pointer">
          <input type="checkbox" checked={privat} onChange={(e) => setPrivat(e.target.checked)} className="w-4 h-4 rounded" />
          Privat (terapist + admin)
        </label>
        <div className="flex justify-end gap-2 pt-2">
          <button type="button" onClick={onClose} className="btn-secondary px-4 py-2">Anulo</button>
          <button type="submit" disabled={saving} className="btn-primary px-6 py-2 disabled:opacity-50">
            {saving ? 'Duke ruajtur...' : 'Ruaj'}
          </button>
        </div>
      </form>
    </Modal>
  )
}

function MatjeFormModal({ open, onClose, klientId, onSaved }) {
  const [form, setForm] = useState({ peshaKg: '', gjatesiaCm: '', yndyraTrupore: '', beliCm: '', kofshaCm: '', shenim: '' })
  const [saving, setSaving] = useState(false)
  useEffect(() => { if (open) setForm({ peshaKg: '', gjatesiaCm: '', yndyraTrupore: '', beliCm: '', kofshaCm: '', shenim: '' }) }, [open])

  const num = (v) => v === '' ? null : Number(v)
  const submit = async (e) => {
    e?.preventDefault?.()
    setSaving(true)
    try {
      await klientMatjetApi.create({
        klientId,
        peshaKg: num(form.peshaKg),
        gjatesiaCm: num(form.gjatesiaCm),
        yndyraTrupore: num(form.yndyraTrupore),
        beliCm: num(form.beliCm),
        kofshaCm: num(form.kofshaCm),
        shenim: form.shenim || null,
      })
      toast.success('Matja u shtua.')
      onSaved?.(); onClose()
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Ruajtja dështoi.')
    } finally { setSaving(false) }
  }

  const fields = [
    { key: 'peshaKg', label: 'Pesha (kg)', step: '0.1' },
    { key: 'gjatesiaCm', label: 'Gjatësia (cm)', step: '0.1' },
    { key: 'yndyraTrupore', label: 'Yndyra trupore (%)', step: '0.1' },
    { key: 'beliCm', label: 'Beli (cm)', step: '0.1' },
    { key: 'kofshaCm', label: 'Kofshët (cm)', step: '0.1' },
  ]

  return (
    <Modal isOpen={open} onClose={onClose} title="Matje e re" size="lg">
      <form onSubmit={submit} className="space-y-4">
        <div className="grid grid-cols-2 gap-3">
          {fields.map(f => (
            <div key={f.key}>
              <label className="block text-xs font-bold text-health-secondary uppercase tracking-wider mb-1">{f.label}</label>
              <input
                type="number" step={f.step} value={form[f.key]}
                onChange={(e) => setForm(s => ({ ...s, [f.key]: e.target.value }))}
                className="input w-full"
              />
            </div>
          ))}
        </div>
        <div>
          <label className="block text-xs font-bold text-health-secondary uppercase tracking-wider mb-1">Shënim</label>
          <textarea value={form.shenim} onChange={(e) => setForm(s => ({ ...s, shenim: e.target.value }))} rows={2} className="input w-full" />
        </div>
        <div className="flex justify-end gap-2 pt-2">
          <button type="button" onClick={onClose} className="btn-secondary px-4 py-2">Anulo</button>
          <button type="submit" disabled={saving} className="btn-primary px-6 py-2 disabled:opacity-50">
            {saving ? 'Duke ruajtur...' : 'Ruaj matjen'}
          </button>
        </div>
      </form>
    </Modal>
  )
}

export default function KlientProfilePage() {
  const { id } = useParams()
  const klientId = Number(id)
  const { user } = useAuthStore()
  const role = user?.role
  const canWrite = role === 'Admin' || role === 'Therapist'

  const [klient, setKlient] = useState(null)
  const [tab, setTab] = useState('shenime')
  const [loading, setLoading] = useState(true)
  const [notes, setNotes] = useState([])
  const [matjet, setMatjet] = useState([])
  const [pikat, setPikat] = useState([])
  const [balance, setBalance] = useState(null)
  const [noteOpen, setNoteOpen] = useState(false)
  const [matjeOpen, setMatjeOpen] = useState(false)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const [kRes, nRes, mRes, pRes, bRes] = await Promise.all([
        klientetApi.getById(klientId),
        klientShenimeApi.forKlient(klientId).catch(() => ({ data: { data: [] } })),
        klientMatjetApi.forKlient(klientId).catch(() => ({ data: { data: [] } })),
        klientPikatApi.list(klientId).catch(() => ({ data: { data: [] } })),
        klientPikatApi.balance(klientId).catch(() => ({ data: null })),
      ])
      setKlient(kRes.data)
      setNotes(nRes.data?.data ?? [])
      setMatjet(mRes.data?.data ?? [])
      setPikat(pRes.data?.data ?? [])
      setBalance(bRes.data)
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Ngarkimi dështoi.')
    } finally { setLoading(false) }
  }, [klientId])

  useEffect(() => { load() }, [load])

  const deleteNote = async (sid) => {
    if (!confirm('Fshij këtë shënim?')) return
    try {
      await klientShenimeApi.delete(sid)
      toast.success('Shënimi u fshi.')
      load()
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Fshirja dështoi.')
    }
  }
  const deleteMatje = async (mid) => {
    if (!confirm('Fshij këtë matje?')) return
    try {
      await klientMatjetApi.delete(mid)
      toast.success('Matja u fshi.')
      load()
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Fshirja dështoi.')
    }
  }

  if (loading && !klient) {
    return <div className="flex items-center justify-center py-20"><Spinner size="lg" /></div>
  }
  if (!klient) {
    return <div className="card p-8 text-center text-health-secondary">Klienti nuk u gjet.</div>
  }

  // Build measurement chart data (oldest → newest), trimming to numeric values
  const chartData = [...matjet]
    .sort((a, b) => new Date(a.dataMatjes) - new Date(b.dataMatjes))
    .map(m => ({
      date: new Date(m.dataMatjes).toLocaleDateString('sq-AL', { month: 'short', day: 'numeric' }),
      pesha: m.peshaKg != null ? Number(m.peshaKg) : null,
      yndyra: m.yndyraTrupore != null ? Number(m.yndyraTrupore) : null,
      bmi: m.bmi != null ? Number(m.bmi) : null,
    }))

  return (
    <div className="space-y-6">
      {/* Back + header */}
      <div className="flex items-center justify-between">
        <Link to="/klientet" className="btn-secondary flex items-center gap-2 px-4 py-2 text-sm">
          <ArrowLeft className="w-4 h-4" /> Lista e klientëve
        </Link>
      </div>

      <div className="card p-6 flex flex-wrap items-start gap-6">
        <div className="w-16 h-16 bg-health-brand/10 rounded-2xl flex items-center justify-center">
          <User className="w-8 h-8 text-health-brand" />
        </div>
        <div className="flex-1 min-w-0">
          <h1 className="text-3xl font-black text-health-primary tracking-tighter">{klient.emri} {klient.mbiemri}</h1>
          <p className="text-sm text-health-secondary mt-1">{klient.email}{klient.telefoni ? ` · ${klient.telefoni}` : ''}</p>
          {klient.kushtetShendetesore && (
            <p className="mt-2 text-xs bg-orange-500/10 border border-orange-500/30 text-orange-400 px-3 py-1.5 rounded-lg inline-block">
              ⚕ {klient.kushtetShendetesore}
            </p>
          )}
        </div>
        {balance && (
          <div className="bg-health-accent/10 border border-health-accent/30 rounded-2xl px-5 py-3 text-right">
            <p className="text-xs font-bold text-health-secondary uppercase tracking-wider">Bilanci i pikëve</p>
            <p className="text-3xl font-black text-health-accent">{balance.balanca}</p>
            <p className="text-[10px] text-health-secondary mt-1">
              fituar {balance.fituarTotal} · përdorur {balance.shperblerTotal}
            </p>
          </div>
        )}
      </div>

      {/* Tabs */}
      <div className="flex border-b border-health-border">
        {TABS.map(t => {
          const Icon = t.icon
          const isActive = tab === t.id
          return (
            <button
              key={t.id}
              onClick={() => setTab(t.id)}
              className={`flex items-center gap-2 px-5 py-3 text-sm font-bold transition-all border-b-2 ${
                isActive
                  ? 'text-health-brand border-health-brand'
                  : 'text-health-secondary border-transparent hover:text-health-primary'
              }`}
            >
              <Icon className="w-4 h-4" />
              {t.label}
            </button>
          )
        })}
      </div>

      {/* === SHENIME === */}
      {tab === 'shenime' && (
        <div className="space-y-4">
          {canWrite && (
            <div className="flex justify-end">
              <button onClick={() => setNoteOpen(true)} className="btn-primary flex items-center gap-2 px-5 py-2">
                <Plus className="w-4 h-4" /> Shënim i ri
              </button>
            </div>
          )}
          {notes.length === 0 ? (
            <div className="card p-12 text-center text-health-secondary">Asnjë shënim klinik ende.</div>
          ) : (
            <div className="space-y-3">
              {notes.map(n => (
                <div key={n.shenimId} className="card p-5">
                  <div className="flex items-start justify-between gap-3">
                    <div className="flex items-center gap-3">
                      <span className="px-2 py-1 text-[10px] font-bold uppercase tracking-wider bg-health-brand/10 text-health-brand border border-health-brand/30 rounded-md">
                        {n.tipi}
                      </span>
                      <p className="text-xs text-health-secondary">{fmtDate(n.dataKrijimit)}</p>
                      {n.terapistEmri && <p className="text-xs text-health-secondary">· {n.terapistEmri}</p>}
                      {n.privat && (
                        <span className="px-2 py-0.5 text-[10px] font-bold uppercase bg-red-500/10 text-red-400 border border-red-500/30 rounded-md">
                          Privat
                        </span>
                      )}
                    </div>
                    {role === 'Admin' && (
                      <button onClick={() => deleteNote(n.shenimId)} className="p-1.5 text-health-secondary hover:text-red-400 hover:bg-red-500/10 rounded-md">
                        <Trash2 className="w-3.5 h-3.5" />
                      </button>
                    )}
                  </div>
                  <p className="mt-3 text-sm text-health-primary whitespace-pre-wrap">{n.permbajtja}</p>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* === MATJET === */}
      {tab === 'matjet' && (
        <div className="space-y-4">
          {canWrite && (
            <div className="flex justify-end">
              <button onClick={() => setMatjeOpen(true)} className="btn-primary flex items-center gap-2 px-5 py-2">
                <Plus className="w-4 h-4" /> Matje e re
              </button>
            </div>
          )}

          {chartData.length >= 2 && (
            <div className="card p-5">
              <h3 className="text-sm font-bold text-health-secondary uppercase tracking-wider mb-4 flex items-center gap-2">
                <Scale className="w-4 h-4" /> Progresi në kohë
              </h3>
              <div className="h-64">
                <ResponsiveContainer width="100%" height="100%">
                  <LineChart data={chartData} margin={{ top: 10, right: 20, left: 0, bottom: 0 }}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#1f2933" />
                    <XAxis dataKey="date" tick={{ fontSize: 11, fill: '#8b9cb3' }} />
                    <YAxis yAxisId="left" tick={{ fontSize: 11, fill: '#8b9cb3' }} />
                    <YAxis yAxisId="right" orientation="right" tick={{ fontSize: 11, fill: '#8b9cb3' }} />
                    <Tooltip contentStyle={{ background: '#161B22', border: '1px solid #30363D', borderRadius: 8 }} />
                    <Line yAxisId="left" type="monotone" dataKey="pesha" name="Pesha (kg)" stroke="#16a34a" strokeWidth={2} dot={{ r: 3 }} />
                    <Line yAxisId="right" type="monotone" dataKey="yndyra" name="Yndyra (%)" stroke="#d97706" strokeWidth={2} dot={{ r: 3 }} />
                    <Line yAxisId="right" type="monotone" dataKey="bmi" name="BMI" stroke="#2563eb" strokeWidth={2} dot={{ r: 3 }} strokeDasharray="4 4" />
                  </LineChart>
                </ResponsiveContainer>
              </div>
            </div>
          )}

          {matjet.length === 0 ? (
            <div className="card p-12 text-center text-health-secondary">Asnjë matje e regjistruar ende.</div>
          ) : (
            <div className="card overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-health-bg border-b border-health-border">
                  <tr>
                    <th className="table-th">Data</th>
                    <th className="table-th">Pesha</th>
                    <th className="table-th">Gjatësia</th>
                    <th className="table-th">BMI</th>
                    <th className="table-th">Yndyra %</th>
                    <th className="table-th">Beli</th>
                    <th className="table-th">Kofshët</th>
                    <th className="table-th text-right">Veprime</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-health-border/50">
                  {matjet.map(m => (
                    <tr key={m.matjeId} className="hover:bg-health-hover/50">
                      <td className="table-td">{fmtDate(m.dataMatjes)}</td>
                      <td className="table-td font-mono">{m.peshaKg ?? '—'}</td>
                      <td className="table-td font-mono">{m.gjatesiaCm ?? '—'}</td>
                      <td className="table-td font-mono font-bold">{m.bmi ?? '—'}</td>
                      <td className="table-td font-mono">{m.yndyraTrupore ?? '—'}</td>
                      <td className="table-td font-mono">{m.beliCm ?? '—'}</td>
                      <td className="table-td font-mono">{m.kofshaCm ?? '—'}</td>
                      <td className="table-td text-right">
                        {role === 'Admin' && (
                          <button onClick={() => deleteMatje(m.matjeId)} className="p-1.5 text-health-secondary hover:text-red-400 hover:bg-red-500/10 rounded-md">
                            <Trash2 className="w-3.5 h-3.5" />
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {/* === PIKAT === */}
      {tab === 'pikat' && (
        <div className="space-y-4">
          {pikat.length === 0 ? (
            <div className="card p-12 text-center text-health-secondary">Asnjë lëvizje pikash ende.</div>
          ) : (
            <div className="card overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-health-bg border-b border-health-border">
                  <tr>
                    <th className="table-th">Data</th>
                    <th className="table-th">Tipi</th>
                    <th className="table-th">Pikë</th>
                    <th className="table-th">Lidhje</th>
                    <th className="table-th">Shënim</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-health-border/50">
                  {pikat.map(p => (
                    <tr key={p.pikaId} className="hover:bg-health-hover/50">
                      <td className="table-td">{fmtDate(p.dataKrijimit)}</td>
                      <td className="table-td">{p.tipi}</td>
                      <td className={`table-td font-mono font-bold ${p.pike > 0 ? 'text-emerald-400' : 'text-red-400'}`}>
                        {p.pike > 0 ? `+${p.pike}` : p.pike}
                      </td>
                      <td className="table-td font-mono">{p.lidhjeId ?? '—'}</td>
                      <td className="table-td text-health-secondary">{p.shenim}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      <NoteFormModal open={noteOpen} onClose={() => setNoteOpen(false)} klientId={klientId} onSaved={load} />
      <MatjeFormModal open={matjeOpen} onClose={() => setMatjeOpen(false)} klientId={klientId} onSaved={load} />
    </div>
  )
}
