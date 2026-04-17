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
      setAuth(res.data)
      const role = res?.data?.user?.role
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
  const [form, setForm] = useState({ firstName: '', lastName: '', email: '', password: '', role: 'Klient' })
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
            <option value="Admin">Admin</option>
          </select>
        </div>

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
