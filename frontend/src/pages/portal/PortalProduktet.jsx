import { useEffect, useState } from 'react'
import { ShoppingBag, ShoppingCart, Plus, Minus } from 'lucide-react'
import { portalApi } from '../../api/portalApi'
import { PageLoader, Spinner } from '../../components/ui'
import toast from 'react-hot-toast'

export default function PortalProduktet() {
  const [produktet, setProduktet] = useState([])
  const [loading, setLoading] = useState(true)
  const [cart, setCart] = useState({})
  const [buying, setBuying] = useState({})

  useEffect(() => {
    portalApi.getProduktet()
      .then((r) => setProduktet(r.data?.data || r.data || []))
      .finally(() => setLoading(false))
  }, [])

  const handleBuy = async (produkt) => {
    const sasia = cart[produkt.produktId] || 1
    setBuying((p) => ({ ...p, [produkt.produktId]: true }))
    try {
      await portalApi.blejProdukt({
        produktId: produkt.produktId,
        sasia,
        cmimiTotal: produkt.cmimi * sasia,
        klientId: 0,
      })
      toast.success(`${produkt.emriProduktit} u ble!`)
      setCart((p) => ({ ...p, [produkt.produktId]: 1 }))
      setProduktet((prev) =>
        prev.map((p) =>
          p.produktId === produkt.produktId ? { ...p, sasiaStok: p.sasiaStok - sasia } : p
        )
      )
    } catch (err) {
      toast.error(err.response?.data?.message || 'Blerja dështoi.')
    } finally {
      setBuying((p) => ({ ...p, [produkt.produktId]: false }))
    }
  }

  if (loading) return <PageLoader />

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-2">Produktet</h1>
      <p className="text-sm text-gray-500 mb-6">Produktet e disponueshme të qendrës wellness</p>
      {produktet.length === 0 ? (
        <div className="card p-8 text-center">
          <ShoppingBag className="w-10 h-10 text-gray-300 mx-auto mb-3" />
          <p className="text-sm text-gray-500">Nuk ka produkte të disponueshme.</p>
        </div>
      ) : (
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-4">
          {produktet.map((p) => (
            <div key={p.produktId} className="card p-5 flex flex-col">
              <div className="w-12 h-12 rounded-xl bg-green-50 flex items-center justify-center mb-3">
                <ShoppingBag className="w-6 h-6 text-green-600" />
              </div>
              <h3 className="font-bold text-gray-900 mb-1">{p.emriProduktit}</h3>
              <p className="text-xs text-gray-500 mb-2">{p.kategoria}</p>
              {p.pershkrimi && <p className="text-sm text-gray-600 mb-3 flex-1">{p.pershkrimi}</p>}
              <div className="flex items-center justify-between mb-3">
                <span className="text-xl font-bold text-green-700">€{p.cmimi}</span>
                <span className="text-xs text-gray-400">{p.sasiaStok} në stok</span>
              </div>
              <div className="flex items-center gap-2 mb-3">
                <button
                  onClick={() => setCart((prev) => ({ ...prev, [p.produktId]: Math.max(1, (prev[p.produktId] || 1) - 1) }))}
                  className="w-8 h-8 rounded-lg bg-gray-100 flex items-center justify-center hover:bg-gray-200"
                >
                  <Minus className="w-3 h-3" />
                </button>
                <span className="flex-1 text-center text-sm font-medium">{cart[p.produktId] || 1}</span>
                <button
                  onClick={() => setCart((prev) => ({ ...prev, [p.produktId]: Math.min(p.sasiaStok, (prev[p.produktId] || 1) + 1) }))}
                  className="w-8 h-8 rounded-lg bg-gray-100 flex items-center justify-center hover:bg-gray-200"
                >
                  <Plus className="w-3 h-3" />
                </button>
              </div>
              <button onClick={() => handleBuy(p)} disabled={buying[p.produktId] || p.sasiaStok === 0} className="btn-primary w-full justify-center">
                {buying[p.produktId] ? <Spinner size="sm" /> : <ShoppingCart className="w-4 h-4" />}
                Bli • €{(p.cmimi * (cart[p.produktId] || 1)).toFixed(2)}
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
