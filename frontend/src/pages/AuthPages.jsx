import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import toast from 'react-hot-toast'
import { Leaf, Eye, EyeOff } from 'lucide-react'
import { authApi } from '../api/api'
import useAuthStore from '../store/authStore'
import { Spinner } from '../components/ui/index'
import useLangStore from '../store/langStore'
import { t } from '../i18n'

function AuthLayout({ children, title, subtitle }) {
  return (
    <div className="min-h-screen bg-health-bg flex items-center justify-center p-6">
      <div className="w-full max-w-md">
        {/* Logo */}
        <div className="flex flex-col items-center mb-10">
          <div className="p-4 bg-health-brand rounded-2xl mb-4 shadow-lg shadow-health-brand/20">
            <Leaf className="h-8 w-8 text-white" />
          </div>
          <h1 className="text-2xl font-bold text-health-primary tracking-tight">Wellness House</h1>
          <p className="text-sm text-health-secondary mt-1 font-medium tracking-wide uppercase">Sistemi i Menaxhimit</p>
        </div>

        {/* Card */}
        <div className="card p-8 shadow-2xl">
          <h2 className="text-xl font-bold text-health-primary mb-2">{title}</h2>
          {subtitle && <p className="text-sm text-health-secondary mb-8 leading-relaxed">{subtitle}</p>}
          {children}
        </div>
      </div>
    </div>
  )
}

export function LoginPage() {
  const navigate = useNavigate()
  const { setAuth } = useAuthStore()
  const { lang } = useLangStore()
  const [form, setForm] = useState({ email: '', password: '' })
  const [loginType, setLoginType] = useState('Klient')
  const [showPassword, setShowPassword] = useState(false)
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (!form.email || !form.password) {
      toast.error(t(lang, 'fillFields'))
      return
    }
    setLoading(true)
    try {
      const res = await authApi.login(form)
      const role = res?.data?.user?.role

      if (loginType === 'Doktor' && role !== 'Therapist') {
        toast.error('Kjo llogari nuk eshte Doktor.')
        return
      }
      if (loginType === 'Klient' && role !== 'Klient') {
        toast.error('Kjo llogari nuk eshte Klient.')
        return
      }

      setAuth(res.data)
      navigate(role === 'Klient' ? '/portal/dashboard' : '/dashboard')
    } catch {
      toast.error(t(lang, 'loginError'))
    } finally {
      setLoading(false)
    }
  }

  return (
    <AuthLayout title={t(lang, 'welcome')} subtitle={t(lang, 'loginSubtitle')}>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="grid grid-cols-2 gap-2 p-1 bg-health-bg rounded-xl border border-health-border">
          <button
            type="button"
            onClick={() => setLoginType('Klient')}
            className={`px-3 py-2 rounded-lg text-sm font-semibold transition-all ${loginType === 'Klient' ? 'bg-health-brand text-white' : 'text-health-secondary hover:bg-health-hover'}`}
          >
            Kycu si Klient
          </button>
          <button
            type="button"
            onClick={() => setLoginType('Doktor')}
            className={`px-3 py-2 rounded-lg text-sm font-semibold transition-all ${loginType === 'Doktor' ? 'bg-health-brand text-white' : 'text-health-secondary hover:bg-health-hover'}`}
          >
            Kycu si Doktor
          </button>
        </div>

        <div>
          <label className="label">{t(lang, 'email')}</label>
          <input
            type="email"
            className="input"
            placeholder={loginType === 'Doktor' ? 'doktor@example.com' : 'ju@example.com'}
            value={form.email}
            onChange={(e) => setForm((p) => ({ ...p, email: e.target.value }))}
            autoComplete="email"
          />
        </div>

        <div>
          <label className="label">{t(lang, 'password')}</label>
          <div className="relative">
            <input
              type={showPassword ? 'text' : 'password'}
              className="input pr-10"
              placeholder="••••••••"
              value={form.password}
              onChange={(e) => setForm((p) => ({ ...p, password: e.target.value }))}
              autoComplete="current-password"
            />
            <button
              type="button"
              onClick={() => setShowPassword((v) => !v)}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
            >
              {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
            </button>
          </div>
        </div>

        <button type="submit" className="btn-primary w-full justify-center mt-2" disabled={loading}>
          {loading ? <Spinner size="sm" /> : null}
          {t(lang, 'login')}
        </button>
      </form>

      <p className="text-sm text-center text-health-secondary mt-8">
        {t(lang, 'noAccount')}{' '}
        <Link to="/register" className="text-health-accent hover:text-health-accent/80 hover:underline font-bold transition-all">
          {t(lang, 'register')}
        </Link>
      </p>
    </AuthLayout>
  )
}

export function RegisterPage() {
  const navigate = useNavigate()
  const { setAuth } = useAuthStore()
  const { lang } = useLangStore()
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    role: 'Klient',
    specializimi: '',
    licenca: '',
    telefoni: '',
  })
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (!form.firstName || !form.lastName || !form.email || !form.password) {
      toast.error(t(lang, 'fillFields'))
      return
    }
    if (form.password.length < 8) {
      toast.error('Fjalekalimi duhet te kete te pakten 8 karaktere.')
      return
    }
    if (!/\d/.test(form.password)) {
      toast.error('Fjalekalimi duhet te permbaje te pakten nje numer.')
      return
    }
    if (form.role === 'Therapist' && !form.specializimi.trim()) {
      toast.error('Specializimi eshte i detyrueshem per doktor.')
      return
    }
    setLoading(true)
    try {
      const res = await authApi.register(form)
      setAuth(res.data)
      const role = res?.data?.user?.role
      navigate(role === 'Klient' ? '/portal/dashboard' : '/dashboard')
    } catch (err) {
      if (err.response?.data?.message === 'EXISTING_ACCOUNT') {
        toast.error(err.response.data.text || 'Kjo llogari ekziston.')
        navigate('/login')
      } else if (Array.isArray(err.response?.data?.errors) && err.response.data.errors.length > 0) {
        toast.error(err.response.data.errors[0])
      } else if (typeof err.response?.data?.message === 'string') {
        toast.error(err.response.data.message)
      } else if (!err.response) {
        toast.error('Backend API nuk po pergjigjet. Ndeze backend-in ne portin 5077.')
      } else {
        toast.error(t(lang, 'registerError'))
      }
    } finally {
      setLoading(false)
    }
  }

  return (
    <AuthLayout title={t(lang, 'createAccount')} subtitle={t(lang, 'registerSubtitle')}>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="label">{t(lang, 'firstName')}</label>
            <input
              type="text"
              className="input"
              placeholder={t(lang, 'firstName')}
              value={form.firstName}
              onChange={(e) => setForm((p) => ({ ...p, firstName: e.target.value }))}
            />
          </div>
          <div>
            <label className="label">{t(lang, 'lastName')}</label>
            <input
              type="text"
              className="input"
              placeholder={t(lang, 'lastName')}
              value={form.lastName}
              onChange={(e) => setForm((p) => ({ ...p, lastName: e.target.value }))}
            />
          </div>
        </div>

        <div>
          <label className="label">{t(lang, 'email')}</label>
          <input
            type="email"
            className="input"
            placeholder="ju@example.com"
            value={form.email}
            onChange={(e) => setForm((p) => ({ ...p, email: e.target.value }))}
            autoComplete="email"
          />
        </div>

        <div>
          <label className="label">{t(lang, 'password')}</label>
          <input
            type="password"
            className="input"
            placeholder={t(lang, 'minChars')}
            value={form.password}
            onChange={(e) => setForm((p) => ({ ...p, password: e.target.value }))}
            autoComplete="new-password"
          />
        </div>

        <div>
          <label className="label">Roli</label>
          <select
            className="input"
            value={form.role}
            onChange={(e) => setForm((p) => ({ ...p, role: e.target.value }))}
          >
            <option value="Klient">Klient</option>
            <option value="Therapist">Doktor</option>
          </select>
        </div>

        {form.role === 'Therapist' && (
          <div className="space-y-4 p-4 rounded-xl border border-health-border bg-health-bg/60">
            <p className="text-xs font-semibold text-health-secondary uppercase tracking-wider">
              Te Dhenat e Doktorit
            </p>
            <div>
              <label className="label">Specializimi *</label>
              <input
                type="text"
                className="input"
                placeholder="p.sh. Fizioterapi"
                value={form.specializimi}
                onChange={(e) => setForm((p) => ({ ...p, specializimi: e.target.value }))}
              />
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="label">Licenca</label>
                <input
                  type="text"
                  className="input"
                  placeholder="p.sh. LIC-2026-001"
                  value={form.licenca}
                  onChange={(e) => setForm((p) => ({ ...p, licenca: e.target.value }))}
                />
              </div>
              <div>
                <label className="label">Telefoni</label>
                <input
                  type="text"
                  className="input"
                  placeholder="+383 44 000 000"
                  value={form.telefoni}
                  onChange={(e) => setForm((p) => ({ ...p, telefoni: e.target.value }))}
                />
              </div>
            </div>
          </div>
        )}

        <button type="submit" className="btn-primary w-full justify-center mt-2" disabled={loading}>
          {loading ? <Spinner size="sm" /> : null}
          {t(lang, 'register')}
        </button>
      </form>

      <p className="text-sm text-center text-health-secondary mt-8">
        {t(lang, 'haveAccount')}{' '}
        <Link to="/login" className="text-health-accent hover:text-health-accent/80 hover:underline font-bold transition-all">
          {t(lang, 'signIn')}
        </Link>
      </p>
    </AuthLayout>
  )
}
