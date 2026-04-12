import { useNavigate } from 'react-router-dom'
import { Leaf, Home } from 'lucide-react'

export default function NotFoundPage() {
  const navigate = useNavigate()
  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div className="text-center">
        <div className="w-20 h-20 rounded-3xl bg-green-100 flex items-center justify-center mx-auto mb-6">
          <Leaf className="w-10 h-10 text-green-600" />
        </div>
        <h1 className="text-7xl font-bold text-gray-200 mb-2">404</h1>
        <h2 className="text-xl font-bold text-gray-900 mb-2">
          Faqja nuk u gjet
        </h2>
        <p className="text-sm text-gray-500 mb-8 max-w-xs mx-auto">
          Faqja që po kërkoni nuk ekziston ose është zhvendosur.
        </p>
        <button onClick={() => navigate('/dashboard')} className="btn-primary">
          <Home className="w-4 h-4" />
          Kthehu në Dashboard
        </button>
      </div>
    </div>
  )
}
