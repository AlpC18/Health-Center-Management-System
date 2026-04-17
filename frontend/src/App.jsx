import { lazy, Suspense } from 'react'
import { BrowserRouter, Routes, Route, Navigate, Outlet } from 'react-router-dom'
import { Toaster } from 'react-hot-toast'
import useAuthStore from './store/authStore'
import Layout from './components/layout/Layout'
import SignalRListener from './components/SignalRListener'
import PortalLayout from './components/layout/PortalLayout'

const LoginPage = lazy(() => import('./pages/AuthPages').then((m) => ({ default: m.LoginPage })))
const RegisterPage = lazy(() => import('./pages/AuthPages').then((m) => ({ default: m.RegisterPage })))
const DashboardPage = lazy(() => import('./pages/DashboardPage'))
const KlientetPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.KlientetPage })))
const SherbiimetPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.SherbiimetPage })))
const TerapistetPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.TerapistetPage })))
const TerminetPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.TerminetPage })))
const PaketaPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.PaketaPage })))
const AnetaresiimetPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.AnetaresiimetPage })))
const ProgrametPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.ProgrametPage })))
const ProduktetPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.ProduktetPage })))
const ShitjetPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.ShitjetPage })))
const VlereisiimetPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.VlereisiimetPage })))
const CalendarPage = lazy(() => import('./pages/CalendarPage'))
const ClientPortalPage = lazy(() => import('./pages/ClientPortalPage'))
const ProfilePage = lazy(() => import('./pages/ProfilePage'))
const AuditLogsPage = lazy(() => import('./pages/entities').then((m) => ({ default: m.AuditLogsPage })))
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
  const token = useAuthStore((s) => s.accessToken)
  if (!token) return <Navigate to="/login" replace />
  return <PortalLayout>{children}</PortalLayout>
}

function GuestRoute() {
  const { accessToken, user } = useAuthStore()
  const roles = Array.isArray(user?.roles) ? user.roles : []
  const isAdmin = user?.role === 'Admin' || roles.includes('Admin') || roles.includes('Staff')
  const defaultPath = isAdmin ? '/dashboard' : '/portal/dashboard'
  return accessToken ? <Navigate to={defaultPath} replace /> : <Outlet />
}

export default function App() {
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
        </Route>

        {/* Protected routes */}
        <Route element={<ProtectedRoute />}>
          <Route element={<Layout />}>
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/klientet" element={<KlientetPage />} />
            <Route path="/sherbimet" element={<SherbiimetPage />} />
            <Route path="/terapistet" element={<TerapistetPage />} />
            <Route path="/terminet" element={<TerminetPage />} />
            <Route path="/paketat" element={<PaketaPage />} />
            <Route path="/anetaresimet" element={<AnetaresiimetPage />} />
            <Route path="/programet" element={<ProgrametPage />} />
            <Route path="/produktet" element={<ProduktetPage />} />
            <Route path="/shitjet" element={<ShitjetPage />} />
            <Route path="/vlereisimet" element={<VlereisiimetPage />} />
            <Route path="/audit-logs" element={<AuditLogsPage />} />
            <Route path="/calendar" element={<CalendarPage />} />
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

        {/* Fallback */}
        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
      </Suspense>
    </BrowserRouter>
  )
}
