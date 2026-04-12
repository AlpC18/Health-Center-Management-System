import { X } from 'lucide-react'

const shortcuts = [
  { keys: ['1'], desc: 'Shko te Dashboard' },
  { keys: ['2'], desc: 'Shko te Klientët' },
  { keys: ['3'], desc: 'Shko te Terapistët' },
  { keys: ['4'], desc: 'Shko te Terminet' },
  { keys: ['5'], desc: 'Shko te Shërbimet' },
  { keys: ['/'], desc: 'Fokusohu te Kërkimi' },
  { keys: ['?'], desc: 'Shfaq/Fshih Shkurtesat' },
]

export default function KeyboardShortcutsModal({ open, onClose }) {
  if (!open) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40" onClick={onClose}>
      <div
        className="bg-white dark:bg-gray-900 rounded-xl shadow-xl w-full max-w-sm mx-4 p-6"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-base font-semibold text-gray-900 dark:text-white">Shkurtesat e Tastierës</h2>
          <button
            onClick={onClose}
            className="p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 rounded-lg"
          >
            <X className="h-4 w-4" />
          </button>
        </div>

        <ul className="space-y-2">
          {shortcuts.map(({ keys, desc }) => (
            <li key={keys.join('+')} className="flex items-center justify-between">
              <span className="text-sm text-gray-600 dark:text-gray-400">{desc}</span>
              <div className="flex gap-1">
                {keys.map((k) => (
                  <kbd
                    key={k}
                    className="inline-flex items-center justify-center px-2 py-0.5 text-xs font-mono font-medium bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 border border-gray-300 dark:border-gray-600 rounded"
                  >
                    {k}
                  </kbd>
                ))}
              </div>
            </li>
          ))}
        </ul>
      </div>
    </div>
  )
}
