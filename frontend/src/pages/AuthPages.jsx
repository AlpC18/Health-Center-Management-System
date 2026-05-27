import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import toast from 'react-hot-toast'
import { Eye, EyeOff, Check, X as XIcon, Stethoscope, User, Heart, Languages, Lock, ChevronDown, ChevronUp, AlertCircle } from 'lucide-react'
import { authApi } from '../api/api'
import useAuthStore from '../store/authStore'
import { Spinner } from '../components/ui/index'
import useLangStore from '../store/langStore'
import { t } from '../i18n'
import healthLogo from '../assets/health-logo.png'
import loginBg from '../assets/login-bg.png'

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

// Basic email shape check used for live validation ticks — server still re-validates.
const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
const isValidEmail = (email) => EMAIL_RE.test(email.trim())

function LanguageToggle() {
  const { lang, toggleLang } = useLangStore()
  return (
    <button
      type="button"
      onClick={toggleLang}
      className="btn-glass-prism z-20"
      style={{ position: 'absolute', top: '1.25rem', right: '1.25rem' }}
      title={lang === 'sq' ? 'Ndrysho gjuhen' : 'Change language'}
    >
      <Languages className="h-3.5 w-3.5 relative" style={{ filter: 'drop-shadow(0 0 6px rgba(255,255,255,0.6))' }} />
      <span className="relative">{lang === 'sq' ? 'SQ' : 'EN'}</span>
      <span className="relative opacity-40">/</span>
      <span className="relative opacity-40">{lang === 'sq' ? 'EN' : 'SQ'}</span>
    </button>
  )
}

function AuthLayout({ children, title, subtitle }) {
  return (
    <div
      className="min-h-screen flex items-center justify-center p-6 relative"
      style={{
        backgroundImage: `url(${loginBg})`,
        backgroundSize: 'cover',
        backgroundPosition: 'center',
        backgroundRepeat: 'no-repeat',
        backgroundAttachment: 'fixed',
      }}
    >
      {/* Language toggle — floating pill, available on every auth page */}
      <LanguageToggle />

      {/* Dark vignette overlay — keeps the glass card readable on top of the bright cyan waveform */}
      <div
        aria-hidden="true"
        className="absolute inset-0 pointer-events-none"
        style={{
          background: 'radial-gradient(ellipse at center, rgba(2,10,28,0.35) 0%, rgba(2,10,28,0.72) 100%)',
        }}
      />

      <div className="w-full max-w-md relative z-10">
        {/* Logo */}
        <div className="flex flex-col items-center mb-8">
          <div
            className="rounded-3xl mb-5 relative overflow-hidden flex items-center justify-center logo-heartbeat"
            style={{
              width: '112px',
              height: '112px',
              background: 'rgba(255,255,255,0.92)',
              border: '1px solid rgba(255,255,255,0.4)',
              backdropFilter: 'blur(18px) saturate(180%)',
              boxShadow: '0 20px 50px -12px rgba(0,0,0,0.55), inset 0 1px 0 rgba(255,255,255,0.65)',
            }}
          >
            <img src={healthLogo} alt="Health Center Management System" className="w-full h-full object-contain p-2" />
          </div>
          <h1 className="text-2xl font-bold tracking-tight text-center" style={{ color: '#fff', textShadow: '0 2px 12px rgba(0,0,0,0.6)' }}>
            Health Center
          </h1>
          <p className="text-[11px] mt-1 font-semibold tracking-[0.2em] uppercase" style={{ color: 'rgba(255,255,255,0.78)' }}>
            Management System
          </p>
        </div>

        {/* Card — slightly stronger backdrop so it pops against the dark image */}
        <div
          className="card p-8"
          style={{
            background: 'rgba(255,255,255,0.88)',
            border: '1px solid rgba(255,255,255,0.55)',
            backdropFilter: 'blur(22px) saturate(180%)',
            WebkitBackdropFilter: 'blur(22px) saturate(180%)',
            boxShadow: '0 30px 80px -16px rgba(0,0,0,0.55), 0 4px 18px -6px rgba(0,0,0,0.35)',
          }}
        >
          <h2 className="text-xl font-bold text-health-primary mb-2">{title}</h2>
          {subtitle && <p className="text-sm text-health-secondary mb-8 leading-relaxed">{subtitle}</p>}
          {children}
        </div>
      </div>
    </div>
  )
}

// Demo credentials for the graded project — visible to anyone who hits the login.
// One-click "Fill" buttons make professor / reviewer onboarding trivial.
const DEMO_ACCOUNTS = [
  { label: 'Admin',     email: 'admin@wellness.com',     password: 'Admin123!',     tab: 'Klient', tone: 'red' },
  { label: 'Doktor',    email: 'therapist@wellness.com', password: 'Therapist123!', tab: 'Doktor', tone: 'amber' },
  { label: 'Klient',    email: 'client@wellness.com',    password: 'Client123!',    tab: 'Klient', tone: 'blue' },
]

export function LoginPage() {
  const navigate = useNavigate()
  const { setAuth } = useAuthStore()
  const { lang } = useLangStore()
  const [form, setForm] = useState({ email: '', password: '', twoFactorCode: '' })
  const [loginType, setLoginType] = useState('Klient')
  const [requiresTwoFactor, setRequiresTwoFactor] = useState(false)
  const [showPassword, setShowPassword] = useState(false)
  const [rememberMe, setRememberMe] = useState(true)   // default-on for course-1 demo convenience
  const [capsLockOn, setCapsLockOn] = useState(false)
  const [loading, setLoading] = useState(false)
  const [showDemo, setShowDemo] = useState(false)

  // Reflect the OS Caps Lock state. event.getModifierState works on keydown/keyup.
  const checkCapsLock = (e) => {
    if (typeof e.getModifierState === 'function') {
      setCapsLockOn(e.getModifierState('CapsLock'))
    }
  }

  const fillDemo = (acc) => {
    setForm({ email: acc.email, password: acc.password, twoFactorCode: '' })
    setLoginType(acc.tab)
    setRequiresTwoFactor(false)
    setShowDemo(false)
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (!form.email || !form.password) {
      toast.error(t(lang, 'fillFields'))
      return
    }
    setLoading(true)
    try {
      // rememberMe is sent to the API as a hint; the backend can use it later to
      // extend the refresh-token lifetime. Today it's a no-op on the server.
      const payload = { email: form.email, password: form.password, rememberMe }
      if (requiresTwoFactor && form.twoFactorCode.trim()) {
        payload.twoFactorCode = form.twoFactorCode.trim()
      }
      const res = await authApi.login(payload)
      if (res?.data?.requiresTwoFactor) {
        setRequiresTwoFactor(true)
        toast('Shkruani kodin 2FA nga aplikacioni autentikues.')
        return
      }
      const role = res?.data?.user?.role

      // Admin credentials override the selected tab and always go to the admin panel.
      if (role !== 'Admin') {
        if (loginType === 'Doktor' && role !== 'Therapist') {
          toast.error('Kjo llogari nuk eshte Doktor.')
          return
        }
        if (loginType === 'Klient' && role !== 'Klient') {
          toast.error('Kjo llogari nuk eshte Klient.')
          return
        }
      }

      setAuth(res.data)
      navigate(role === 'Klient' ? '/portal/dashboard' : role === 'Therapist' ? '/terapist-portal' : '/dashboard')
    } catch {
      toast.error(t(lang, 'loginError'))
    } finally {
      setLoading(false)
    }
  }

  return (
    <AuthLayout title={t(lang, 'welcome')} subtitle={t(lang, 'loginSubtitle')}>
      <form onSubmit={handleSubmit} className="space-y-4">
        {/* Two clean role tabs — Therapist was redundant (same backend role as Doktor) */}
        <div className="seg-wrap grid-cols-2">
          <button
            type="button"
            onClick={() => setLoginType('Klient')}
            className={`seg-btn flex items-center justify-center gap-1.5 ${loginType === 'Klient' ? 'seg-btn-active' : ''}`}
          >
            <User className="h-3.5 w-3.5" />
            Klient
          </button>
          <button
            type="button"
            onClick={() => setLoginType('Doktor')}
            className={`seg-btn flex items-center justify-center gap-1.5 ${loginType === 'Doktor' ? 'seg-btn-active' : ''}`}
          >
            <Stethoscope className="h-3.5 w-3.5" />
            Doktor
          </button>
        </div>

        {requiresTwoFactor && (
          <div>
            <label className="label">Kodi 2FA</label>
            <input
              type="text"
              inputMode="numeric"
              className="input"
              placeholder="123456"
              value={form.twoFactorCode}
              onChange={(e) => setForm((p) => ({ ...p, twoFactorCode: e.target.value }))}
              autoComplete="one-time-code"
            />
          </div>
        )}

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
              onKeyDown={checkCapsLock}
              onKeyUp={checkCapsLock}
              autoComplete="current-password"
            />
            <button
              type="button"
              onClick={() => setShowPassword((v) => !v)}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
              tabIndex={-1}
            >
              {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
            </button>
          </div>

          {/* Caps Lock warning — appears only when the user is actively typing with Caps on */}
          {capsLockOn && (
            <div className="mt-2 flex items-center gap-1.5 text-[11px] font-semibold text-amber-600">
              <AlertCircle className="h-3.5 w-3.5" />
              {lang === 'sq' ? 'Caps Lock është aktiv' : 'Caps Lock is on'}
            </div>
          )}
        </div>

        {/* Remember me + Forgot password row — classic login layout */}
        <div className="flex items-center justify-between text-sm">
          <label className="flex items-center gap-2 cursor-pointer select-none">
            <input
              type="checkbox"
              checked={rememberMe}
              onChange={(e) => setRememberMe(e.target.checked)}
              className="w-4 h-4 rounded accent-health-brand"
            />
            <span className="text-health-secondary font-medium">
              {lang === 'sq' ? 'Më mbaj të kyçur' : 'Remember me'}
            </span>
          </label>
          <Link
            to="/forgot-password"
            className="font-semibold text-health-brand hover:text-health-brand/80 hover:underline transition-colors"
          >
            {lang === 'sq' ? 'Keni harruar?' : 'Forgot password?'}
          </Link>
        </div>

        {/* Submit — heartbeat icon + text instead of generic spinner while loading */}
        <button type="submit" className="btn-primary w-full justify-center mt-2" disabled={loading}>
          {loading
            ? <span className="flex items-center gap-2"><Heart className="h-4 w-4 fill-current animate-[heartbeatBeat_900ms_ease-in-out_infinite]" /> {lang === 'sq' ? 'Duke u kyçur...' : 'Signing in...'}</span>
            : t(lang, 'login')}
        </button>
      </form>

      {/* Demo credentials — collapsible footer, one-click fill */}
      <div className="mt-6 pt-4 border-t border-health-border/60">
        <button
          type="button"
          onClick={() => setShowDemo((v) => !v)}
          className="w-full flex items-center justify-center gap-1.5 text-[11px] font-bold uppercase tracking-wider text-health-secondary hover:text-health-brand transition-colors"
        >
          {showDemo ? <ChevronUp className="h-3 w-3" /> : <ChevronDown className="h-3 w-3" />}
          {lang === 'sq' ? 'Llogari demo' : 'Demo accounts'}
        </button>

        {showDemo && (
          <div className="mt-3 grid grid-cols-3 gap-2 animate-[fadeSlideIn_220ms_ease-out]">
            {DEMO_ACCOUNTS.map((acc) => (
              <button
                key={acc.email}
                type="button"
                onClick={() => fillDemo(acc)}
                className="group p-2.5 rounded-xl border border-health-border bg-white/60 hover:bg-white/90 hover:border-health-brand/40 transition-all text-left"
                title={`${acc.email} / ${acc.password}`}
              >
                <p className="text-[10px] font-bold uppercase tracking-wider text-health-secondary group-hover:text-health-brand">
                  {acc.label}
                </p>
                <p className="text-[10px] text-health-primary font-medium truncate mt-0.5">
                  {acc.email}
                </p>
              </button>
            ))}
          </div>
        )}
      </div>

      <p className="text-sm text-center text-health-secondary mt-6">
        {t(lang, 'noAccount')}{' '}
        <Link to="/register" className="text-health-brand hover:text-health-brand/80 hover:underline font-bold transition-all">
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
        <Link to="/login" className="text-health-brand hover:text-health-brand/80 hover:underline font-bold transition-all">
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
        <Link to="/login" className="text-health-brand hover:text-health-brand/80 hover:underline font-bold transition-all">
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
    confirmPassword: '',
    role: 'Klient',
    specializimi: '',
    licenca: '',
    telefoni: '',
    acceptedConsent: false,
  })
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirm, setShowConfirm] = useState(false)
  const [loading, setLoading] = useState(false)

  // Live validation derived state — no extra useEffects needed.
  const emailValid = form.email.length > 0 && isValidEmail(form.email)
  const emailInvalid = form.email.length > 0 && !emailValid
  const passwordStrength = getPasswordStrength(form.password)
  const passwordValid = passwordChecks.every((c) => c.test(form.password))
  const confirmFilled = form.confirmPassword.length > 0
  const confirmMatches = confirmFilled && form.confirmPassword === form.password

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (!form.firstName || !form.lastName || !form.email || !form.password) {
      toast.error(t(lang, 'fillFields'))
      return
    }
    if (!emailValid) {
      toast.error('Email-i nuk eshte i vlefshem.')
      return
    }
    if (!passwordValid) {
      toast.error('Fjalekalimi nuk i ploteson kerkesat e sigurise.')
      return
    }
    if (!confirmMatches) {
      toast.error('Fjalekalimet nuk perputhen.')
      return
    }
    if (form.role === 'Therapist' && !form.specializimi.trim()) {
      toast.error('Specializimi eshte i detyrueshem per doktor.')
      return
    }
    if (!form.acceptedConsent) {
      toast.error('Duhet te pranoni kushtet dhe politiken e privatesise.')
      return
    }
    setLoading(true)
    try {
      // confirmPassword is client-only — strip it before sending.
      const { confirmPassword, ...payload } = form
      const res = await authApi.register(payload)
      setAuth(res.data)
      const role = res?.data?.user?.role
      navigate(role === 'Klient' ? '/portal/dashboard' : role === 'Therapist' ? '/terapist-portal' : '/dashboard')
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
        {/* Name row */}
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

        {/* Email — live validation tick */}
        <div>
          <label className="label">{t(lang, 'email')}</label>
          <div className="relative">
            <input
              type="email"
              className={`input pr-10 ${emailInvalid ? 'border-red-400 focus:border-red-500' : ''}`}
              placeholder="ju@example.com"
              value={form.email}
              onChange={(e) => setForm((p) => ({ ...p, email: e.target.value }))}
              autoComplete="email"
            />
            {form.email.length > 0 && (
              <span className="absolute right-3 top-1/2 -translate-y-1/2">
                {emailValid
                  ? <Check className="h-4 w-4 text-emerald-500" />
                  : <XIcon className="h-4 w-4 text-red-400" />}
              </span>
            )}
          </div>
        </div>

        {/* Password — with show/hide + strength bar + requirements checklist */}
        <div>
          <label className="label">{t(lang, 'password')}</label>
          <div className="relative">
            <input
              type={showPassword ? 'text' : 'password'}
              className="input pr-10"
              placeholder="••••••••"
              value={form.password}
              onChange={(e) => setForm((p) => ({ ...p, password: e.target.value }))}
              autoComplete="new-password"
            />
            <button
              type="button"
              onClick={() => setShowPassword((v) => !v)}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
              tabIndex={-1}
            >
              {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
            </button>
          </div>

          {form.password.length > 0 && (
            <div className="mt-2 space-y-2">
              {/* Strength bar */}
              <div className="flex items-center gap-2">
                <div className="flex-1 h-1.5 bg-gray-200 rounded-full overflow-hidden">
                  <div
                    className={`h-full ${passwordStrength.color} transition-all duration-300`}
                    style={{ width: passwordStrength.width }}
                  />
                </div>
                <span className="text-[10px] font-bold uppercase tracking-wider text-health-secondary min-w-[55px] text-right">
                  {passwordStrength.label}
                </span>
              </div>

              {/* Requirements checklist */}
              <ul className="grid grid-cols-2 gap-x-3 gap-y-1">
                {passwordChecks.map((check) => {
                  const passed = check.test(form.password)
                  return (
                    <li
                      key={check.label}
                      className={`flex items-center gap-1.5 text-[11px] ${passed ? 'text-emerald-600' : 'text-gray-400'}`}
                    >
                      {passed
                        ? <Check className="h-3 w-3 flex-shrink-0" />
                        : <span className="h-3 w-3 rounded-full border border-gray-300 flex-shrink-0" />}
                      <span className={passed ? 'font-semibold' : ''}>{check.label}</span>
                    </li>
                  )
                })}
              </ul>
            </div>
          )}
        </div>

        {/* Confirm password */}
        <div>
          <label className="label">Konfirmo fjalekalimin</label>
          <div className="relative">
            <input
              type={showConfirm ? 'text' : 'password'}
              className={`input pr-16 ${confirmFilled && !confirmMatches ? 'border-red-400 focus:border-red-500' : ''}`}
              placeholder="••••••••"
              value={form.confirmPassword}
              onChange={(e) => setForm((p) => ({ ...p, confirmPassword: e.target.value }))}
              autoComplete="new-password"
            />
            <div className="absolute right-3 top-1/2 -translate-y-1/2 flex items-center gap-2">
              {confirmFilled && (
                confirmMatches
                  ? <Check className="h-4 w-4 text-emerald-500" />
                  : <XIcon className="h-4 w-4 text-red-400" />
              )}
              <button
                type="button"
                onClick={() => setShowConfirm((v) => !v)}
                className="text-gray-400 hover:text-gray-600"
                tabIndex={-1}
              >
                {showConfirm ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
              </button>
            </div>
          </div>
          {confirmFilled && !confirmMatches && (
            <p className="text-[11px] text-red-500 mt-1 font-medium">Fjalekalimet nuk perputhen.</p>
          )}
        </div>

        {/* Role — segmented control matching login page */}
        <div>
          <label className="label">Roli</label>
          <div className="seg-wrap grid-cols-2">
            <button
              type="button"
              onClick={() => setForm((p) => ({ ...p, role: 'Klient' }))}
              className={`seg-btn flex items-center justify-center gap-1.5 ${form.role === 'Klient' ? 'seg-btn-active' : ''}`}
            >
              <User className="h-3.5 w-3.5" />
              Klient
            </button>
            <button
              type="button"
              onClick={() => setForm((p) => ({ ...p, role: 'Therapist' }))}
              className={`seg-btn flex items-center justify-center gap-1.5 ${form.role === 'Therapist' ? 'seg-btn-active' : ''}`}
            >
              <Stethoscope className="h-3.5 w-3.5" />
              Doktor
            </button>
          </div>
        </div>

        {/* Therapist-only block — animated slide-in */}
        {form.role === 'Therapist' && (
          <div className="space-y-4 p-4 rounded-xl border border-health-brand/30 bg-health-brand/5 animate-[fadeSlideIn_240ms_ease-out]">
            <div className="flex items-center gap-2">
              <Stethoscope className="h-4 w-4 text-health-brand" />
              <p className="text-xs font-bold text-health-brand uppercase tracking-wider">
                Te Dhenat e Doktorit
              </p>
            </div>
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

        <label className="flex items-start gap-2 text-sm text-health-secondary cursor-pointer select-none">
          <input
            type="checkbox"
            checked={form.acceptedConsent}
            onChange={(e) => setForm((p) => ({ ...p, acceptedConsent: e.target.checked, consentVersion: 'v1' }))}
            className="mt-1 w-4 h-4 rounded accent-health-brand"
          />
          <span>
            Pranoj kushtet, politiken e privatesise dhe regjistrimin e consent-it tim.
          </span>
        </label>

        <button type="submit" className="btn-primary w-full justify-center mt-2" disabled={loading}>
          {loading ? <Spinner size="sm" /> : null}
          {t(lang, 'register')}
        </button>
      </form>

      <p className="text-sm text-center text-health-secondary mt-8">
        {t(lang, 'haveAccount')}{' '}
        <Link to="/login" className="text-health-brand hover:text-health-brand/80 hover:underline font-bold transition-all">
          {t(lang, 'signIn')}
        </Link>
      </p>
    </AuthLayout>
  )
}
