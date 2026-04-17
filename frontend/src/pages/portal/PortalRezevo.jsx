import { useEffect, useState } from 'react'
import { Clock, User, Stethoscope, CheckCircle } from 'lucide-react'
import { portalApi } from '../../api/portalApi'
import { Spinner, Alert } from '../../components/ui'
import toast from 'react-hot-toast'
import { useNavigate } from 'react-router-dom'

export default function PortalRezevo() {
  const navigate = useNavigate()
  const [sherbimet, setSherbimet] = useState([])
  const [terapistet, setTerapistet] = useState([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [step, setStep] = useState(1)
  const [form, setForm] = useState({
    sherbimId: '',
    terapistId: '',
    dataTerminit: '',
    oraFillimit: '09:00',
    oraMbarimit: '10:00',
    shenimet: '',
  })

  useEffect(() => {
    Promise.all([portalApi.getSherbimet(), portalApi.getTerapistet()])
      .then(([s, t]) => {
        setSherbimet(s.data?.data || s.data || [])
        setTerapistet(t.data?.data || t.data || [])
      })
      .finally(() => setLoading(false))
  }, [])

  const selectedSherbim = sherbimet.find((s) => s.sherbimId === +form.sherbimId)
  const selectedTerapist = terapistet.find((t) => t.terapistId === +form.terapistId)

  const handleSherbimSelect = (s) => {
    const [h, m] = form.oraFillimit.split(':').map(Number)
    const totalMin = h * 60 + m + s.kohezgjatjaMin
    setForm((prev) => ({
      ...prev,
      sherbimId: String(s.sherbimId),
      oraMbarimit: `${String(Math.floor(totalMin / 60)).padStart(2, '0')}:${String(totalMin % 60).padStart(2, '0')}`,
    }))
    setStep(2)
  }

  const handleSubmit = async () => {
    setError('')
    setSaving(true)
    try {
      const [fh, fm] = form.oraFillimit.split(':').map(Number)
      const [mh, mm] = form.oraMbarimit.split(':').map(Number)
      await portalApi.createTermin({
        sherbimId: +form.sherbimId,
        terapistId: +form.terapistId,
        dataTerminit: form.dataTerminit,
        oraFillimit: `${String(fh).padStart(2, '0')}:${String(fm).padStart(2, '0')}:00`,
        oraMbarimit: `${String(mh).padStart(2, '0')}:${String(mm).padStart(2, '0')}:00`,
        shenimet: form.shenimet || null,
        statusi: 'Planifikuar',
      })
      toast.success('Termini u rezervua me sukses!')
      navigate('/portal/terminet')
    } catch (err) {
      const data = err.response?.data
      if (Array.isArray(data?.errors) && data.errors.length > 0) {
        setError(data.errors[0])
      } else {
        setError(data?.message || 'Rezervimi dështoi.')
      }
    } finally {
      setSaving(false)
    }
  }

  const today = new Date().toISOString().split('T')[0]

  if (loading) return <div className="flex justify-center py-16"><Spinner size="lg" /></div>

  return (
    <div className="max-w-2xl">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Rezervo Termin</h1>
        <p className="text-sm text-gray-500 mt-1">Zgjidhni shërbimin, terapistin dhe kohën e preferuar</p>
      </div>

      <div className="flex items-center gap-1 mb-6">
        {[1, 2, 3, 4].map((s, i) => (
          <div key={s} className="flex items-center gap-1 flex-1">
            <div className={`w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold transition-all ${step >= s ? 'bg-green-600 text-white' : 'bg-gray-200 text-gray-500'}`}>
              {step > s ? <CheckCircle className="w-4 h-4" /> : s}
            </div>
            {i < 3 && <div className={`flex-1 h-0.5 ${step > s ? 'bg-green-600' : 'bg-gray-200'}`} />}
          </div>
        ))}
      </div>

      {error && <Alert type="error" message={error} />}

      {step === 1 && (
        <div className="card p-5">
          <h2 className="font-semibold text-gray-900 mb-4 flex items-center gap-2">
            <Stethoscope className="w-4 h-4 text-green-600" />
            Zgjidhni Shërbimin
          </h2>
          <div className="grid gap-3">
            {sherbimet.map((s) => (
              <button
                key={s.sherbimId}
                onClick={() => handleSherbimSelect(s)}
                className="p-4 rounded-xl border-2 border-gray-200 text-left hover:border-green-300 hover:bg-green-50/30 transition-all"
              >
                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-semibold text-gray-900">{s.emriSherbimit}</p>
                    <p className="text-xs text-gray-500">{s.kategoria} • {s.kohezgjatjaMin} min</p>
                  </div>
                  <p className="font-bold text-green-700">€{s.cmimi}</p>
                </div>
              </button>
            ))}
          </div>
        </div>
      )}

      {step === 2 && (
        <div className="card p-5">
          <h2 className="font-semibold text-gray-900 mb-4 flex items-center gap-2">
            <User className="w-4 h-4 text-green-600" />
            Zgjidhni Terapistin
          </h2>
          <div className="grid gap-3">
            {terapistet.map((t) => (
              <button
                key={t.terapistId}
                onClick={() => {
                  setForm((p) => ({ ...p, terapistId: String(t.terapistId) }))
                  setStep(3)
                }}
                className="p-4 rounded-xl border-2 border-gray-200 text-left hover:border-green-300 hover:bg-green-50/30 transition-all"
              >
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-full bg-green-100 flex items-center justify-center">
                    <span className="text-sm font-bold text-green-700">{t.emri[0]}{t.mbiemri[0]}</span>
                  </div>
                  <div>
                    <p className="font-semibold text-gray-900">{t.emri} {t.mbiemri}</p>
                    <p className="text-xs text-gray-500">{t.specializimi}</p>
                  </div>
                </div>
              </button>
            ))}
          </div>
          <button onClick={() => setStep(1)} className="btn-secondary mt-4">← Kthehu</button>
        </div>
      )}

      {step === 3 && (
        <div className="card p-5">
          <h2 className="font-semibold text-gray-900 mb-4 flex items-center gap-2">
            <Clock className="w-4 h-4 text-green-600" />
            Zgjidhni Datën dhe Orën
          </h2>
          <div className="space-y-4">
            <div>
              <label className="label">Data e Terminit *</label>
              <input
                type="date"
                className="input"
                min={today}
                value={form.dataTerminit}
                onChange={(e) => setForm((p) => ({ ...p, dataTerminit: e.target.value }))}
              />
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="label">Ora e Fillimit *</label>
                <input
                  type="time"
                  className="input"
                  value={form.oraFillimit}
                  onChange={(e) => setForm((p) => ({ ...p, oraFillimit: e.target.value }))}
                />
              </div>
              <div>
                <label className="label">Ora e Mbarimit *</label>
                <input
                  type="time"
                  className="input"
                  value={form.oraMbarimit}
                  onChange={(e) => setForm((p) => ({ ...p, oraMbarimit: e.target.value }))}
                />
              </div>
            </div>
            <div>
              <label className="label">Shënime (opsionale)</label>
              <textarea
                className="input resize-none"
                rows={3}
                placeholder="Ndonjë kërkesë e veçantë..."
                value={form.shenimet}
                onChange={(e) => setForm((p) => ({ ...p, shenimet: e.target.value }))}
              />
            </div>
          </div>
          <div className="flex gap-3 mt-4">
            <button onClick={() => setStep(2)} className="btn-secondary">← Kthehu</button>
            <button
              onClick={() => form.dataTerminit && setStep(4)}
              disabled={!form.dataTerminit}
              className="btn-primary flex-1 justify-center"
            >
              Vazhdo →
            </button>
          </div>
        </div>
      )}

      {step === 4 && (
        <div className="card p-5">
          <h2 className="font-semibold text-gray-900 mb-4">Konfirmo Rezervimin</h2>
          <div className="bg-green-50 rounded-xl p-4 space-y-3 mb-6">
            {[
              ['Shërbimi', selectedSherbim?.emriSherbimit],
              ['Terapisti', `${selectedTerapist?.emri} ${selectedTerapist?.mbiemri}`],
              ['Data', new Date(form.dataTerminit).toLocaleDateString('sq-AL')],
              ['Ora', `${form.oraFillimit} - ${form.oraMbarimit}`],
            ].map(([label, value]) => (
              <div key={label} className="flex justify-between">
                <span className="text-sm text-gray-600">{label}:</span>
                <span className="text-sm font-semibold">{value}</span>
              </div>
            ))}
            <div className="flex justify-between border-t border-green-200 pt-3">
              <span className="text-sm font-semibold text-gray-700">Çmimi:</span>
              <span className="text-sm font-bold text-green-700">€{selectedSherbim?.cmimi}</span>
            </div>
          </div>
          <div className="flex gap-3">
            <button onClick={() => setStep(3)} className="btn-secondary">← Kthehu</button>
            <button onClick={handleSubmit} disabled={saving} className="btn-primary flex-1 justify-center">
              {saving && <Spinner size="sm" />}
              Konfirmo Rezervimin
            </button>
          </div>
        </div>
      )}
    </div>
  )
}

