import { useState } from 'react'
import { Lock, Eye, EyeOff, Check } from 'lucide-react'
import useAuthStore from '../store/authStore'
import api from '../api/api'
import { Alert, Spinner } from '../components/ui'
import toast from 'react-hot-toast'

export default function ProfilePage() {
  const { user } = useAuthStore()
  const roles = Array.isArray(user?.roles) ? user.roles : []
  const isAdmin = user?.role === 'Admin' || roles.includes('Admin') || roles.includes('Staff')
  const [pwForm, setPwForm] = useState({
    current: '',
    new: '',
    confirm: '',
  })
  const [showPw, setShowPw] = useState({ current: false, new: false, confirm: false })
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const handlePasswordChange = async (e) => {
    e.preventDefault()
    setError('')
    setSuccess('')
    if (pwForm.new !== pwForm.confirm) {
      setError('Fjalëkalimet e reja nuk përputhen.')
      return
    }
    if (pwForm.new.length < 8) {
      setError('Fjalëkalimi i ri duhet të ketë minimumi 8 karaktere.')
      return
    }
    setLoading(true)
    try {
      await api.put('/auth/change-password', {
        currentPassword: pwForm.current,
        newPassword: pwForm.new,
      })
      setSuccess('Fjalëkalimi u ndryshua me sukses!')
      setPwForm({ current: '', new: '', confirm: '' })
      toast.success('Fjalëkalimi u ndryshua!')
    } catch (err) {
      const errs = err.response?.data?.errors
      setError(errs ? errs.join(' ') : 'Fjalëkalimi aktual nuk është i saktë.')
    } finally {
      setLoading(false)
    }
  }

  const PasswordInput = ({ field, label, placeholder }) => (
    <div>
      <label className="label">{label}</label>
      <div className="relative">
        <input
          className="input pr-10"
          type={showPw[field] ? 'text' : 'password'}
          placeholder={placeholder}
          value={pwForm[field]}
          onChange={(e) => setPwForm({ ...pwForm, [field]: e.target.value })}
          required
        />
        <button
          type="button"
          onClick={() => setShowPw({ ...showPw, [field]: !showPw[field] })}
          className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400"
        >
          {showPw[field] ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
        </button>
      </div>
    </div>
  )

  return (
    <div className="max-w-2xl space-y-8">
      <h1 className="text-3xl font-bold text-health-primary tracking-tight">Profili Im</h1>

      {/* User info */}
      <div className="card p-8 shadow-2xl">
        <div className="flex items-center gap-6 mb-8">
          <div className="w-20 h-20 rounded-2xl bg-health-accent/10 border border-health-accent/20 flex items-center justify-center shadow-lg shadow-health-accent/5">
            <span className="text-3xl font-extrabold text-health-accent tracking-tighter">
              {user?.firstName?.[0]}{user?.lastName?.[0]}
            </span>
          </div>
          <div>
            <h2 className="text-xl font-bold text-health-primary tracking-tight">
              {user?.firstName} {user?.lastName}
            </h2>
            <p className="text-sm font-medium text-health-secondary mt-1 tracking-wide">{user?.email}</p>
          </div>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
          <div>
            <label className="label">Emri</label>
            <input className="input bg-health-bg/50 cursor-not-allowed border-health-border/50 text-health-secondary" value={user?.firstName || ''} disabled />
          </div>
          <div>
            <label className="label">Mbiemri</label>
            <input className="input bg-health-bg/50 cursor-not-allowed border-health-border/50 text-health-secondary" value={user?.lastName || ''} disabled />
          </div>
          <div className="sm:col-span-2">
            <label className="label">Email</label>
            <input className="input bg-health-bg/50 cursor-not-allowed border-health-border/50 text-health-secondary" value={user?.email || ''} disabled />
          </div>
        </div>
      </div>

      {/* Change password */}
      <div className="card p-8">
        <h3 className="text-lg font-bold text-health-primary mb-6 flex items-center gap-2 tracking-tight">
          <Lock className="w-5 h-5 text-health-brand" /> Ndrysho Fjalëkalimin
        </h3>
        <Alert type="error" message={error} />
        <Alert type="success" message={success} />
        <form onSubmit={handlePasswordChange} className="space-y-4 mt-4">
          <PasswordInput field="current" label="Fjalëkalimi Aktual" placeholder="••••••••" />
          <PasswordInput field="new" label="Fjalëkalimi i Ri" placeholder="Minimumi 8 karaktere" />
          <PasswordInput field="confirm" label="Konfirmo Fjalëkalimin e Ri" placeholder="••••••••" />
          <button type="submit" className="btn-primary w-full sm:w-auto flex items-center justify-center gap-3 px-8 py-3 shadow-lg shadow-health-brand/20" disabled={loading}>
            {loading ? <Spinner size="sm" /> : <Check className="w-5 h-5" />}
            Ndrysho Fjalëkalimin
          </button>
        </form>
      </div>

      {/* Database Backup (Admin Only) */}
      {isAdmin && (
        <div className="card p-8 border-health-brand/30 bg-health-brand/5">
          <h3 className="text-lg font-bold text-health-brand mb-2 flex items-center gap-2 tracking-tight uppercase tracking-widest text-[12px]">
            Veri Yedekleme (Backup)
          </h3>
          <p className="text-sm text-health-secondary mb-6 leading-relaxed">Sistemdeki tüm verileri SQLite formatında yedeğini indirir.</p>
          <button 
            onClick={() => window.open('http://localhost:5077/api/backup/database', '_blank')}
            className="btn-secondary flex items-center gap-3 px-8 py-3 border-health-brand/40 text-health-brand hover:bg-health-brand hover:text-white transition-all shadow-lg shadow-health-brand/10 group"
          >
            <Check className="w-5 h-5 opacity-0 group-hover:opacity-100 transition-opacity" />
            Veritabanını İndir (.db)
          </button>
        </div>
      )}
    </div>
  )
}
