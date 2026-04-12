import { WifiOff } from 'lucide-react'

export default function OfflineBanner() {
  return (
    <div className="flex items-center gap-2 px-4 py-2 bg-yellow-50 dark:bg-yellow-900/30 border-b border-yellow-200 dark:border-yellow-700 text-yellow-800 dark:text-yellow-300 text-sm">
      <WifiOff className="h-4 w-4 flex-shrink-0" />
      <span>Nuk ka lidhje interneti. Disa funksione mund të mos funksionojnë.</span>
    </div>
  )
}
