import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import toast from 'react-hot-toast'
import { Leaf, Eye, EyeOff } from 'lucide-react'
import { authApi } from '../api/api'
import useAuthStore from '../store/authStore'
import { Spinner } from '../components/ui/index'
import useLangStore from '../store/langStore'
import { t } from '../i18n'

const passwordChecks = [
  { label: 'Te pakten 8 karaktere', test: (value) => value.length >= 8 },
  { label: 'Te pakten nje numer', test: (value) => /\d/.test(value) },
  { label: 'Te pakten nje shkronje te madhe', test: (value) => /[A-Z]/.test(value) },
  { label: 'Te pakten nje simbol', test: (value) => /[^A-Za-z0-9]/.test(value) },
]

function getPasswordStrength(password) {
  const passed = passwordChecks.filter((check) => check.test(password)).length
  if (passed <= 1) return { label: 'I dobet', width: '25%', color: 'bg-red-500' }
  if (passed === 2) return { label: 'Mesatar', width: '50%', color: 'bg-yellow-500' }
  if (passed === 3) return { label: 'I mire', width: '75%', color: 'bg-blue-500' }
  return { label: 'I forte', width: '100%', color: 'bg-health-brand' }
}

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
          <div className="flex justify-end mt-2">
            <Link to="/forgot-password" className="text-sm font-semibold text-health-accent hover:text-health-accent/80 hover:underline">
              Forgot Password?
            </Link>
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

export function ForgotPasswordPage() {
  const [email, setEmail] = useState('')
  const [loading, setLoading] = useState(false)
  const [sent, setSent] = useState(false)

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (!email.trim()) {
      toast.error('Shkruani email-in.')
      return
    }

    setLoading(true)
    try {
      const res = await authApi.forgotPassword({ email })
      setSent(true)
      toast.success(res.data?.message || 'Kontrolloni email-in per linkun e resetimit.')
    } catch (err) {
      toast.error(err.response?.data?.message || 'Nuk mund te dergohej kerkesa per resetim.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <AuthLayout title="Reset Password" subtitle="Shkruani email-in e llogarise. Nese ekziston, do te pranoni nje link resetimi.">
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="label">Email</label>
          <input
            type="email"
            className="input"
            placeholder="ju@example.com"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            autoComplete="email"
          />
        </div>

        {sent && (
          <div className="rounded-lg border border-health-border bg-health-bg px-4 py-3 text-sm text-health-secondary">
            Nese email-i ekziston, link-u per resetim u dergua. Link-u skadon pas 30 minutash.
          </div>
        )}

        <button type="submit" className="btn-primary w-full justify-center" disabled={loading}>
          {loading ? <Spinner size="sm" /> : null}
          Dergo linkun
        </button>
      </form>

      <p className="text-sm text-center text-health-secondary mt-8">
        <Link to="/login" className="text-health-accent hover:text-health-accent/80 hover:underline font-bold transition-all">
          Kthehu te kyçja
        </Link>
      </p>
    </AuthLayout>
  )
}

export function ResetPasswordPage() {
  const { token = '' } = useParams()
  const navigate = useNavigate()
  const [form, setForm] = useState({ newPassword: '', confirmPassword: '' })
  const [loading, setLoading] = useState(false)
  const [tokenStatus, setTokenStatus] = useState({ checking: true, valid: false, message: '' })
  const strength = getPasswordStrength(form.newPassword)
  const isStrongEnough = passwordChecks.slice(0, 2).every((check) => check.test(form.newPassword))

  useEffect(() => {
    let cancelled = false

    const validate = async () => {
      if (!token) {
        setTokenStatus({ checking: false, valid: false, message: 'Link-u i resetimit mungon.' })
        return
      }

      try {
        await authApi.validateResetToken(token)
        if (!cancelled) setTokenStatus({ checking: false, valid: true, message: '' })
      } catch (err) {
        if (!cancelled) {
          setTokenStatus({
            checking: false,
            valid: false,
            message: err.response?.data?.message || 'Link-u per resetim eshte i pavlefshem ose ka skaduar.',
          })
        }
      }
    }

    validate()
    return () => { cancelled = true }
  }, [token])

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (!token) {
      toast.error('Link-u i resetimit mungon.')
      return
    }
    if (form.newPassword !== form.confirmPassword) {
      toast.error('Fjalekalimet nuk perputhen.')
      return
    }
    if (!isStrongEnough) {
      toast.error('Fjalekalimi duhet te kete te pakten 8 karaktere dhe nje numer.')
      return
    }

    setLoading(true)
    try {
      const res = await authApi.resetPassword({ token, ...form })
      toast.success(res.data?.message || 'Fjalekalimi u ndryshua.')
      navigate('/login')
    } catch (err) {
      const errors = err.response?.data?.errors
      toast.error(Array.isArray(errors) ? errors[0] : err.response?.data?.message || 'Resetimi deshtoi.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <AuthLayout title="New Password" subtitle="Vendosni fjalekalimin e ri per llogarine tuaj.">
      {tokenStatus.checking && (
        <div className="flex items-center gap-2 text-sm text-health-secondary">
          <Spinner size="sm" />
          Duke verifikuar linkun...
        </div>
      )}

      {!tokenStatus.checking && !tokenStatus.valid && (
        <div className="space-y-5">
          <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
            {tokenStatus.message}
          </div>
          <Link to="/forgot-password" className="btn-primary w-full justify-center">
            Kerko link te ri
          </Link>
        </div>
      )}

      {!tokenStatus.checking && tokenStatus.valid && (
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="label">New password</label>
          <input
            type="password"
            className="input"
            placeholder="Minimum 8 karaktere"
            value={form.newPassword}
            onChange={(e) => setForm((p) => ({ ...p, newPassword: e.target.value }))}
            autoComplete="new-password"
          />
          <div className="mt-3">
            <div className="h-2 rounded-full bg-health-bg overflow-hidden border border-health-border">
              <div className={`h-full ${strength.color} transition-all`} style={{ width: strength.width }} />
            </div>
            <p className="mt-2 text-xs font-semibold text-health-secondary">Strength: {strength.label}</p>
          </div>
          <div className="mt-3 grid gap-1">
            {passwordChecks.map((check) => {
              const passed = check.test(form.newPassword)
              return (
                <p key={check.label} className={`text-xs ${passed ? 'text-health-brand' : 'text-health-secondary'}`}>
                    {passed ? 'OK' : '-'} {check.label}
                </p>
              )
            })}
          </div>
        </div>

        <div>
          <label className="label">Confirm password</label>
          <input
            type="password"
            className="input"
            placeholder="Perserit fjalekalimin"
            value={form.confirmPassword}
            onChange={(e) => setForm((p) => ({ ...p, confirmPassword: e.target.value }))}
            autoComplete="new-password"
          />
        </div>

        <button type="submit" className="btn-primary w-full justify-center" disabled={loading}>
          {loading ? <Spinner size="sm" /> : null}
          Ndrysho fjalekalimin
        </button>
      </form>
      )}

      <p className="text-sm text-center text-health-secondary mt-8">
        <Link to="/login" className="text-health-accent hover:text-health-accent/80 hover:underline font-bold transition-all">
          Kthehu te kyçja
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
