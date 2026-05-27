// Therapist self-service portal — only visible to users in role "Therapist".
// Shows the therapist's own schedule + clients + lets them mark Konfirmuar
// appointments as Perfunduar (which awards the client +10 loyalty points).
import { useEffect, useState, useCallback } from 'react'
import toast from 'react-hot-toast'
import {
  Calendar, Users, CheckCircle2, Clock, User, RefreshCw, FileText, ChevronRight,
} from 'lucide-react'
import { therapistPortalApi, klientShenimeApi } from '../api/api'
import { Spinner, Modal } from '../components/ui/index'

function StatusBadge({ statusi }) {
  const map = {
    Planifikuar: 'bg-blue-500/10 text-blue-400 border-blue-500/30',
    Konfirmuar: 'bg-emerald-500/10 text-emerald-400 border-emerald-500/30',
    NdryshimPropozuar: 'bg-amber-500/10 text-amber-400 border-amber-500/30',
    Perfunduar: 'bg-gray-500/10 text-gray-400 border-gray-500/30',
    Anuluar: 'bg-red-500/10 text-red-400 border-red-500/30',
  }
  const cls = map[statusi] ?? map.Planifikuar
  return <span className={`text-[10px] font-bold uppercase tracking-wider px-2 py-1 rounded-md border ${cls}`}>{statusi}</span>
}

function NoteModal({ open, termin, onClose, onSaved }) {
  const [tipi, setTipi] = useState('Trajtim')
  const [permbajtja, setPermbajtja] = useState('')
  const [privat, setPrivat] = useState(false)
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    if (open) {
      setTipi('Trajtim')
      setPermbajtja('')
      setPrivat(false)
    }
  }, [open])

  const submit = async (e) => {
    e?.preventDefault?.()
    if (!permbajtja.trim()) return toast.error('Përmbajtja nuk mund të jetë bosh.')
    setSaving(true)
    try {
      await klientShenimeApi.create({
        klientId: termin.klientId,
        terminId: termin.terminId,
        tipi, permbajtja, privat,
      })
      toast.success('Shënimi u ruajt.')
      onSaved?.()
      onClose()
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Ruajtja dështoi.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal isOpen={open} onClose={onClose} title={`Shënim klinik — ${termin?.klientEmri ?? ''}`} size="lg">
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
          <textarea
            value={permbajtja}
            onChange={(e) => setPermbajtja(e.target.value)}
            rows={6}
            className="input w-full font-mono"
            placeholder="Klienti u trajtua për..."
            required
          />
        </div>
        <label className="flex items-center gap-2 text-sm text-health-primary cursor-pointer">
          <input type="checkbox" checked={privat} onChange={(e) => setPrivat(e.target.checked)} className="w-4 h-4 rounded" />
          Privat (vetëm terapist + admin e shohin; klienti nuk e sheh)
        </label>
        <div className="flex justify-end gap-2 pt-2">
          <button type="button" onClick={onClose} className="btn-secondary px-4 py-2">Anulo</button>
          <button type="submit" disabled={saving} className="btn-primary px-6 py-2 disabled:opacity-50">
            {saving ? 'Duke ruajtur...' : 'Ruaj shënimin'}
          </button>
        </div>
      </form>
    </Modal>
  )
}

function RescheduleModal({ open, termin, onClose, onSaved }) {
  const [proposedStart, setProposedStart] = useState('')
  const [proposedEnd, setProposedEnd] = useState('')
  const [note, setNote] = useState('')
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    if (open && termin) {
      const date = termin.dataTerminit?.slice(0, 10) || new Date().toISOString().slice(0, 10)
      setProposedStart(`${date}T${fmtInputTime(termin.oraFillimit)}`)
      setProposedEnd(`${date}T${fmtInputTime(termin.oraMbarimit)}`)
      setNote('')
    }
  }, [open, termin])

  const submit = async (e) => {
    e.preventDefault()
    if (!proposedStart || !proposedEnd) return toast.error('Zgjidhni orarin e propozuar.')
    setSaving(true)
    try {
      await therapistPortalApi.proposeReschedule(termin.terminId, {
        proposedStart,
        proposedEnd,
        note,
      })
      toast.success('Propozimi u dergua te klienti.')
      onSaved?.()
      onClose()
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Propozimi deshtoi.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal isOpen={open} onClose={onClose} title={`Propozo orar te ri — ${termin?.klientEmri ?? ''}`} size="lg">
      <form onSubmit={submit} className="space-y-4">
        <div className="grid sm:grid-cols-2 gap-3">
          <label>
            <span className="label">Fillimi i propozuar</span>
            <input type="datetime-local" className="input" value={proposedStart} onChange={(e) => setProposedStart(e.target.value)} />
          </label>
          <label>
            <span className="label">Mbarimi i propozuar</span>
            <input type="datetime-local" className="input" value={proposedEnd} onChange={(e) => setProposedEnd(e.target.value)} />
          </label>
        </div>
        <label>
          <span className="label">Shenim</span>
          <textarea rows={4} className="input" value={note} onChange={(e) => setNote(e.target.value)} />
        </label>
        <div className="flex justify-end gap-2">
          <button type="button" onClick={onClose} className="btn-secondary px-4 py-2">Anulo</button>
          <button disabled={saving} className="btn-primary px-5 py-2">{saving ? 'Duke derguar...' : 'Dergo propozimin'}</button>
        </div>
      </form>
    </Modal>
  )
}

function fmtInputTime(value) {
  return typeof value === 'string' ? value.substring(0, 5) : '09:00'
}

export default function TherapistPortalPage() {
  const [me, setMe] = useState(null)
  const [schedule, setSchedule] = useState([])
  const [clients, setClients] = useState([])
  const [loading, setLoading] = useState(true)
  const [completing, setCompleting] = useState(null)
  const [noteFor, setNoteFor] = useState(null)
  const [rescheduleFor, setRescheduleFor] = useState(null)
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')

  const fetchAll = useCallback(async () => {
    setLoading(true)
    try {
      const [meRes, schRes, clRes] = await Promise.all([
        therapistPortalApi.me(),
        therapistPortalApi.mySchedule(dateFrom || undefined, dateTo || undefined),
        therapistPortalApi.myClients(),
      ])
      setMe(meRes.data)
      setSchedule(schRes.data ?? [])
      setClients(clRes.data ?? [])
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Ngarkimi dështoi.')
    } finally {
      setLoading(false)
    }
  }, [dateFrom, dateTo])

  useEffect(() => { fetchAll() }, [fetchAll])

  const handleComplete = async (terminId) => {
    setCompleting(terminId)
    try {
      const res = await therapistPortalApi.completeAppointment(terminId)
      toast.success(res.data?.message || 'Termini u përfundua.')
      await fetchAll()
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Veprimi dështoi.')
    } finally {
      setCompleting(null)
    }
  }

  const fmtDate = (s) => s ? new Date(s).toLocaleDateString('sq-AL') : '—'
  const fmtTime = (s) => typeof s === 'string' ? s.substring(0, 5) : '—'

  if (loading && !me) {
    return <div className="flex items-center justify-center py-20"><Spinner size="lg" /></div>
  }

  const todayCount = schedule.filter(s => {
    const d = new Date(s.dataTerminit)
    const today = new Date()
    return d.toDateString() === today.toDateString()
  }).length
  const konfirmuarCount = schedule.filter(s => s.statusi === 'Konfirmuar').length

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-black text-health-primary tracking-tighter">
            Mirë se vini, {me?.emri} {me?.mbiemri}
          </h1>
          <p className="text-sm text-health-secondary mt-1">
            {me?.specializimi ?? 'Terapist'} · {me?.email}
          </p>
        </div>
        <button onClick={fetchAll} className="btn-secondary flex items-center gap-2 px-5 py-2.5 rounded-xl">
          <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
          Rifresko
        </button>
      </div>

      {/* Stat cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="card p-5 flex items-center gap-4">
          <div className="p-3 bg-blue-500/10 rounded-xl"><Calendar className="w-6 h-6 text-blue-400" /></div>
          <div>
            <p className="text-2xl font-bold text-health-primary">{todayCount}</p>
            <p className="text-xs font-bold text-health-secondary uppercase tracking-wider">Sot</p>
          </div>
        </div>
        <div className="card p-5 flex items-center gap-4">
          <div className="p-3 bg-emerald-500/10 rounded-xl"><Clock className="w-6 h-6 text-emerald-400" /></div>
          <div>
            <p className="text-2xl font-bold text-health-primary">{konfirmuarCount}</p>
            <p className="text-xs font-bold text-health-secondary uppercase tracking-wider">Të konfirmuara</p>
          </div>
        </div>
        <div className="card p-5 flex items-center gap-4">
          <div className="p-3 bg-purple-500/10 rounded-xl"><Users className="w-6 h-6 text-purple-400" /></div>
          <div>
            <p className="text-2xl font-bold text-health-primary">{clients.length}</p>
            <p className="text-xs font-bold text-health-secondary uppercase tracking-wider">Klientët e mi</p>
          </div>
        </div>
      </div>

      {/* Date range */}
      <div className="card p-4 flex flex-wrap items-center gap-3">
        <Calendar className="w-4 h-4 text-health-secondary" />
        <label className="flex items-center gap-2 text-xs font-bold text-health-secondary uppercase">
          Nga
          <input type="date" value={dateFrom} onChange={(e) => setDateFrom(e.target.value)} className="input" />
        </label>
        <label className="flex items-center gap-2 text-xs font-bold text-health-secondary uppercase">
          Deri
          <input type="date" value={dateTo} onChange={(e) => setDateTo(e.target.value)} className="input" />
        </label>
        {(dateFrom || dateTo) && (
          <button onClick={() => { setDateFrom(''); setDateTo('') }} className="btn-secondary px-3 py-1.5 text-xs">
            Pastro
          </button>
        )}
        <p className="ml-auto text-xs text-health-secondary">Default: 14 ditët e ardhshme</p>
      </div>

      {/* Schedule table */}
      <div className="card overflow-hidden">
        <div className="px-6 py-4 border-b border-health-border flex items-center justify-between">
          <h2 className="text-lg font-bold text-health-primary">Orari im</h2>
          <span className="text-xs font-bold text-health-secondary uppercase tracking-wider">{schedule.length} termine</span>
        </div>

        {schedule.length === 0 ? (
          <div className="py-12 text-center text-sm text-health-secondary">
            Nuk keni termine të planifikuara në këtë interval.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-health-bg border-b border-health-border">
                <tr>
                  <th className="table-th">Data</th>
                  <th className="table-th">Orari</th>
                  <th className="table-th">Klienti</th>
                  <th className="table-th">Shërbimi</th>
                  <th className="table-th">Statusi</th>
                  <th className="table-th text-right">Veprime</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-health-border/50">
                {schedule.map((t) => (
                  <tr key={t.terminId} className="hover:bg-health-hover/50 transition-colors">
                    <td className="table-td">{fmtDate(t.dataTerminit)}</td>
                    <td className="table-td font-mono">{fmtTime(t.oraFillimit)} – {fmtTime(t.oraMbarimit)}</td>
                    <td className="table-td font-semibold text-health-primary">{t.klientEmri}</td>
                    <td className="table-td">{t.sherbimi}</td>
                    <td className="table-td"><StatusBadge statusi={t.statusi} /></td>
                    <td className="table-td text-right">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => setNoteFor(t)}
                          className="flex items-center gap-1 px-2 py-1 text-xs font-bold text-blue-400 hover:bg-blue-500/10 rounded-md transition-colors"
                          title="Shto shënim klinik"
                        >
                          <FileText className="w-3 h-3" /> Shënim
                        </button>
                        {t.statusi === 'Konfirmuar' && (
                          <button
                            disabled={completing === t.terminId}
                            onClick={() => handleComplete(t.terminId)}
                            className="flex items-center gap-1 px-3 py-1 text-xs font-bold text-emerald-400 hover:bg-emerald-500/10 rounded-md transition-colors disabled:opacity-40"
                          >
                            <CheckCircle2 className="w-3 h-3" />
                            {completing === t.terminId ? '...' : 'Përfundo'}
                          </button>
                        )}
                        {(t.statusi === 'Planifikuar' || t.statusi === 'Konfirmuar') && (
                          <button
                            onClick={() => setRescheduleFor(t)}
                            className="flex items-center gap-1 px-2 py-1 text-xs font-bold text-amber-400 hover:bg-amber-500/10 rounded-md transition-colors"
                            title="Propozo orar të ri"
                          >
                            <RefreshCw className="w-3 h-3" /> Ndrysho
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* My clients */}
      <div className="card overflow-hidden">
        <div className="px-6 py-4 border-b border-health-border">
          <h2 className="text-lg font-bold text-health-primary">Klientët e mi</h2>
        </div>
        {clients.length === 0 ? (
          <div className="py-12 text-center text-sm text-health-secondary">Nuk keni klientë të regjistruar ende.</div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3 p-4">
            {clients.map((c) => (
              <div key={c.klientId} className="card p-4 hover:border-health-accent/30 transition-all">
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-purple-500/10 rounded-xl"><User className="w-5 h-5 text-purple-400" /></div>
                  <div className="min-w-0 flex-1">
                    <p className="font-bold text-health-primary truncate">{c.emri} {c.mbiemri}</p>
                    <p className="text-xs text-health-secondary truncate">{c.email}</p>
                    {c.telefoni && <p className="text-xs text-health-secondary mt-0.5">📞 {c.telefoni}</p>}
                  </div>
                  <ChevronRight className="w-4 h-4 text-health-secondary/30" />
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Add-note modal */}
      <NoteModal
        open={Boolean(noteFor)}
        termin={noteFor}
        onClose={() => setNoteFor(null)}
        onSaved={fetchAll}
      />
      <RescheduleModal
        open={Boolean(rescheduleFor)}
        termin={rescheduleFor}
        onClose={() => setRescheduleFor(null)}
        onSaved={fetchAll}
      />
    </div>
  )
}
