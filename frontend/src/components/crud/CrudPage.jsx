import { useState, useEffect } from 'react'
import toast from 'react-hot-toast'
import { Plus, Search, Pencil, Trash2, ChevronUp, ChevronDown, ChevronsUpDown } from 'lucide-react'
import { Spinner, Modal, EmptyState, notify } from '../ui/index'
import TableSkeleton from '../ui/TableSkeleton'
import useLangStore from '../../store/langStore'
import { t } from '../../i18n'

const LIMIT = 10

export default function CrudPage({
  title,
  subtitle,
  columns,
  api,
  FormComponent,
  emptyIcon,
  searchKeys = [],
  idKey = 'id',
  extraFormProps = {},
  onSaved,
}) {
  const { lang } = useLangStore()
  const [items, setItems] = useState([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [page, setPage] = useState(1)
  const [sortBy, setSortBy] = useState('')
  const [sortDir, setSortDir] = useState('asc')
  const [total, setTotal] = useState(0)
  const [refreshKey, setRefreshKey] = useState(0)

  const [modalOpen, setModalOpen] = useState(false)
  const [editItem, setEditItem] = useState(null)
  const [formLoading, setFormLoading] = useState(false)


  // Debounce search input
  useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearch(search), 500)
    return () => clearTimeout(timer)
  }, [search])

  // Reset to page 1 when debounced search changes
  useEffect(() => {
    setPage(1)
  }, [debouncedSearch])

  // Main fetch — fires on page, search, sort, or manual reload
  useEffect(() => {
    let cancelled = false
    setLoading(true)
    const params = new URLSearchParams({ page, limit: LIMIT })
    if (debouncedSearch) params.set('search', debouncedSearch)
    if (sortBy) { params.set('sortBy', sortBy); params.set('sortDir', sortDir) }

    api.getAll(params.toString())
      .then(({ data }) => {
        if (cancelled) return
        if (data?.data !== undefined) {
          setItems(data.data)
          setTotal(data.total)
        } else {
          const arr = Array.isArray(data) ? data : []
          setItems(arr)
          setTotal(arr.length)
        }
      })
      .catch(() => { if (!cancelled) toast.error(t(lang, 'loadError')) })
      .finally(() => { if (!cancelled) setLoading(false) })

    return () => { cancelled = true }
  }, [page, debouncedSearch, sortBy, sortDir, api, lang, refreshKey])

  const reload = () => setRefreshKey((k) => k + 1)

  const openCreate = () => { setEditItem(null); setModalOpen(true) }
  const openEdit = (item) => { setEditItem(item); setModalOpen(true) }
  const closeModal = () => { setModalOpen(false); setEditItem(null) }

  const handleSave = async (data) => {
    setFormLoading(true)
    const toastId = notify.loading(editItem ? 'Duke përditësuar...' : 'Duke krijuar...')
    try {
      let savedItem
      if (editItem) {
        const res = await api.update(editItem[idKey], data)
        savedItem = res.data
      } else {
        const res = await api.create(data)
        savedItem = res.data
      }
      
      if (typeof onSaved === 'function') {
        await onSaved(savedItem, data)
      }

      toast.dismiss(toastId)
      notify.success(editItem ? 'U përditësua me sukses!' : 'U krijua me sukses!')
      closeModal()
      reload()
    } catch (err) {
      toast.dismiss(toastId)
      notify.error(err?.response?.data?.message || t(lang, 'saveError'))
    } finally {
      setFormLoading(false)
    }
  }

  const handleDelete = async (item) => {
    setItems((prev) => prev.filter((i) => i[idKey] !== item[idKey]))
    let undone = false
    notify.undo(`"${getItemName(item)}" u fshi.`, () => {
      undone = true
      setItems((prev) => [...prev, item].sort((a, b) => a[idKey] - b[idKey]))
    })
    await new Promise((r) => setTimeout(r, 4200))
    if (!undone) {
      try {
        await api.delete(item[idKey])
      } catch {
        notify.error('Fshirja dështoi.')
        setItems((prev) => [...prev, item].sort((a, b) => a[idKey] - b[idKey]))
      }
    }
  }

  const getItemName = (item) =>
    (item.emri && item.mbiemri ? `${item.emri} ${item.mbiemri}` : null) ??
    item.emriSherbimit ??
    item.emriPaketes ??
    item.emriProgramit ??
    item.emriProduktit ??
    item.emri ??
    `#${item[idKey]}`

  const totalPages = Math.ceil(total / LIMIT) || 1

  // Sortable column header (closure over sortBy/sortDir state)
  const SortHeader = ({ col }) => (
    <th
      className="table-th cursor-pointer hover:bg-health-hover select-none transition-colors"
      onClick={() => {
        if (sortBy === col.key) {
          setSortDir((d) => (d === 'asc' ? 'desc' : 'asc'))
        } else {
          setSortBy(col.key)
          setSortDir('asc')
        }
        setPage(1)
      }}
    >
      <div className="flex items-center gap-1.5">
        {col.label}
        {sortBy === col.key ? (
          sortDir === 'asc'
            ? <ChevronUp className="w-3.5 h-3.5 text-health-accent" />
            : <ChevronDown className="w-3.5 h-3.5 text-health-accent" />
        ) : (
          <ChevronsUpDown className="w-3.5 h-3.5 text-health-secondary/30" />
        )}
      </div>
    </th>
  )

  if (loading) return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div className="h-8 w-64 bg-health-surface border border-health-border rounded animate-pulse" />
        <div className="h-10 w-32 bg-health-surface border border-health-border rounded-xl animate-pulse" />
      </div>
      <div className="card overflow-hidden">
        <TableSkeleton rows={7} cols={columns.length} />
      </div>
    </div>
  )

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-health-primary tracking-tight">{title}</h1>
          {subtitle && <p className="text-sm text-health-secondary mt-1">{subtitle}</p>}
        </div>
        <button className="btn-primary flex-shrink-0 px-6 py-2.5 shadow-lg shadow-health-brand/20" onClick={openCreate}>
          <Plus className="h-4 w-4" />
          {t(lang, 'add')}
        </button>
      </div>

      {/* Search */}
      {searchKeys.length > 0 && (
        <div className="relative max-w-xs">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400 pointer-events-none" />
          <input
            type="text"
            placeholder={t(lang, 'search')}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="input pl-9"
          />
        </div>
      )}

      {/* Table */}
      <div className="card overflow-hidden">
        {items.length === 0 ? (
          <EmptyState
            icon={emptyIcon}
            title={t(lang, 'noRecords')}
            description={debouncedSearch ? t(lang, 'changeSearch') : t(lang, 'addData')}
            action={
              !debouncedSearch && (
                <button className="btn-primary" onClick={openCreate}>
                  <Plus className="h-4 w-4" />
                  {t(lang, 'add')}
                </button>
              )
            }
          />
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-health-bg border-b border-health-border">
                  <tr>
                    {columns.map((col) => (
                      <SortHeader key={col.key} col={col} />
                    ))}
                    <th className="table-th text-right">{t(lang, 'actions')}</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-health-border/50">
                  {items.map((item) => (
                    <tr key={item[idKey]} className="hover:bg-health-hover/50 transition-colors">
                      {columns.map((col) => (
                        <td key={col.key} className="table-td">
                          {col.render ? col.render(item) : (item[col.key] ?? '-')}
                        </td>
                      ))}
                      <td className="table-td text-right">
                        <div className="flex items-center justify-end gap-1">
                          <button
                            onClick={() => openEdit(item)}
                            className="p-1.5 text-gray-400 hover:text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-900/20 rounded-lg transition-colors"
                            title="Ndrysho"
                          >
                            <Pencil className="h-3.5 w-3.5" />
                          </button>
                          <button
                            onClick={() => handleDelete(item)}
                            className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition-colors"
                            title="Fshi"
                          >
                            <Trash2 className="h-3.5 w-3.5" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Pagination */}
            <div className="flex items-center justify-between px-6 py-4 border-t border-health-border bg-health-surface/50">
              <p className="text-xs font-bold text-health-secondary uppercase tracking-widest">
                {(page - 1) * LIMIT + 1}–{Math.min(page * LIMIT, total)} / {total} {lang === 'sq' ? 'rekorde' : 'records'}
              </p>
              <div className="flex items-center gap-3">
                <button
                  onClick={() => setPage((p) => p - 1)}
                  disabled={page === 1}
                  className="btn-secondary px-4 py-2 text-xs font-bold disabled:opacity-20 translate-y-0 active:scale-95 transition-all"
                >
                  ← {t(lang, 'prev')}
                </button>
                <div className="flex items-center justify-center w-12 h-8 rounded-lg bg-health-bg border border-health-border">
                  <span className="text-xs text-health-primary font-bold">
                    {page}
                  </span>
                </div>
                <button
                  onClick={() => setPage((p) => p + 1)}
                  disabled={page >= totalPages}
                  className="btn-secondary px-4 py-2 text-xs font-bold disabled:opacity-20 translate-y-0 active:scale-95 transition-all"
                >
                  {t(lang, 'next')} →
                </button>
              </div>
            </div>
          </>
        )}
      </div>

      {/* Create/Edit Modal */}
      <Modal
        isOpen={modalOpen}
        onClose={closeModal}
        title={editItem ? t(lang, 'editRecord') : `${t(lang, 'add')} ${title.toLowerCase()}`}
        size="lg"
      >
        <FormComponent
          initial={editItem}
          onSave={handleSave}
          loading={formLoading}
          onCancel={closeModal}
          {...extraFormProps}
        />
      </Modal>

    </div>
  )
}
