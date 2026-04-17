import { useState } from 'react'
import { NavLink, Link, Outlet, useNavigate } from 'react-router-dom'
import toast from 'react-hot-toast'
import NotificationCenter from './NotificationCenter'
import {
  LayoutDashboard,
  Users,
  Stethoscope,
  UserCheck,
  Calendar,
  Package,
  CreditCard,
  Dumbbell,
  ShoppingBag,
  ShoppingCart,
  Star,
  CalendarDays,
  Leaf,
  LogOut,
  Menu,
  X,
  Sun,
  Moon,
  Keyboard,
  Activity,
} from 'lucide-react'
import useAuthStore from '../../store/authStore'
import useThemeStore from '../../store/themeStore'
import useLangStore from '../../store/langStore'
import { t } from '../../i18n'
import { authApi } from '../../api/api'
import useOnlineStatus from '../../hooks/useOnlineStatus'
import useKeyboardShortcuts from '../../hooks/useKeyboardShortcuts'
import OfflineBanner from '../ui/OfflineBanner'
import KeyboardShortcutsModal from '../ui/KeyboardShortcutsModal'
import ChatWidget from '../ui/ChatWidget'

const adminNavItems = [
  { to: '/dashboard', labelKey: 'dashboard', icon: LayoutDashboard },
  { to: '/klientet', labelKey: 'clients', icon: Users },
  { to: '/sherbimet', labelKey: 'services', icon: Stethoscope },
  { to: '/terapistet', labelKey: 'therapists', icon: UserCheck },
  { to: '/terminet', labelKey: 'appointments', icon: Calendar },
  { to: '/paketat', labelKey: 'packages', icon: Package },
  { to: '/anetaresimet', labelKey: 'memberships', icon: CreditCard },
  { to: '/programet', labelKey: 'programs', icon: Dumbbell },
  { to: '/produktet', labelKey: 'products', icon: ShoppingBag },
  { to: '/shitjet', labelKey: 'sales', icon: ShoppingCart },
  { to: '/vlereisimet', labelKey: 'reviews', icon: Star },
  { to: '/audit-logs', labelKey: 'auditLogs', icon: Activity },
  { to: '/calendar', labelKey: 'calendar', icon: CalendarDays },
]

const clientNavItems = [
  { to: '/portal', labelKey: 'Portal', icon: LayoutDashboard },
  { to: '/calendar', labelKey: 'Terminet e Mia', icon: CalendarDays },
  { to: '/profile', labelKey: 'Profili Im', icon: Users },
]

function SidebarContent({ onClose, lang = 'sq' }) {
  const { user, clearAuth } = useAuthStore()
  const navigate = useNavigate()
  const roles = Array.isArray(user?.roles) ? user.roles : []
  const isAdmin =
    user?.role === 'Admin' ||
    user?.role === 'Therapist' ||
    roles.includes('Admin') ||
    roles.includes('Staff') ||
    roles.includes('Therapist')

  const handleLogout = async () => {
    try {
      await authApi.logout()
    } catch {
      // ignore
    } finally {
      clearAuth()
      navigate('/login')
    }
  }

  return (
    <div className="flex flex-col h-full">
      {/* Logo */}
      <div className="flex items-center gap-3 px-6 py-6 border-b border-health-border">
        <div className="p-2 bg-health-brand rounded-xl">
          <Leaf className="h-5 w-5 text-white" />
        </div>
        <span className="font-bold text-health-primary text-lg tracking-tight">Wellness House</span>
        {onClose && (
          <button
            onClick={onClose}
            className="ml-auto p-2 text-health-secondary hover:text-health-primary lg:hidden"
          >
            <X className="h-5 w-5" />
          </button>
        )}
      </div>

      {/* Nav */}
      <nav className="flex-1 overflow-y-auto px-4 py-6 space-y-1">
        {(isAdmin ? adminNavItems : clientNavItems).map(({ to, labelKey, icon: Icon }) => (
          <NavLink
            key={to}
            to={to}
            onClick={onClose}
            className={({ isActive }) =>
              `flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-medium transition-all duration-200 ${
                isActive
                  ? 'bg-health-brand text-white shadow-lg shadow-health-brand/20'
                  : 'text-health-secondary hover:bg-health-hover hover:text-health-primary'
              }`
            }
          >
            <Icon className="h-4 w-4 flex-shrink-0" />
            {labelKey.startsWith('Portal') ? labelKey : t(lang, labelKey)}
          </NavLink>
        ))}

        <div className="pt-8 px-4 pb-4 border-t border-health-border/50">
          <p className="text-[10px] font-black text-health-secondary uppercase tracking-[0.2em] mb-4 pl-4 opacity-40">Dërgimet e Shpejta</p>
          <div className="space-y-1">
             {[
               { to: '/dashboard', label: '1', name: 'Dashboard', icon: LayoutDashboard },
               { to: '/klientet', label: '2', name: 'Klientët', icon: Users },
               { to: '/terapistet', label: '3', name: 'Terapistët', icon: UserCheck },
               { to: '/terminet', label: '4', name: 'Terminet', icon: Calendar },
               { to: '/sherbimet', label: '5', name: 'Shërbimet', icon: Stethoscope },
             ].map((s) => (
               <NavLink
                 key={s.label}
                 to={s.to}
                 className={({ isActive }) =>
                   `flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-medium transition-all duration-200 ${
                     isActive
                       ? 'bg-health-brand text-white shadow-lg shadow-health-brand/20'
                       : 'text-health-secondary hover:bg-health-hover hover:text-health-primary group'
                   }`
                 }
               >
                 <s.icon className="h-4 w-4 flex-shrink-0" />
                 <span>{s.name}</span>
               </NavLink>
             ))}
          </div>
        </div>
      </nav>

      {/* User */}
      <div className="px-4 py-6 border-t border-health-border">
        <Link to="/profile" className="flex items-center gap-3 px-3 py-2.5 mb-2 rounded-xl hover:bg-health-hover transition-all">
          <div className="h-10 w-10 rounded-full bg-health-accent/10 border border-health-accent/20 flex items-center justify-center flex-shrink-0">
            <span className="text-sm font-bold text-health-accent">
              {user?.firstName?.[0]?.toUpperCase() ?? user?.email?.[0]?.toUpperCase() ?? 'U'}
            </span>
          </div>
          <div className="min-w-0">
            <p className="text-sm font-semibold text-health-primary truncate">
              {user?.firstName && user?.lastName
                ? `${user.firstName} ${user.lastName}`
                : user?.email ?? 'Përdoruesi'}
            </p>
            {user?.email && (
              <p className="text-xs text-health-secondary truncate">{user.email}</p>
            )}
          </div>
        </Link>
        <button
          onClick={handleLogout}
          className="w-full flex items-center gap-3 px-4 py-2.5 rounded-xl text-sm font-medium text-health-brand/80 hover:bg-health-brand/10 hover:text-health-brand transition-all"
        >
          <LogOut className="h-4 w-4" />
          {t(lang, 'logout')}
        </button>
      </div>
    </div>
  )
}

export default function Layout() {
  const [sidebarOpen, setSidebarOpen] = useState(false)
  const [shortcutsOpen, setShortcutsOpen] = useState(false)
  const { user } = useAuthStore()
  const roles = Array.isArray(user?.roles) ? user.roles : []
  const displayRole = user?.role || roles[0] || 'Anetar'
  const { theme, toggleTheme } = useThemeStore()
  const { lang, toggleLang } = useLangStore()
  const isOnline = useOnlineStatus()
  useKeyboardShortcuts({ onShowHelp: () => setShortcutsOpen((v) => !v) })

  return (
    <div className="flex h-screen bg-health-bg overflow-hidden text-health-primary">
      {/* Desktop sidebar */}
      <aside className="hidden lg:flex flex-col w-64 flex-shrink-0 bg-health-surface border-r border-health-border shadow-xl">
        <SidebarContent lang={lang} />
      </aside>

      {/* Mobile overlay */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 z-40 bg-black/60 backdrop-blur-sm lg:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Mobile sidebar */}
      <aside
        className={`fixed inset-y-0 left-0 z-50 w-64 bg-health-surface border-r border-health-border lg:hidden transition-transform duration-300 ease-in-out ${
          sidebarOpen ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        <SidebarContent onClose={() => setSidebarOpen(false)} lang={lang} />
      </aside>

      {/* Main content */}
      <div className="flex-1 flex flex-col min-w-0 overflow-hidden">
        {/* Mobile header */}
        <div className="flex items-center gap-3 px-6 py-4 bg-health-surface border-b border-health-border lg:hidden">
          <button
            onClick={() => setSidebarOpen(true)}
            className="p-2 text-health-secondary hover:text-health-primary"
          >
            <Menu className="h-6 w-6" />
          </button>
          <div className="flex items-center gap-2">
            <Leaf className="h-5 w-5 text-health-brand" />
          </div>
          <div className="ml-auto flex items-center gap-4">
            <NotificationCenter />
            <div className="w-[1px] h-4 bg-health-border" />
            <button
              onClick={toggleLang}
              className="text-xs font-bold text-health-secondary hover:text-health-primary uppercase tracking-widest"
            >
              {lang === 'sq' ? 'EN' : 'SQ'}
            </button>
          </div>
        </div>

        {/* Desktop header toolbar */}
        <div className="hidden lg:flex items-center justify-between px-10 py-4 bg-health-surface/50 backdrop-blur-md border-b border-health-border sticky top-0 z-30">
          <div className="flex items-center gap-4">
            <h2 className="text-xl font-bold text-health-primary tracking-tight">
              Mirë se vini, <span className="text-health-brand">{user?.firstName}</span>
            </h2>
            <div className="px-3 py-1 bg-health-accent/10 border border-health-accent/20 rounded-lg">
              <span className="text-[10px] font-bold text-health-accent uppercase tracking-widest">
                {displayRole}
              </span>
            </div>
          </div>
          
          <div className="flex items-center gap-6">
            <div className="flex items-center gap-3">
              <NotificationCenter />
              
              {/* Theme Selector */}
              <div className="flex items-center bg-health-bg border border-health-border p-1 rounded-2xl">
                <button
                  onClick={() => theme !== 'light' && toggleTheme()}
                  className={`p-2 rounded-xl transition-all ${theme === 'light' ? 'bg-health-brand text-white shadow-lg' : 'text-health-secondary hover:text-health-primary'}`}
                  title="Açık Tema"
                >
                  <Sun className="h-4 w-4" />
                </button>
                <button
                  onClick={() => theme !== 'dark' && toggleTheme()}
                  className={`p-2 rounded-xl transition-all ${theme === 'dark' ? 'bg-health-brand text-white shadow-lg' : 'text-health-secondary hover:text-health-primary'}`}
                  title="Koyu Tema"
                >
                  <Moon className="h-4 w-4" />
                </button>
              </div>

              <button
                onClick={() => setShortcutsOpen(true)}
                className="p-2.5 text-health-secondary hover:text-health-primary hover:bg-health-hover rounded-xl transition-all border border-transparent hover:border-health-border ring-health-accent/20 active:ring-4"
                title="Shkurtesat?"
              >
                <Keyboard className="h-4 w-4" />
              </button>
            </div>
            
            <div className="w-[1px] h-6 bg-health-border" />
            
            <button
              onClick={toggleLang}
              className="px-4 py-2 text-xs font-bold text-health-secondary hover:text-health-primary hover:bg-health-bg border border-transparent hover:border-health-border rounded-xl transition-all uppercase tracking-widest"
            >
              {lang === 'sq' ? 'EN' : 'SQ'}
            </button>
          </div>
        </div>

        {!isOnline && <OfflineBanner />}
        <main className="flex-1 overflow-y-auto p-6"><Outlet /></main>
      </div>

      <KeyboardShortcutsModal open={shortcutsOpen} onClose={() => setShortcutsOpen(false)} />
      <ChatWidget />
    </div>
  )
}

