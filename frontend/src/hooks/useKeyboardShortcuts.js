import { useEffect } from 'react'
import { useNavigate } from 'react-router-dom'

const PAGE_SHORTCUTS = {
  '1': '/dashboard',
  '2': '/klientet',
  '3': '/terapistet',
  '4': '/terminet',
  '5': '/sherbimet',
}

export default function useKeyboardShortcuts({ onShowHelp } = {}) {
  const navigate = useNavigate()

  useEffect(() => {
    const handler = (e) => {
      // Ignore when typing in inputs/textareas/selects
      const tag = e.target.tagName
      if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT' || e.target.isContentEditable) {
        return
      }

      // Ctrl/Cmd combos — skip
      if (e.ctrlKey || e.metaKey || e.altKey) return

      if (PAGE_SHORTCUTS[e.key]) {
        e.preventDefault()
        navigate(PAGE_SHORTCUTS[e.key])
        return
      }

      if (e.key === '/') {
        e.preventDefault()
        const searchInput = document.querySelector('input[type="search"], input[placeholder*="Kërko"], input[placeholder*="Search"]')
        searchInput?.focus()
        return
      }

      if (e.key === '?') {
        e.preventDefault()
        onShowHelp?.()
      }
    }

    window.addEventListener('keydown', handler)
    return () => window.removeEventListener('keydown', handler)
  }, [navigate, onShowHelp])
}
