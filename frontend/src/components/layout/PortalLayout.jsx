import { createElement, useState } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import {
  LayoutDashboard, Calendar, CreditCard, ShoppingBag,
  Star, Activity, User, LogOut, Menu, Package
} from 'lucide-react'
import useAuthStore from '../../store/authStore'
import { authApi } from '../../api/api'
import ChatWidget from '../ui/ChatWidget'
import healthLogo from '../../assets/health-logo.png'

const NAV = [
  { label: 'Ballina', path: '/portal/dashboard', icon: LayoutDashboard },
  { label: 'Terminet e Mia', path: '/portal/terminet', icon: Calendar },
  { label: 'Rezervo Termin', path: '/portal/rezervo', icon: Calendar },
  { label: 'Anëtarësimi', path: '/portal/anetaresimi', icon: CreditCard },
  { label: 'Programet', path: '/portal/programet', icon: Activity },
  { label: 'Produktet', path: '/portal/produktet', icon: ShoppingBag },
  { label: 'Blerjet e Mia', path: '/portal/shitjet', icon: Package },
  { label: 'Vlerësimet', path: '/portal/vlereisimet', icon: Star },
  { label: 'Profili Im', path: '/portal/profili', icon: User },
]

export default function PortalLayout({ children }) {
  const [sidebarOpen, setSidebarOpen] = useState(false)
  const { user, clearAuth } = useAuthStore()
  const navigate = useNavigate()

  const handleLogout = async () => {
    try {
      await authApi.logout()
    } catch {
      // Logout should still clear local auth if the API is unreachable.
    }
    clearAuth()
    navigate('/login')
  }

  return (
    <div className="min-h-screen lg:h-screen lg:overflow-hidden flex text-health-primary">
      {sidebarOpen && (
        <div className="fixed inset-0 z-20 bg-black/50 backdrop-blur-md lg:hidden" onClick={() => setSidebarOpen(false)} />
      )}
      <aside
        className={`
        fixed top-0 left-0 h-screen z-30 w-64 glass-sidebar
        flex flex-col transition-transform duration-300
        ${sidebarOpen ? 'translate-x-0' : '-translate-x-full'}
        lg:translate-x-0 lg:static lg:h-auto lg:flex
      `}
      >
        <div className="flex items-center gap-3 px-5 py-5 border-b border-health-border">
          <div
            className="w-11 h-11 rounded-2xl flex items-center justify-center relative overflow-hidden"
            style={{
              background: 'var(--glass-tint-strong)',
              border: '1px solid var(--glass-border)',
              backdropFilter: 'blur(14px)',
              boxShadow: '0 8px 22px -6px rgba(15,23,42,0.18), inset 0 1px 0 rgba(255,255,255,0.55)',
            }}
          >
            <img src={healthLogo} alt="Health Center logo" className="w-full h-full object-contain p-1" />
          </div>
          <div>
            <p className="text-sm font-bold text-health-primary leading-tight">Health Center</p>
            <p className="text-[10px] text-health-secondary uppercase tracking-widest font-semibold">Portali i Klientit</p>
          </div>
        </div>

        <div className="px-4 py-4 border-b border-health-border">
          <div className="flex items-center gap-3">
            <div
              className="w-10 h-10 rounded-full flex items-center justify-center"
              style={{
                background: 'color-mix(in srgb, var(--health-accent) 18%, transparent)',
                border: '1px solid color-mix(in srgb, var(--health-accent) 35%, transparent)',
                backdropFilter: 'blur(8px)',
              }}
            >
              <span className="text-sm font-bold text-health-accent">
                {user?.firstName?.[0]}
                {user?.lastName?.[0]}
              </span>
            </div>
            <div>
              <p className="text-sm font-semibold text-health-primary">
                {user?.firstName} {user?.lastName}
              </p>
              <p className="text-xs text-health-secondary">Klient</p>
            </div>
          </div>
        </div>

        <nav className="flex-1 overflow-y-auto py-3 px-2">
          {NAV.map(({ label, path, icon: Icon }) => (
            <NavLink
              key={path}
              to={path}
              onClick={() => setSidebarOpen(false)}
              className={({ isActive }) =>
                `flex items-center gap-3 px-4 py-2.5 rounded-full text-sm font-semibold mb-1 transition-all duration-300 relative overflow-hidden ${
                  isActive
                    ? 'text-white nav-link-active-glass'
                    : 'text-health-secondary hover:bg-health-hover hover:text-health-primary'
                }`
              }
            >
              {createElement(Icon, { className: 'w-4 h-4 flex-shrink-0' })}
              {label}
            </NavLink>
          ))}
        </nav>

        <div className="p-3 border-t border-health-border">
          <button
            onClick={handleLogout}
            className="w-full flex items-center gap-3 px-4 py-2.5 rounded-full text-sm font-medium text-health-secondary hover:text-health-brand hover:bg-health-hover transition-colors"
          >
            <LogOut className="w-4 h-4" />
            Dilni
          </button>
        </div>
      </aside>

      <div className="flex-1 flex flex-col min-w-0">
        <header className="lg:hidden flex items-center gap-3 px-4 py-3 glass-bar">
          <button onClick={() => setSidebarOpen(true)} className="btn-ghost !p-2">
            <Menu className="w-5 h-5" />
          </button>
          <span className="text-sm font-bold text-health-primary">Wellness House</span>
        </header>
        <main className="flex-1 p-6 overflow-auto max-w-5xl mx-auto w-full">{children}</main>
      </div>

      <ChatWidget />
    </div>
  )
}
