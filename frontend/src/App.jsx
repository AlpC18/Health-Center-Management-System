import { lazy, Suspense, useEffect } from 'react'
import { BrowserRouter, Routes, Route, Navigate, Outlet } from 'react-router-dom'
import { Toaster } from 'react-hot-toast'
import useAuthStore from './store/authStore'
import Layout from './components/layout/Layout'
import SignalRListener from './components/SignalRListener'
import PortalLayout from './components/layout/PortalLayout'
import { ForgotPasswordPage, LoginPage, RegisterPage, ResetPasswordPage } from './pages/AuthPages'

const DashboardPage = lazy(() => import('./pages/DashboardPage'))
const KlientetPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.KlientetPage })))
const SherbiimetPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.SherbiimetPage })))
const TerapistetPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.TerapistetPage })))
const TerminetPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.TerminetPage })))
const PaketaPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.PaketaPage })))
const AnetaresiimetPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.AnetaresiimetPage })))
const ProgrametPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.ProgrametPage })))
const KlientProgrametPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.KlientProgrametPage })))
const ProduktetPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.ProduktetPage })))
const ShitjetPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.ShitjetPage })))
const VlereisiimetPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.VlereisiimetPage })))
const CalendarPage = lazy(() => import('./pages/CalendarPage'))
const ClientPortalPage = lazy(() => import('./pages/ClientPortalPage'))
const ProfilePage = lazy(() => import('./pages/ProfilePage'))
const AuditLogsPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.AuditLogsPage })))
const SallatPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.SallatPage })))
const FurnizuesitPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.FurnizuesitPage })))
const LajmerimetPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.LajmerimetPage })))
const ZbritjetPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.ZbritjetPage })))
const PushimetPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.PushimetPage })))
const AdvancedFeaturesPage = lazy(() => import('./pages/AdvancedFeaturesPage'))
const ReportsPage = lazy(() => import('./pages/ReportsPage'))
const TherapistPortalPage = lazy(() => import('./pages/TherapistPortalPage'))
const KlientProfilePage = lazy(() => import('./pages/KlientProfilePage'))
const NotFoundPage = lazy(() => import('./pages/NotFoundPage'))
const PortalDashboard = lazy(() => import('./pages/portal/PortalDashboard'))
const PortalTerminet = lazy(() => import('./pages/portal/PortalTerminet'))
const PortalRezevo = lazy(() => import('./pages/portal/PortalRezevo'))
const PortalAnetaresimi = lazy(() => import('./pages/portal/PortalAnetaresimi'))
const PortalProgramet = lazy(() => import('./pages/portal/PortalProgramet'))
const PortalProduktet = lazy(() => import('./pages/portal/PortalProduktet'))
const PortalShitjet = lazy(() => import('./pages/portal/PortalShitjet'))
const PortalVlereisimet = lazy(() => import('./pages/portal/PortalVlereisimet'))
const PortalProfili = lazy(() => import('./pages/portal/PortalProfili'))
const NotificationsInboxPage = lazy(() => import('./pages/Phase3Pages').then((m) => ({ default: m.NotificationsInboxPage })))
const TwoFactorSetupPage = lazy(() => import('./pages/Phase3Pages').then((m) => ({ default: m.TwoFactorSetupPage })))
const ConsentPage = lazy(() => import('./pages/Phase3Pages').then((m) => ({ default: m.ConsentPage })))
const PrivacySelfServicePage = lazy(() => import('./pages/Phase3Pages').then((m) => ({ default: m.PrivacySelfServicePage })))
const TemplatesAdminPage = lazy(() => import('./pages/Phase3Pages').then((m) => ({ default: m.TemplatesAdminPage })))
const LocationsAdminPage = lazy(() => import('./pages/Phase3Pages').then((m) => ({ default: m.LocationsAdminPage })))

function PageFallback() {
  return (
    <div className="flex items-center justify-center h-screen bg-health-bg">
      <div className="flex flex-col items-center gap-3">
        <div className="w-10 h-10 border-4 border-health-brand border-t-transparent rounded-full animate-spin" />
        <p className="text-sm text-health-secondary">Duke ngarkuar...</p>
      </div>
    </div>
  )
}

function ProtectedRoute() {
  const accessToken = useAuthStore((s) => s.accessToken)
  return accessToken ? <Outlet /> : <Navigate to="/login" replace />
}

function KlientRoute({ children }) {
  const { accessToken, user } = useAuthStore()
  if (!accessToken) return <Navigate to="/login" replace />
  const role = user?.role ?? ''
  const isAdmin = role === 'Admin' || role === 'Staff' || role === 'Therapist'
  if (isAdmin) return <Navigate to="/dashboard" replace />
  return <PortalLayout>{children}</PortalLayout>
}

function AdminRoute() {
  const { accessToken, user } = useAuthStore()
  if (!accessToken) return <Navigate to="/login" replace />
  if (user?.role !== 'Admin') return <Navigate to="/dashboard" replace />
  return <Outlet />
}

function TherapistRoute({ children }) {
  const { accessToken, user } = useAuthStore()
  if (!accessToken) return <Navigate to="/login" replace />
  if (user?.role !== 'Therapist' && user?.role !== 'Admin') return <Navigate to="/dashboard" replace />
  return children
}

function GuestRoute() {
  const { accessToken, user } = useAuthStore()
  const roles = Array.isArray(user?.roles) ? user.roles : []
  const isAdmin = user?.role === 'Admin' || roles.includes('Admin') || roles.includes('Staff')
  const isTherapist = user?.role === 'Therapist' || roles.includes('Therapist')
  const defaultPath = isAdmin ? '/dashboard' : isTherapist ? '/terapist-portal' : '/portal/dashboard'
  return accessToken ? <Navigate to={defaultPath} replace /> : <Outlet />
}

export default function App() {
  const hydrated = useAuthStore((s) => s.hydrated)
  const hydrate = useAuthStore((s) => s.hydrate)

  useEffect(() => {
    hydrate()
  }, [hydrate])

  // Wait for the boot-time session restore before routing so a page
  // refresh doesn't flash the login screen for an authenticated user.
  if (!hydrated) return <PageFallback />

  return (
    <BrowserRouter>
      <SignalRListener />
      <Toaster
        position="top-right"
        toastOptions={{
          style: { 
            borderRadius: '12px', 
            fontSize: '14px', 
            maxWidth: '400px',
            background: '#161B22',
            color: '#F0F6FC',
            border: '1px solid #30363D'
          }
        }}
      />
      <Suspense fallback={<PageFallback />}>
      <Routes>
        {/* Guest-only routes */}
        <Route element={<GuestRoute />}>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/forgot-password" element={<ForgotPasswordPage />} />
          <Route path="/reset-password/:token" element={<ResetPasswordPage />} />
        </Route>

        {/* Protected routes */}
        <Route element={<ProtectedRoute />}>
          <Route element={<Layout />}>
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/terapist-portal" element={<TherapistRoute><TherapistPortalPage /></TherapistRoute>} />
            <Route path="/klientet" element={<KlientetPage />} />
            <Route path="/klientet/:id" element={<KlientProfilePage />} />
            <Route path="/sherbimet" element={<SherbiimetPage />} />
            <Route path="/terapistet" element={<TerapistetPage />} />
            <Route path="/terminet" element={<TerminetPage />} />
            <Route path="/paketat" element={<PaketaPage />} />
            <Route path="/anetaresimet" element={<AnetaresiimetPage />} />
            <Route path="/programet" element={<ProgrametPage />} />
            <Route path="/klient-programet" element={<KlientProgrametPage />} />
            <Route path="/produktet" element={<ProduktetPage />} />
            <Route path="/shitjet" element={<ShitjetPage />} />
            <Route path="/vlereisimet" element={<VlereisiimetPage />} />
            <Route path="/sallat" element={<SallatPage />} />
            <Route path="/furnizuesit" element={<FurnizuesitPage />} />
            <Route path="/lajmerimet" element={<LajmerimetPage />} />
            <Route path="/zbritjet" element={<ZbritjetPage />} />
            <Route path="/pushimet" element={<PushimetPage />} />
            <Route path="/notifications" element={<NotificationsInboxPage />} />
            <Route path="/security/2fa" element={<TwoFactorSetupPage />} />
            <Route element={<AdminRoute />}>
              <Route path="/audit-logs" element={<AuditLogsPage />} />
              <Route path="/templates" element={<TemplatesAdminPage />} />
              <Route path="/locations" element={<LocationsAdminPage />} />
            </Route>
            <Route path="/calendar" element={<CalendarPage />} />
            <Route path="/advanced" element={<AdvancedFeaturesPage />} />
            <Route path="/reports" element={<ReportsPage />} />
            <Route path="/profile" element={<ProfilePage />} />
            <Route path="/portal" element={<ClientPortalPage />} />
          </Route>
        </Route>

        <Route path="/portal/dashboard" element={<KlientRoute><PortalDashboard /></KlientRoute>} />
        <Route path="/portal/terminet" element={<KlientRoute><PortalTerminet /></KlientRoute>} />
        <Route path="/portal/rezervo" element={<KlientRoute><PortalRezevo /></KlientRoute>} />
        <Route path="/portal/anetaresimi" element={<KlientRoute><PortalAnetaresimi /></KlientRoute>} />
        <Route path="/portal/programet" element={<KlientRoute><PortalProgramet /></KlientRoute>} />
        <Route path="/portal/produktet" element={<KlientRoute><PortalProduktet /></KlientRoute>} />
        <Route path="/portal/shitjet" element={<KlientRoute><PortalShitjet /></KlientRoute>} />
        <Route path="/portal/vlereisimet" element={<KlientRoute><PortalVlereisimet /></KlientRoute>} />
        <Route path="/portal/profili" element={<KlientRoute><PortalProfili /></KlientRoute>} />
        <Route path="/portal/notifications" element={<KlientRoute><NotificationsInboxPage /></KlientRoute>} />
        <Route path="/portal/consent" element={<KlientRoute><ConsentPage /></KlientRoute>} />
        <Route path="/portal/privacy" element={<KlientRoute><PrivacySelfServicePage /></KlientRoute>} />
        <Route path="/portal/2fa" element={<KlientRoute><TwoFactorSetupPage /></KlientRoute>} />

        {/* Fallback */}
        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
      </Suspense>
    </BrowserRouter>
  )
}
