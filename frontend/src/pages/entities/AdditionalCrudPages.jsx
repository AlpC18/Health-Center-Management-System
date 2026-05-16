import { useState, useEffect } from 'react'
import { Building2, Truck, Megaphone, Tag, CalendarOff } from 'lucide-react'
import CrudPage from '../../components/crud/CrudPage'
import { Field, StatusBadge } from '../../components/ui/index'
import {
  sallatApi,
  furnizuesitApi,
  lajmerimetApi,
  zbritjetApi,
  pushimetApi,
  terapistetApi,
} from '../../api/api'

// ── Helpers shared by all forms ────────────────────────────────────────────

const emptyStr = (v) => (v === undefined || v === null ? '' : v)
const toDateInputValue = (value) => {
  if (!value) return ''
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return ''
  return d.toISOString().slice(0, 10)
}

function FormActions({ loading, onCancel }) {
  return (
    <div className="flex gap-3 justify-end pt-4 mt-4 border-t border-gray-100 dark:border-gray-700">
      <button type="button" className="btn-secondary" onClick={onCancel} disabled={loading}>
        Anulo
      </button>
      <button type="submit" className="btn-primary" disabled={loading}>
        Ruaj
      </button>
    </div>
  )
}

// ════════════════════════════════════════════════════════════════════════════
// 1. SALLAT
// ════════════════════════════════════════════════════════════════════════════

function SallaForm({ initial, onSave, loading, onCancel }) {
  const [f, setF] = useState({
    emri: '',
    kapaciteti: 1,
    tipi: '',
    pershkrimi: '',
    aktive: true,
    ...initial,
  })
  const set = (k) => (e) => setF((p) => ({ ...p, [k]: e.target.value }))
  const submit = (e) => {
    e.preventDefault()
    onSave({
      emri: f.emri?.trim(),
      kapaciteti: Number(f.kapaciteti) || 0,
      tipi: f.tipi || null,
      pershkrimi: f.pershkrimi?.trim() || null,
      aktive: !!f.aktive,
    })
  }
  return (
    <form onSubmit={submit} className="space-y-4">
      <Field label="Emri i Sallës" required>
        <input className="input" value={emptyStr(f.emri)} onChange={set('emri')} required />
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field label="Kapaciteti" required>
          <input
            className="input"
            type="number"
            min="1"
            value={emptyStr(f.kapaciteti)}
            onChange={set('kapaciteti')}
            required
          />
        </Field>
        <Field label="Tipi">
          <select className="input" value={emptyStr(f.tipi)} onChange={set('tipi')}>
            <option value="">-- Zgjidh --</option>
            <option value="Masazh">Masazh</option>
            <option value="Yoga">Yoga</option>
            <option value="Fizioterapi">Fizioterapi</option>
            <option value="Spa">Spa</option>
            <option value="Konsultë">Konsultë</option>
            <option value="Tjetër">Tjetër</option>
          </select>
        </Field>
      </div>
      <Field label="Përshkrimi">
        <textarea className="input" rows={3} value={emptyStr(f.pershkrimi)} onChange={set('pershkrimi')} />
      </Field>
      <Field label="Aktive">
        <label className="inline-flex items-center gap-2">
          <input type="checkbox" checked={!!f.aktive} onChange={(e) => setF((p) => ({ ...p, aktive: e.target.checked }))} />
          <span className="text-sm">Salla është në përdorim</span>
        </label>
      </Field>
      <FormActions loading={loading} onCancel={onCancel} />
    </form>
  )
}

export function SallatPage() {
  return (
    <CrudPage
      title="Sallat"
      subtitle="Dhomat e trajtimeve dhe terapive"
      emptyIcon={Building2}
      api={sallatApi}
      FormComponent={SallaForm}
      idKey="sallaId"
      searchKeys={['emri', 'tipi']}
      columns={[
        { key: 'emri', label: 'Emri' },
        { key: 'tipi', label: 'Tipi' },
        { key: 'kapaciteti', label: 'Kapaciteti' },
        {
          key: 'aktive',
          label: 'Statusi',
          render: (item) => <StatusBadge status={item.aktive ? 'Aktiv' : 'Perfunduar'} />,
        },
      ]}
    />
  )
}

// ════════════════════════════════════════════════════════════════════════════
// 2. FURNIZUESIT
// ════════════════════════════════════════════════════════════════════════════

function FurnizuesiForm({ initial, onSave, loading, onCancel }) {
  const [f, setF] = useState({
    emri: '',
    kontaktPersona: '',
    email: '',
    telefoni: '',
    adresa: '',
    aktiv: true,
    ...initial,
  })
  const set = (k) => (e) => setF((p) => ({ ...p, [k]: e.target.value }))
  const submit = (e) => {
    e.preventDefault()
    onSave({
      emri: f.emri?.trim(),
      kontaktPersona: f.kontaktPersona?.trim() || null,
      email: f.email?.trim() || null,
      telefoni: f.telefoni?.trim() || null,
      adresa: f.adresa?.trim() || null,
      aktiv: !!f.aktiv,
    })
  }
  return (
    <form onSubmit={submit} className="space-y-4">
      <Field label="Emri i Furnizuesit" required>
        <input className="input" value={emptyStr(f.emri)} onChange={set('emri')} required />
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field label="Personi i Kontaktit">
          <input className="input" value={emptyStr(f.kontaktPersona)} onChange={set('kontaktPersona')} />
        </Field>
        <Field label="Telefoni">
          <input className="input" value={emptyStr(f.telefoni)} onChange={set('telefoni')} />
        </Field>
      </div>
      <Field label="Email">
        <input className="input" type="email" value={emptyStr(f.email)} onChange={set('email')} />
      </Field>
      <Field label="Adresa">
        <input className="input" value={emptyStr(f.adresa)} onChange={set('adresa')} />
      </Field>
      <Field label="Aktiv">
        <label className="inline-flex items-center gap-2">
          <input type="checkbox" checked={!!f.aktiv} onChange={(e) => setF((p) => ({ ...p, aktiv: e.target.checked }))} />
          <span className="text-sm">Furnizuesi është në bashkëpunim</span>
        </label>
      </Field>
      <FormActions loading={loading} onCancel={onCancel} />
    </form>
  )
}

export function FurnizuesitPage() {
  return (
    <CrudPage
      title="Furnizuesit"
      subtitle="Kompanitë furnizuese të produkteve"
      emptyIcon={Truck}
      api={furnizuesitApi}
      FormComponent={FurnizuesiForm}
      idKey="furnizuesId"
      searchKeys={['emri', 'email']}
      columns={[
        { key: 'emri', label: 'Emri' },
        { key: 'kontaktPersona', label: 'Kontakti' },
        { key: 'email', label: 'Email' },
        { key: 'telefoni', label: 'Telefoni' },
        {
          key: 'aktiv',
          label: 'Statusi',
          render: (item) => <StatusBadge status={item.aktiv ? 'Aktiv' : 'Perfunduar'} />,
        },
      ]}
    />
  )
}

// ════════════════════════════════════════════════════════════════════════════
// 3. LAJMERIMET
// ════════════════════════════════════════════════════════════════════════════

function LajmerimiForm({ initial, onSave, loading, onCancel }) {
  const safeInitial = { ...initial, dataSkadimit: toDateInputValue(initial?.dataSkadimit) }
  const [f, setF] = useState({
    titulli: '',
    permbajtja: '',
    audienca: 'All',
    prioriteti: 'Mesem',
    dataSkadimit: '',
    aktiv: true,
    ...safeInitial,
  })
  const set = (k) => (e) => setF((p) => ({ ...p, [k]: e.target.value }))
  const submit = (e) => {
    e.preventDefault()
    onSave({
      titulli: f.titulli?.trim(),
      permbajtja: f.permbajtja?.trim(),
      audienca: f.audienca || 'All',
      prioriteti: f.prioriteti || 'Mesem',
      dataSkadimit: f.dataSkadimit ? new Date(f.dataSkadimit).toISOString() : null,
      aktiv: !!f.aktiv,
    })
  }
  return (
    <form onSubmit={submit} className="space-y-4">
      <Field label="Titulli" required>
        <input className="input" value={emptyStr(f.titulli)} onChange={set('titulli')} required />
      </Field>
      <Field label="Përmbajtja" required>
        <textarea className="input" rows={4} value={emptyStr(f.permbajtja)} onChange={set('permbajtja')} required />
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field label="Audienca">
          <select className="input" value={emptyStr(f.audienca)} onChange={set('audienca')}>
            <option value="All">Të gjithë</option>
            <option value="Klient">Vetëm Klientët</option>
            <option value="Admin">Vetëm Admin</option>
            <option value="Therapist">Vetëm Terapistët</option>
          </select>
        </Field>
        <Field label="Prioriteti">
          <select className="input" value={emptyStr(f.prioriteti)} onChange={set('prioriteti')}>
            <option value="Ulet">I ulët</option>
            <option value="Mesem">I mesëm</option>
            <option value="Larte">I lartë</option>
          </select>
        </Field>
      </div>
      <Field label="Data e Skadimit (opsionale)">
        <input className="input" type="date" value={emptyStr(f.dataSkadimit)} onChange={set('dataSkadimit')} />
      </Field>
      <Field label="Aktiv">
        <label className="inline-flex items-center gap-2">
          <input type="checkbox" checked={!!f.aktiv} onChange={(e) => setF((p) => ({ ...p, aktiv: e.target.checked }))} />
          <span className="text-sm">I dukshëm për audiencën</span>
        </label>
      </Field>
      <FormActions loading={loading} onCancel={onCancel} />
    </form>
  )
}

export function LajmerimetPage() {
  return (
    <CrudPage
      title="Lajmërimet"
      subtitle="Njoftime dhe lajme për përdoruesit"
      emptyIcon={Megaphone}
      api={lajmerimetApi}
      FormComponent={LajmerimiForm}
      idKey="lajmerimId"
      searchKeys={['titulli']}
      columns={[
        { key: 'titulli', label: 'Titulli' },
        { key: 'audienca', label: 'Audienca' },
        {
          key: 'prioriteti',
          label: 'Prioriteti',
          render: (item) => {
            const color =
              item.prioriteti === 'Larte'
                ? 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300'
                : item.prioriteti === 'Mesem'
                ? 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-300'
                : 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300'
            return <span className={`px-2 py-0.5 rounded text-xs font-medium ${color}`}>{item.prioriteti}</span>
          },
        },
        {
          key: 'dataKrijimit',
          label: 'Krijuar',
          render: (item) =>
            item.dataKrijimit ? new Date(item.dataKrijimit).toLocaleDateString('sq-AL') : '-',
        },
        {
          key: 'aktiv',
          label: 'Statusi',
          render: (item) => <StatusBadge status={item.aktiv ? 'Aktiv' : 'Perfunduar'} />,
        },
      ]}
    />
  )
}

// ════════════════════════════════════════════════════════════════════════════
// 4. ZBRITJET
// ════════════════════════════════════════════════════════════════════════════

function ZbritjeForm({ initial, onSave, loading, onCancel }) {
  const safeInitial = {
    ...initial,
    dataFillimit: toDateInputValue(initial?.dataFillimit),
    dataMbarimit: toDateInputValue(initial?.dataMbarimit),
  }
  const [f, setF] = useState({
    kodi: '',
    perqindjaZbritjes: 10,
    dataFillimit: toDateInputValue(new Date()),
    dataMbarimit: '',
    limitiPerdorimit: 100,
    aktive: true,
    ...safeInitial,
  })
  const set = (k) => (e) => setF((p) => ({ ...p, [k]: e.target.value }))
  const submit = (e) => {
    e.preventDefault()
    onSave({
      kodi: f.kodi?.trim()?.toUpperCase(),
      perqindjaZbritjes: Number(f.perqindjaZbritjes) || 0,
      dataFillimit: f.dataFillimit ? new Date(f.dataFillimit).toISOString() : null,
      dataMbarimit: f.dataMbarimit ? new Date(f.dataMbarimit).toISOString() : null,
      limitiPerdorimit: Number(f.limitiPerdorimit) || 0,
      aktive: !!f.aktive,
    })
  }
  return (
    <form onSubmit={submit} className="space-y-4">
      <div className="grid grid-cols-2 gap-4">
        <Field label="Kodi i Zbritjes" required>
          <input
            className="input uppercase"
            value={emptyStr(f.kodi)}
            onChange={set('kodi')}
            placeholder="P.SH. VERA2026"
            required
          />
        </Field>
        <Field label="Përqindja %" required>
          <input
            className="input"
            type="number"
            min="0"
            max="100"
            step="0.5"
            value={emptyStr(f.perqindjaZbritjes)}
            onChange={set('perqindjaZbritjes')}
            required
          />
        </Field>
      </div>
      <div className="grid grid-cols-2 gap-4">
        <Field label="Data e Fillimit" required>
          <input
            className="input"
            type="date"
            value={emptyStr(f.dataFillimit)}
            onChange={set('dataFillimit')}
            required
          />
        </Field>
        <Field label="Data e Mbarimit" required>
          <input
            className="input"
            type="date"
            value={emptyStr(f.dataMbarimit)}
            onChange={set('dataMbarimit')}
            required
          />
        </Field>
      </div>
      <Field label="Limiti i Përdorimit">
        <input
          className="input"
          type="number"
          min="1"
          value={emptyStr(f.limitiPerdorimit)}
          onChange={set('limitiPerdorimit')}
        />
      </Field>
      <Field label="Aktive">
        <label className="inline-flex items-center gap-2">
          <input type="checkbox" checked={!!f.aktive} onChange={(e) => setF((p) => ({ ...p, aktive: e.target.checked }))} />
          <span className="text-sm">Kodi mund të përdoret</span>
        </label>
      </Field>
      <FormActions loading={loading} onCancel={onCancel} />
    </form>
  )
}

export function ZbritjetPage() {
  return (
    <CrudPage
      title="Zbritjet"
      subtitle="Kode promo dhe zbritje për shërbime / produkte"
      emptyIcon={Tag}
      api={zbritjetApi}
      FormComponent={ZbritjeForm}
      idKey="zbritjeId"
      searchKeys={['kodi']}
      columns={[
        {
          key: 'kodi',
          label: 'Kodi',
          render: (item) => (
            <span className="font-mono font-bold text-health-accent">{item.kodi}</span>
          ),
        },
        {
          key: 'perqindjaZbritjes',
          label: 'Zbritja',
          render: (item) => `${item.perqindjaZbritjes}%`,
        },
        {
          key: 'dataMbarimit',
          label: 'Skadon',
          render: (item) =>
            item.dataMbarimit ? new Date(item.dataMbarimit).toLocaleDateString('sq-AL') : '-',
        },
        {
          key: 'hereshShfrytezuar',
          label: 'Përdorur',
          render: (item) => `${item.hereshShfrytezuar ?? 0} / ${item.limitiPerdorimit ?? '∞'}`,
        },
        {
          key: 'aktive',
          label: 'Statusi',
          render: (item) => <StatusBadge status={item.aktive ? 'Aktiv' : 'Perfunduar'} />,
        },
      ]}
    />
  )
}

// ════════════════════════════════════════════════════════════════════════════
// 5. PUSHIMET
// ════════════════════════════════════════════════════════════════════════════

function PushimiForm({ initial, onSave, loading, onCancel, terapistet = [] }) {
  const safeInitial = {
    ...initial,
    dataFillimit: toDateInputValue(initial?.dataFillimit),
    dataMbarimit: toDateInputValue(initial?.dataMbarimit),
  }
  const [f, setF] = useState({
    terapistId: '',
    dataFillimit: '',
    dataMbarimit: '',
    arsyeja: '',
    statusi: 'Kerkuar',
    ...safeInitial,
  })
  const set = (k) => (e) => setF((p) => ({ ...p, [k]: e.target.value }))
  const submit = (e) => {
    e.preventDefault()
    onSave({
      terapistId: Number(f.terapistId),
      dataFillimit: f.dataFillimit ? new Date(f.dataFillimit).toISOString() : null,
      dataMbarimit: f.dataMbarimit ? new Date(f.dataMbarimit).toISOString() : null,
      arsyeja: f.arsyeja?.trim() || null,
      statusi: f.statusi || 'Kerkuar',
    })
  }
  return (
    <form onSubmit={submit} className="space-y-4">
      <Field label="Terapisti" required>
        <select className="input" value={emptyStr(f.terapistId)} onChange={set('terapistId')} required>
          <option value="">-- Zgjidh terapistin --</option>
          {terapistet.map((t) => (
            <option key={t.terapistId} value={t.terapistId}>
              {t.emri} {t.mbiemri} {t.specializimi ? `— ${t.specializimi}` : ''}
            </option>
          ))}
        </select>
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field label="Data e Fillimit" required>
          <input className="input" type="date" value={emptyStr(f.dataFillimit)} onChange={set('dataFillimit')} required />
        </Field>
        <Field label="Data e Mbarimit" required>
          <input className="input" type="date" value={emptyStr(f.dataMbarimit)} onChange={set('dataMbarimit')} required />
        </Field>
      </div>
      <Field label="Arsyeja">
        <textarea className="input" rows={3} value={emptyStr(f.arsyeja)} onChange={set('arsyeja')} />
      </Field>
      <Field label="Statusi">
        <select className="input" value={emptyStr(f.statusi)} onChange={set('statusi')}>
          <option value="Kerkuar">Kërkuar</option>
          <option value="Aprovuar">Aprovuar</option>
          <option value="Refuzuar">Refuzuar</option>
        </select>
      </Field>
      <FormActions loading={loading} onCancel={onCancel} />
    </form>
  )
}

export function PushimetPage() {
  // Load therapists once so the select in the form can show names instead of IDs.
  const [terapistet, setTerapistet] = useState([])
  useEffect(() => {
    terapistetApi
      .getAll('page=1&limit=100')
      .then(({ data }) => setTerapistet(data?.data ?? data ?? []))
      .catch(() => setTerapistet([]))
  }, [])
  const terapistMap = Object.fromEntries(terapistet.map((t) => [t.terapistId, t]))

  return (
    <CrudPage
      title="Pushimet"
      subtitle="Kërkesat për pushim të terapistëve"
      emptyIcon={CalendarOff}
      api={pushimetApi}
      FormComponent={PushimiForm}
      idKey="pushimId"
      extraFormProps={{ terapistet }}
      columns={[
        {
          key: 'terapistId',
          label: 'Terapisti',
          render: (item) => {
            const t = terapistMap[item.terapistId]
            return t ? `${t.emri} ${t.mbiemri}` : `#${item.terapistId}`
          },
        },
        {
          key: 'dataFillimit',
          label: 'Nga',
          render: (item) =>
            item.dataFillimit ? new Date(item.dataFillimit).toLocaleDateString('sq-AL') : '-',
        },
        {
          key: 'dataMbarimit',
          label: 'Deri',
          render: (item) =>
            item.dataMbarimit ? new Date(item.dataMbarimit).toLocaleDateString('sq-AL') : '-',
        },
        { key: 'arsyeja', label: 'Arsyeja' },
        {
          key: 'statusi',
          label: 'Statusi',
          render: (item) => {
            const color =
              item.statusi === 'Aprovuar'
                ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300'
                : item.statusi === 'Refuzuar'
                ? 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300'
                : 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-300'
            return <span className={`px-2 py-0.5 rounded text-xs font-medium ${color}`}>{item.statusi}</span>
          },
        },
      ]}
    />
  )
}
