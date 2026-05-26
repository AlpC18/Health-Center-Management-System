// Export helpers — CSV / Excel / PDF
// Pure functions: take rows + columns, return nothing (trigger download).
// Columns: [{ key, label, render? }]

import * as XLSX from 'xlsx'
import jsPDF from 'jspdf'
import autoTable from 'jspdf-autotable'

// Convert a single row to a flat object suitable for export.
// If a column has `render`, we still prefer the raw value (since render
// may return JSX) — fall back to JSON stringify for objects.
function rowToPlainObject(item, columns) {
  const out = {}
  for (const col of columns) {
    let val = item[col.key]
    if (val === null || val === undefined) val = ''
    else if (typeof val === 'object') val = JSON.stringify(val)
    out[col.label] = val
  }
  return out
}

function sanitizeFilename(name) {
  return String(name || 'export').replace(/[^a-zA-Z0-9_\-]+/g, '_')
}

function downloadBlob(blob, filename) {
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}

export function exportCSV(rows, columns, title) {
  if (!rows?.length) return false
  const header = columns.map((c) => `"${String(c.label).replace(/"/g, '""')}"`).join(',')
  const lines = rows.map((item) =>
    columns
      .map((col) => {
        let val = item[col.key]
        if (val === null || val === undefined) val = ''
        else if (typeof val === 'object') val = JSON.stringify(val)
        const strVal = String(val).replace(/"/g, '""')
        return `"${strVal}"`
      })
      .join(',')
  )
  // Prepend BOM so Excel opens UTF-8 (Albanian letters) correctly
  const csv = '﻿' + header + '\n' + lines.join('\n')
  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' })
  downloadBlob(blob, `${sanitizeFilename(title)}_${new Date().toISOString().slice(0, 10)}.csv`)
  return true
}

export function exportExcel(rows, columns, title) {
  if (!rows?.length) return false
  const plain = rows.map((r) => rowToPlainObject(r, columns))
  const ws = XLSX.utils.json_to_sheet(plain, { header: columns.map((c) => c.label) })

  // Auto-size columns based on max content length (cap at 50)
  const colWidths = columns.map((c) => {
    const headerLen = String(c.label).length
    const maxDataLen = rows.reduce((max, item) => {
      const v = item[c.key]
      const len = v === null || v === undefined ? 0 : String(v).length
      return Math.max(max, len)
    }, 0)
    return { wch: Math.min(50, Math.max(headerLen, maxDataLen) + 2) }
  })
  ws['!cols'] = colWidths

  const wb = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(wb, ws, sanitizeFilename(title).slice(0, 31))
  XLSX.writeFile(wb, `${sanitizeFilename(title)}_${new Date().toISOString().slice(0, 10)}.xlsx`)
  return true
}

export function exportPDF(rows, columns, title) {
  if (!rows?.length) return false
  // Landscape A4 fits more columns
  const doc = new jsPDF({ orientation: 'landscape', unit: 'pt', format: 'a4' })

  doc.setFontSize(14)
  doc.text(String(title || 'Export'), 40, 36)
  doc.setFontSize(9)
  doc.setTextColor(120)
  doc.text(`Generated: ${new Date().toLocaleString()}`, 40, 52)
  doc.setTextColor(0)

  const head = [columns.map((c) => c.label)]
  const body = rows.map((item) =>
    columns.map((col) => {
      let val = item[col.key]
      if (val === null || val === undefined) return '-'
      if (typeof val === 'object') return JSON.stringify(val)
      return String(val)
    })
  )

  autoTable(doc, {
    head,
    body,
    startY: 68,
    styles: { fontSize: 8, cellPadding: 4, overflow: 'linebreak' },
    headStyles: { fillColor: [22, 163, 74], textColor: 255, fontStyle: 'bold' },
    alternateRowStyles: { fillColor: [245, 247, 250] },
    margin: { left: 30, right: 30 },
  })

  doc.save(`${sanitizeFilename(title)}_${new Date().toISOString().slice(0, 10)}.pdf`)
  return true
}
