import { useState } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import {
  LayoutDashboard, Calendar, CreditCard, ShoppingBag,
  Star, Activity, User, LogOut, Menu, Leaf, Package
} from 'lucide-react'
import useAuthStore from '../../store/authStore'
import { authApi } from '../../api/api'
import ChatWidget from '../ui/ChatWidget'

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
  const { user, refreshToken, clearAuth } = useAuthStore()
  const navigate = useNavigate()

  const handleLogout = async () => {
    try {
      await authApi.logout(refreshToken)
    } catch (_) {}
    clearAuth()
    navigate('/login')
  }

  return (
    <div className="min-h-screen flex bg-gradient-to-br from-green-50 to-white">
      {sidebarOpen && (
        <div className="fixed inset-0 z-20 bg-black/40 lg:hidden" onClick={() => setSidebarOpen(false)} />
      )}
      <aside
        className={`
        fixed top-0 left-0 h-full z-30 w-64 bg-white border-r border-green-100
        flex flex-col transition-transform duration-300 shadow-lg
        ${sidebarOpen ? 'translate-x-0' : '-translate-x-full'}
        lg:translate-x-0 lg:static lg:flex
      `}
      >
        <div className="flex items-center gap-3 px-5 py-5 bg-gradient-to-r from-green-600 to-emerald-500">
          <div className="w-9 h-9 rounded-xl bg-white/20 flex items-center justify-center">
            <Leaf className="w-5 h-5 text-white" />
          </div>
          <div>
            <p className="text-sm font-bold text-white">Wellness House</p>
            <p className="text-xs text-green-100">Portali i Klientit</p>
          </div>
        </div>

        <div className="px-4 py-4 border-b border-green-50 bg-green-50/50">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-full bg-green-200 flex items-center justify-center">
              <span className="text-sm font-bold text-green-700">
                {user?.firstName?.[0]}
                {user?.lastName?.[0]}
              </span>
            </div>
            <div>
              <p className="text-sm font-semibold text-gray-900">
                {user?.firstName} {user?.lastName}
              </p>
              <p className="text-xs text-gray-400">Klient</p>
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
                `flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm
                font-medium mb-1 transition-all ${
                  isActive
                    ? 'bg-green-600 text-white shadow-sm'
                    : 'text-gray-600 hover:bg-green-50 hover:text-green-700'
                }`
              }
            >
              <Icon className="w-4 h-4 flex-shrink-0" />
              {label}
            </NavLink>
          ))}
        </nav>

        <div className="p-3 border-t border-green-50">
          <button
            onClick={handleLogout}
            className="w-full flex items-center gap-3 px-3 py-2 text-sm
            text-gray-500 hover:text-red-600 hover:bg-red-50 rounded-xl transition-colors"
          >
            <LogOut className="w-4 h-4" />
            Dilni
          </button>
        </div>
      </aside>

      <div className="flex-1 flex flex-col min-w-0">
        <header className="lg:hidden flex items-center gap-3 px-4 py-3 bg-white border-b border-green-100 shadow-sm">
          <button onClick={() => setSidebarOpen(true)} className="p-2 rounded-lg hover:bg-gray-100">
            <Menu className="w-5 h-5" />
          </button>
          <span className="text-sm font-bold text-gray-900">Wellness House</span>
        </header>
        <main className="flex-1 p-6 overflow-auto max-w-5xl mx-auto w-full">{children}</main>
      </div>

      <ChatWidget />
    </div>
  )
}
