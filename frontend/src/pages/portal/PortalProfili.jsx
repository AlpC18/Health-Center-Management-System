import { useState, useEffect } from 'react'
import { User, Lock, Save, Eye, EyeOff } from 'lucide-react'
import { portalApi } from '../../api/portalApi'
import api from '../../api/api'
import { Spinner, Alert } from '../../components/ui'
import toast from 'react-hot-toast'

export default function PortalProfili() {
  const [profili, setProfili] = useState(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [pwForm, setPwForm] = useState({ currentPassword: '', newPassword: '', confirmPassword: '' })
  const [pwLoading, setPwLoading] = useState(false)
  const [pwError, setPwError] = useState('')
  const [showPw, setShowPw] = useState({})

  useEffect(() => {
    portalApi.getProfili()
      .then((r) => setProfili(r.data))
      .finally(() => setLoading(false))
  }, [])

  const handleSaveProfil = async (e) => {
    e.preventDefault()
    setSaving(true)
    try {
      const { data } = await portalApi.updateProfili(profili)
      setProfili(data)
      toast.success('Profili u përditësua!')
    } catch {
      toast.error('Gabim gjatë ruajtjes.')
    } finally {
      setSaving(false)
    }
  }

  const handleChangePw = async (e) => {
    e.preventDefault()
    setPwError('')
    if (pwForm.newPassword !== pwForm.confirmPassword) {
      setPwError('Fjalëkalimet nuk përputhen.')
      return
    }
    setPwLoading(true)
    try {
      await api.put('/auth/change-password', {
        currentPassword: pwForm.currentPassword,
        newPassword: pwForm.newPassword,
      })
      toast.success('Fjalëkalimi u ndryshua!')
      setPwForm({ currentPassword: '', newPassword: '', confirmPassword: '' })
    } catch (err) {
      const errs = err.response?.data?.errors
      setPwError(errs ? errs.join(' ') : 'Fjalëkalimi aktual është gabim.')
    } finally {
      setPwLoading(false)
    }
  }

  if (loading) return <div className="flex justify-center py-16"><Spinner size="lg" /></div>

  return (
    <div className="max-w-2xl space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Profili Im</h1>
      <div className="card p-6">
        <h2 className="font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <User className="w-4 h-4 text-green-600" />
          Të Dhënat Personale
        </h2>
        <form onSubmit={handleSaveProfil} className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="label">Emri</label>
              <input className="input" value={profili?.emri || ''} onChange={(e) => setProfili((p) => ({ ...p, emri: e.target.value }))} />
            </div>
            <div>
              <label className="label">Mbiemri</label>
              <input className="input" value={profili?.mbiemri || ''} onChange={(e) => setProfili((p) => ({ ...p, mbiemri: e.target.value }))} />
            </div>
          </div>
          <div>
            <label className="label">Email</label>
            <input className="input bg-gray-50" value={profili?.email || ''} disabled />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="label">Telefoni</label>
              <input className="input" value={profili?.telefoni || ''} onChange={(e) => setProfili((p) => ({ ...p, telefoni: e.target.value }))} />
            </div>
            <div>
              <label className="label">Data Lindjes</label>
              <input
                type="date"
                className="input"
                value={profili?.dataLindjes ? new Date(profili.dataLindjes).toISOString().split('T')[0] : ''}
                onChange={(e) => setProfili((p) => ({ ...p, dataLindjes: e.target.value }))}
              />
            </div>
          </div>
          <div>
            <label className="label">Gjinia</label>
            <select className="input" value={profili?.gjinia || ''} onChange={(e) => setProfili((p) => ({ ...p, gjinia: e.target.value }))}>
              <option value="">— Zgjidh —</option>
              <option value="M">Mashkull</option>
              <option value="F">Femër</option>
            </select>
          </div>
          <div>
            <label className="label">Kushtet Shëndetësore</label>
            <textarea className="input resize-none" rows={3} value={profili?.kushtetShendetesore || ''} onChange={(e) => setProfili((p) => ({ ...p, kushtetShendetesore: e.target.value }))} />
          </div>
          <button type="submit" className="btn-primary" disabled={saving}>
            {saving && <Spinner size="sm" />}
            <Save className="w-4 h-4" />
            Ruaj Ndryshimet
          </button>
        </form>
      </div>

      <div className="card p-6">
        <h2 className="font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <Lock className="w-4 h-4 text-green-600" />
          Ndrysho Fjalëkalimin
        </h2>
        {pwError && <Alert type="error" message={pwError} />}
        <form onSubmit={handleChangePw} className="space-y-4 mt-4">
          {[
            { field: 'currentPassword', label: 'Fjalëkalimi Aktual' },
            { field: 'newPassword', label: 'Fjalëkalimi i Ri' },
            { field: 'confirmPassword', label: 'Konfirmo Fjalëkalimin' },
          ].map(({ field, label }) => (
            <div key={field}>
              <label className="label">{label}</label>
              <div className="relative">
                <input
                  className="input pr-10"
                  type={showPw[field] ? 'text' : 'password'}
                  value={pwForm[field]}
                  onChange={(e) => setPwForm((p) => ({ ...p, [field]: e.target.value }))}
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowPw((p) => ({ ...p, [field]: !p[field] }))}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400"
                >
                  {showPw[field] ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                </button>
              </div>
            </div>
          ))}
          <button type="submit" className="btn-primary" disabled={pwLoading}>
            {pwLoading && <Spinner size="sm" />}
            Ndrysho Fjalëkalimin
          </button>
        </form>
      </div>
    </div>
  )
}
