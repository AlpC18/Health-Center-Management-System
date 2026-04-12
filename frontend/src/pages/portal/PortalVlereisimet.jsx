import { useEffect, useState } from 'react'
import { Star, Plus } from 'lucide-react'
import { portalApi } from '../../api/portalApi'
import { PageLoader, Modal, Spinner } from '../../components/ui'
import toast from 'react-hot-toast'

export default function PortalVlereisimet() {
  const [vlereisimet, setVlereisimet] = useState([])
  const [sherbimet, setSherbimet] = useState([])
  const [terapistet, setTerapistet] = useState([])
  const [loading, setLoading] = useState(true)
  const [modalOpen, setModalOpen] = useState(false)
  const [form, setForm] = useState({ sherbimId: '', terapistId: '', nota: 5, komenti: '' })
  const [saving, setSaving] = useState(false)

  const fetchAll = () => {
    Promise.all([portalApi.getVlereisimet(), portalApi.getSherbimet(), portalApi.getTerapistet()])
      .then(([v, s, t]) => {
        setVlereisimet(v.data?.data || v.data || [])
        setSherbimet(s.data?.data || s.data || [])
        setTerapistet(t.data?.data || t.data || [])
      })
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    fetchAll()
  }, [])

  const handleSave = async (e) => {
    e.preventDefault()
    setSaving(true)
    try {
      await portalApi.addVleresim({
        sherbimId: +form.sherbimId,
        terapistId: +form.terapistId,
        nota: +form.nota,
        komenti: form.komenti,
        klientId: 0,
      })
      toast.success('Vlerësimi u shtua!')
      setModalOpen(false)
      setForm({ sherbimId: '', terapistId: '', nota: 5, komenti: '' })
      fetchAll()
    } catch (err) {
      toast.error(err.response?.data?.message || 'Gabim gjatë ruajtjes.')
    } finally {
      setSaving(false)
    }
  }

  if (loading) return <PageLoader />

  const fmtDate = (d) => new Date(d).toLocaleDateString('sq-AL')

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Vlerësimet e Mia</h1>
        <button onClick={() => setModalOpen(true)} className="btn-primary">
          <Plus className="w-4 h-4" />
          Shto Vlerësim
        </button>
      </div>

      {vlereisimet.length === 0 ? (
        <div className="card p-8 text-center">
          <Star className="w-10 h-10 text-gray-300 mx-auto mb-3" />
          <p className="text-sm text-gray-500">Nuk keni vlerësime ende.</p>
        </div>
      ) : (
        <div className="space-y-3">
          {vlereisimet.map((v) => (
            <div key={v.vleresimId} className="card p-4">
              <div className="flex items-start justify-between mb-2">
                <div>
                  <p className="font-semibold text-gray-900">{v.sherbimEmri}</p>
                  <p className="text-sm text-gray-500">Terapist: {v.terapistEmri}</p>
                </div>
                <div className="flex items-center gap-0.5">
                  {[1, 2, 3, 4, 5].map((n) => (
                    <Star key={n} className={`w-4 h-4 ${n <= v.nota ? 'text-yellow-400 fill-yellow-400' : 'text-gray-300'}`} />
                  ))}
                </div>
              </div>
              {v.komenti && <p className="text-sm text-gray-600 italic">"{v.komenti}"</p>}
              <p className="text-xs text-gray-400 mt-2">{fmtDate(v.dataVleresimit)}</p>
            </div>
          ))}
        </div>
      )}

      <Modal isOpen={modalOpen} onClose={() => setModalOpen(false)} title="Shto Vlerësim">
        <form onSubmit={handleSave} className="space-y-4">
          <div>
            <label className="label">Shërbimi *</label>
            <select className="input" value={form.sherbimId} required onChange={(e) => setForm((p) => ({ ...p, sherbimId: e.target.value }))}>
              <option value="">— Zgjidh —</option>
              {sherbimet.map((s) => (
                <option key={s.sherbimId} value={s.sherbimId}>{s.emriSherbimit}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="label">Terapisti *</label>
            <select className="input" value={form.terapistId} required onChange={(e) => setForm((p) => ({ ...p, terapistId: e.target.value }))}>
              <option value="">— Zgjidh —</option>
              {terapistet.map((t) => (
                <option key={t.terapistId} value={t.terapistId}>{t.emri} {t.mbiemri}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="label">Nota *</label>
            <div className="flex gap-2">
              {[1, 2, 3, 4, 5].map((n) => (
                <button
                  type="button"
                  key={n}
                  onClick={() => setForm((p) => ({ ...p, nota: n }))}
                  className={`w-10 h-10 rounded-lg font-bold text-sm transition-all ${form.nota === n ? 'bg-yellow-400 text-yellow-900 scale-110' : 'bg-gray-100 text-gray-500 hover:bg-yellow-100'}`}
                >
                  {n}
                </button>
              ))}
            </div>
          </div>
          <div>
            <label className="label">Komenti</label>
            <textarea
              className="input resize-none"
              rows={3}
              value={form.komenti}
              onChange={(e) => setForm((p) => ({ ...p, komenti: e.target.value }))}
              placeholder="Ndani përvojën tuaj..."
            />
          </div>
          <div className="flex justify-end gap-3 pt-2 border-t border-gray-100">
            <button type="button" className="btn-secondary" onClick={() => setModalOpen(false)}>Anulo</button>
            <button type="submit" className="btn-primary" disabled={saving}>
              {saving && <Spinner size="sm" />}
              Ruaj Vlerësimin
            </button>
          </div>
        </form>
      </Modal>
    </div>
  )
}
