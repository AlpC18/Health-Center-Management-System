import { createElement, useState, useEffect, useRef } from 'react'
import toast from 'react-hot-toast'
import {
  Plus, Search, Pencil, Trash2, ChevronUp, ChevronDown, ChevronsUpDown, Download,
  FileText, FileSpreadsheet, FileType,
} from 'lucide-react'
import { Spinner, Modal, EmptyState, notify } from '../ui/index'
import TableSkeleton from '../ui/TableSkeleton'
import useLangStore from '../../store/langStore'
import { t } from '../../i18n'
import { exportCSV, exportExcel, exportPDF } from '../../utils/export'

const PAGE_SIZES = [10, 25, 50, 100]

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
  filterFn,
}) {
  const { lang } = useLangStore()
  const [items, setItems] = useState([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [sortBy, setSortBy] = useState('')
  const [sortDir, setSortDir] = useState('asc')
  const [total, setTotal] = useState(0)
  const [refreshKey, setRefreshKey] = useState(0)

  const [modalOpen, setModalOpen] = useState(false)
  const [editItem, setEditItem] = useState(null)
  const [formLoading, setFormLoading] = useState(false)

  const [exportMenuOpen, setExportMenuOpen] = useState(false)
  const exportMenuRef = useRef(null)


  // Debounce search input
  useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearch(search), 500)
    return () => clearTimeout(timer)
  }, [search])

  // Reset to page 1 when search or page-size changes
  useEffect(() => {
    setPage(1)
  }, [debouncedSearch, pageSize])

  // Close export menu when clicking outside
  useEffect(() => {
    if (!exportMenuOpen) return
    const onClick = (e) => {
      if (exportMenuRef.current && !exportMenuRef.current.contains(e.target)) {
        setExportMenuOpen(false)
      }
    }
    document.addEventListener('mousedown', onClick)
    return () => document.removeEventListener('mousedown', onClick)
  }, [exportMenuOpen])

  // Main fetch — fires on page, search, sort, page-size, or manual reload
  useEffect(() => {
    let cancelled = false
    setLoading(true)
    const params = new URLSearchParams({ page, limit: pageSize })
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
  }, [page, pageSize, debouncedSearch, sortBy, sortDir, api, lang, refreshKey])

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
      } catch (err) {
        notify.error(err?.response?.data?.message || 'Fshirja dështoi.')
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

  const totalPages = Math.ceil(total / pageSize) || 1
  const visibleItems = typeof filterFn === 'function' ? items.filter(filterFn) : items
  const shownFrom = visibleItems.length > 0 ? (page - 1) * pageSize + 1 : 0
  const shownTo = visibleItems.length > 0 ? (page - 1) * pageSize + visibleItems.length : 0

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

  // Export handlers (route through the shared util)
  const doExport = (fn, fmt) => {
    setExportMenuOpen(false)
    if (visibleItems.length === 0) {
      toast.error(t(lang, 'noDataToExport'))
      return
    }
    const ok = fn(visibleItems, columns, title)
    if (ok) toast.success(`${t(lang, 'exportSuccess')} (${fmt})`)
  }

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
        <div className="flex items-center gap-3">
          {/* Export dropdown */}
          <div className="relative" ref={exportMenuRef}>
            <button
              onClick={() => setExportMenuOpen((v) => !v)}
              className="btn-secondary flex items-center gap-2 flex-shrink-0 px-4 py-2 bg-health-surface border border-health-border rounded-lg text-sm font-semibold hover:bg-health-hover transition-colors"
            >
              <Download className="h-4 w-4" />
              {t(lang, 'exportMenu')}
              <ChevronDown className={`h-3 w-3 transition-transform ${exportMenuOpen ? 'rotate-180' : ''}`} />
            </button>
            {exportMenuOpen && (
              <div className="absolute right-0 mt-2 w-44 z-30 bg-health-surface border border-health-border rounded-xl shadow-lg overflow-hidden">
                <button
                  type="button"
                  onClick={() => doExport(exportCSV, 'CSV')}
                  className="w-full flex items-center gap-2 px-4 py-2.5 text-sm font-medium text-health-primary hover:bg-health-hover text-left transition-colors"
                >
                  <FileText className="h-4 w-4 text-health-secondary" />
                  {t(lang, 'exportCsv')}
                </button>
                <button
                  type="button"
                  onClick={() => doExport(exportExcel, 'XLSX')}
                  className="w-full flex items-center gap-2 px-4 py-2.5 text-sm font-medium text-health-primary hover:bg-health-hover text-left transition-colors border-t border-health-border"
                >
                  <FileSpreadsheet className="h-4 w-4 text-green-600" />
                  {t(lang, 'exportExcel')}
                </button>
                <button
                  type="button"
                  onClick={() => doExport(exportPDF, 'PDF')}
                  className="w-full flex items-center gap-2 px-4 py-2.5 text-sm font-medium text-health-primary hover:bg-health-hover text-left transition-colors border-t border-health-border"
                >
                  <FileType className="h-4 w-4 text-red-500" />
                  {t(lang, 'exportPdf')}
                </button>
              </div>
            )}
          </div>

          <button className="btn-primary flex-shrink-0 px-6 py-2.5 shadow-lg shadow-health-brand/20" onClick={openCreate}>
            <Plus className="h-4 w-4" />
            {t(lang, 'add')}
          </button>
        </div>
      </div>

      {/* Toolbar: search + page-size */}
      <div className="flex items-center justify-between gap-4 flex-wrap">
        {searchKeys.length > 0 ? (
          <div className="relative max-w-xs flex-1 min-w-[180px]">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400 pointer-events-none" />
            <input
              type="text"
              placeholder={t(lang, 'search')}
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="input pl-9"
            />
          </div>
        ) : <div />}

        <label className="flex items-center gap-2 text-xs font-bold text-health-secondary uppercase tracking-wider">
          <select
            value={pageSize}
            onChange={(e) => setPageSize(Number(e.target.value))}
            className="bg-health-surface border border-health-border rounded-lg px-3 py-2 text-sm font-semibold text-health-primary focus:outline-none focus:ring-2 focus:ring-health-brand/30 transition-all"
          >
            {PAGE_SIZES.map((n) => (
              <option key={n} value={n}>{n}</option>
            ))}
          </select>
          <span>{t(lang, 'perPage')}</span>
        </label>
      </div>

      {/* Table */}
      <div className="card overflow-hidden">
        {visibleItems.length === 0 ? (
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
                  {visibleItems.map((item) => (
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
                {shownFrom}–{shownTo} / {total} {lang === 'sq' ? 'rekorde' : 'records'}
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
                    {page} / {totalPages}
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
        {createElement(FormComponent, {
          initial: editItem,
          onSave: handleSave,
          loading: formLoading,
          onCancel: closeModal,
          ...extraFormProps,
        })}
      </Modal>

    </div>
  )
}
