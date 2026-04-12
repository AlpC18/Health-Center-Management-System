import { useState, useEffect } from 'react'
import { Field, Spinner } from '../ui/index'

const emptyStr = (v) => (v === undefined || v === null ? '' : v)

function FormActions({ loading, onCancel }) {
  return (
    <div className="flex gap-3 justify-end pt-4 mt-4 border-t border-gray-100">
      <button type="button" className="btn-secondary" onClick={onCancel} disabled={loading}>
        Anulo
      </button>
      <button type="submit" className="btn-primary" disabled={loading}>
        {loading && <Spinner size="sm" />}
        Ruaj
      </button>
    </div>
  )
}

// KlientForm
export function KlientForm({ initial, onSave, loading, onCancel }) {
  const [f, setF] = useState({
    emri: '',
    mbiemri: '',
    email: '',
    telefoni: '',
    dataLindjes: '',
    gjinia: '',
    kushtetShendetesore: '',
    ...initial,
  })

  const set = (k) => (e) => setF((p) => ({ ...p, [k]: e.target.value }))

  const submit = (e) => {
    e.preventDefault()
    onSave(f)
  }

  return (
    <form onSubmit={submit} className="space-y-4">
      <div className="grid grid-cols-2 gap-4">
        <Field label="Emri" required>
          <input className="input" value={emptyStr(f.emri)} onChange={set('emri')} required />
        </Field>
        <Field label="Mbiemri" required>
          <input className="input" value={emptyStr(f.mbiemri)} onChange={set('mbiemri')} required />
        </Field>
      </div>
      <Field label="Email" required>
        <input className="input" type="email" value={emptyStr(f.email)} onChange={set('email')} required />
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field label="Telefoni">
          <input className="input" value={emptyStr(f.telefoni)} onChange={set('telefoni')} />
        </Field>
        <Field label="Data e Lindjes">
          <input className="input" type="date" value={emptyStr(f.dataLindjes)} onChange={set('dataLindjes')} />
        </Field>
      </div>
      <Field label="Gjinia">
        <select className="input" value={emptyStr(f.gjinia)} onChange={set('gjinia')}>
          <option value="">-- Zgjidh --</option>
          <option value="M">Mashkull</option>
          <option value="F">Femër</option>
          <option value="Tjeter">Tjetër</option>
        </select>
      </Field>
      <Field label="Kushtet Shëndetësore">
        <textarea className="input" rows={2} value={emptyStr(f.kushtetShendetesore)} onChange={set('kushtetShendetesore')} />
      </Field>

      <Field label="Foto e Klientit">
        <div className="flex items-center gap-4 mt-1">
          {f.fotoPath && (
            <img 
              src={`http://localhost:5077${f.fotoPath}`} 
              alt="Preview" 
              className="h-16 w-16 rounded-lg object-cover border border-gray-100 dark:border-gray-700" 
            />
          )}
          <input 
            type="file" 
            accept="image/*"
            onChange={(e) => {
              const file = e.target.files?.[0]
              if (file) setF(prev => ({ ...prev, _newFoto: file }))
            }}
            className="text-xs text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-full file:border-0 file:text-xs file:font-semibold file:bg-green-50 file:text-green-700 hover:file:bg-green-100 dark:file:bg-gray-700 dark:file:text-gray-300"
          />
        </div>
      </Field>
      <FormActions loading={loading} onCancel={onCancel} />
    </form>
  )
}

// SherbimForm
export function SherbimForm({ initial, onSave, loading, onCancel }) {
  const [f, setF] = useState({
    emriSherbimit: '',
    kategoria: '',
    pershkrimi: '',
    kohezgjatjaMin: '',
    cmimi: '',
    aktiv: true,
    ...initial,
  })

  const set = (k) => (e) => setF((p) => ({ ...p, [k]: e.target.value }))

  const submit = (e) => {
    e.preventDefault()
    onSave(f)
  }

  return (
    <form onSubmit={submit} className="space-y-4">
      <Field label="Emri i Shërbimit" required>
        <input className="input" value={emptyStr(f.emriSherbimit)} onChange={set('emriSherbimit')} required />
      </Field>
      <Field label="Kategoria" required>
        <select className="input" value={emptyStr(f.kategoria)} onChange={set('kategoria')} required>
          <option value="">-- Zgjidh --</option>
          <option value="Masazh">Masazh</option>
          <option value="Yoga">Yoga</option>
          <option value="Meditim">Meditim</option>
          <option value="Spa">Spa</option>
          <option value="Fizioterapi">Fizioterapi</option>
          <option value="Nutricion">Nutricion</option>
          <option value="Tjeter">Tjetër</option>
        </select>
      </Field>
      <Field label="Përshkrimi">
        <textarea className="input" rows={3} value={emptyStr(f.pershkrimi)} onChange={set('pershkrimi')} />
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field label="Kohëzgjatja (min)" required>
          <input className="input" type="number" min="1" value={emptyStr(f.kohezgjatjaMin)} onChange={set('kohezgjatjaMin')} required />
        </Field>
        <Field label="Çmimi (€)" required>
          <input className="input" type="number" min="0" step="0.01" value={emptyStr(f.cmimi)} onChange={set('cmimi')} required />
        </Field>
      </div>
      <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer">
        <input
          type="checkbox"
          checked={!!f.aktiv}
          onChange={(e) => setF((p) => ({ ...p, aktiv: e.target.checked }))}
          className="rounded border-gray-300 text-green-600"
        />
        Aktiv
      </label>
      <FormActions loading={loading} onCancel={onCancel} />
    </form>
  )
}

// TerapistForm
export function TerapistForm({ initial, onSave, loading, onCancel }) {
  const [f, setF] = useState({
    emri: '',
    mbiemri: '',
    specializimi: '',
    licenca: '',
    email: '',
    telefoni: '',
    aktiv: true,
    ...initial,
  })

  const set = (k) => (e) => setF((p) => ({ ...p, [k]: e.target.value }))

  const submit = (e) => {
    e.preventDefault()
    onSave(f)
  }

  return (
    <form onSubmit={submit} className="space-y-4">
      <div className="grid grid-cols-2 gap-4">
        <Field label="Emri" required>
          <input className="input" value={emptyStr(f.emri)} onChange={set('emri')} required />
        </Field>
        <Field label="Mbiemri" required>
          <input className="input" value={emptyStr(f.mbiemri)} onChange={set('mbiemri')} required />
        </Field>
      </div>
      <Field label="Specializimi" required>
        <select className="input" value={emptyStr(f.specializimi)} onChange={set('specializimi')} required>
          <option value="">-- Zgjidh --</option>
          <option value="Masazh Terapeutik">Masazh Terapeutik</option>
          <option value="Yoga & Meditim">Yoga &amp; Meditim</option>
          <option value="Fizioterapi">Fizioterapi</option>
          <option value="Nutricion">Nutricion</option>
          <option value="Spa & Beauty">Spa &amp; Beauty</option>
          <option value="Tjeter">Tjetër</option>
        </select>
      </Field>
      <Field label="Licenca">
        <input className="input" value={emptyStr(f.licenca)} onChange={set('licenca')} />
      </Field>
      <Field label="Email" required>
        <input className="input" type="email" value={emptyStr(f.email)} onChange={set('email')} required />
      </Field>
      <Field label="Telefoni">
        <input className="input" value={emptyStr(f.telefoni)} onChange={set('telefoni')} />
      </Field>
      <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer">
        <input
          type="checkbox"
          checked={!!f.aktiv}
          onChange={(e) => setF((p) => ({ ...p, aktiv: e.target.checked }))}
          className="rounded border-gray-300 text-green-600"
        />
        Aktiv
      </label>
      <FormActions loading={loading} onCancel={onCancel} />
    </form>
  )
}

// TerminForm
export function TerminForm({ initial, onSave, loading, onCancel, klientet = [], sherbimet = [], terapistet = [] }) {
  const [f, setF] = useState({
    klientId: '',
    sherbimId: '',
    terapistId: '',
    dataTerminit: '',
    oraFillimit: '',
    oraMbarimit: '',
    statusi: 'Planifikuar',
    shenimet: '',
    ...initial,
  })

  const set = (k) => (e) => setF((p) => ({ ...p, [k]: e.target.value }))

  const submit = (e) => {
    e.preventDefault()
    onSave(f)
  }

  return (
    <form onSubmit={submit} className="space-y-4">
      <Field label="Klienti" required>
        <select className="input" value={emptyStr(f.klientId)} onChange={set('klientId')} required>
          <option value="">-- Zgjidh klientin --</option>
          {klientet.map((k) => (
            <option key={k.klientId} value={k.klientId}>
              {k.emri} {k.mbiemri}
            </option>
          ))}
        </select>
      </Field>
      <Field label="Shërbimi" required>
        <select className="input" value={emptyStr(f.sherbimId)} onChange={set('sherbimId')} required>
          <option value="">-- Zgjidh shërbimin --</option>
          {sherbimet.map((s) => (
            <option key={s.sherbimId} value={s.sherbimId}>
              {s.emriSherbimit}
            </option>
          ))}
        </select>
      </Field>
      <Field label="Terapisti" required>
        <select className="input" value={emptyStr(f.terapistId)} onChange={set('terapistId')} required>
          <option value="">-- Zgjidh terapistin --</option>
          {terapistet.map((t) => (
            <option key={t.terapistId} value={t.terapistId}>
              {t.emri} {t.mbiemri}
            </option>
          ))}
        </select>
      </Field>
      <Field label="Data e Terminit" required>
        <input className="input" type="date" value={emptyStr(f.dataTerminit)} onChange={set('dataTerminit')} required />
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field label="Ora Fillimit" required>
          <input className="input" type="time" value={emptyStr(f.oraFillimit)} onChange={set('oraFillimit')} required />
        </Field>
        <Field label="Ora Mbarimit" required>
          <input className="input" type="time" value={emptyStr(f.oraMbarimit)} onChange={set('oraMbarimit')} required />
        </Field>
      </div>
      <Field label="Statusi">
        <select className="input" value={emptyStr(f.statusi)} onChange={set('statusi')}>
          <option value="Planifikuar">Planifikuar</option>
          <option value="Konfirmuar">Konfirmuar</option>
          <option value="Anuluar">Anuluar</option>
          <option value="Perfunduar">Perfunduar</option>
        </select>
      </Field>
      <Field label="Shënime">
        <textarea className="input" rows={3} value={emptyStr(f.shenimet)} onChange={set('shenimet')} />
      </Field>
      <FormActions loading={loading} onCancel={onCancel} />
    </form>
  )
}

// PaketaForm
export function PaketaForm({ initial, onSave, loading, onCancel }) {
  const [f, setF] = useState({
    emriPaketes: '',
    pershkrimi: '',
    sherbimiPerfshire: '',
    cmimi: '',
    kohezgjatjaMuaj: '',
    aktive: true,
    ...initial,
  })

  const set = (k) => (e) => setF((p) => ({ ...p, [k]: e.target.value }))

  const submit = (e) => {
    e.preventDefault()
    onSave(f)
  }

  return (
    <form onSubmit={submit} className="space-y-4">
      <Field label="Emri i Paketës" required>
        <input className="input" value={emptyStr(f.emriPaketes)} onChange={set('emriPaketes')} required />
      </Field>
      <Field label="Përshkrimi">
        <textarea className="input" rows={3} value={emptyStr(f.pershkrimi)} onChange={set('pershkrimi')} />
      </Field>
      <Field label="Shërbimet e Përfshira">
        <input className="input" value={emptyStr(f.sherbimiPerfshire)} onChange={set('sherbimiPerfshire')} />
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field label="Çmimi (€)" required>
          <input className="input" type="number" min="0" step="0.01" value={emptyStr(f.cmimi)} onChange={set('cmimi')} required />
        </Field>
        <Field label="Kohëzgjatja (muaj)" required>
          <input className="input" type="number" min="1" value={emptyStr(f.kohezgjatjaMuaj)} onChange={set('kohezgjatjaMuaj')} required />
        </Field>
      </div>
      <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer">
        <input
          type="checkbox"
          checked={!!f.aktive}
          onChange={(e) => setF((p) => ({ ...p, aktive: e.target.checked }))}
          className="rounded border-gray-300 text-green-600"
        />
        Aktive
      </label>
      <FormActions loading={loading} onCancel={onCancel} />
    </form>
  )
}

// AnetaresimForm
export function AnetaresimForm({ initial, onSave, loading, onCancel, klientet = [], paketat = [] }) {
  const [f, setF] = useState({
    klientId: '',
    paketId: '',
    dataFillimit: '',
    dataMbarimit: '',
    statusi: 'Aktiv',
    cmimiPaguar: '',
    ...initial,
  })

  const set = (k) => (e) => setF((p) => ({ ...p, [k]: e.target.value }))

  const submit = (e) => {
    e.preventDefault()
    onSave(f)
  }

  return (
    <form onSubmit={submit} className="space-y-4">
      <Field label="Klienti" required>
        <select className="input" value={emptyStr(f.klientId)} onChange={set('klientId')} required>
          <option value="">-- Zgjidh klientin --</option>
          {klientet.map((k) => (
            <option key={k.klientId} value={k.klientId}>
              {k.emri} {k.mbiemri}
            </option>
          ))}
        </select>
      </Field>
      <Field label="Paketa" required>
        <select className="input" value={emptyStr(f.paketId)} onChange={set('paketId')} required>
          <option value="">-- Zgjidh paketën --</option>
          {paketat.map((p) => (
            <option key={p.paketId} value={p.paketId}>
              {p.emriPaketes}
            </option>
          ))}
        </select>
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field label="Data Fillimit" required>
          <input className="input" type="date" value={emptyStr(f.dataFillimit)} onChange={set('dataFillimit')} required />
        </Field>
        <Field label="Data Mbarimit" required>
          <input className="input" type="date" value={emptyStr(f.dataMbarimit)} onChange={set('dataMbarimit')} required />
        </Field>
      </div>
      <div className="grid grid-cols-2 gap-4">
        <Field label="Statusi">
          <select className="input" value={emptyStr(f.statusi)} onChange={set('statusi')}>
            <option value="Aktiv">Aktiv</option>
            <option value="Skaduar">Skaduar</option>
            <option value="Anuluar">Anuluar</option>
          </select>
        </Field>
        <Field label="Çmimi i Paguar (€)">
          <input className="input" type="number" min="0" step="0.01" value={emptyStr(f.cmimiPaguar)} onChange={set('cmimiPaguar')} />
        </Field>
      </div>
      <FormActions loading={loading} onCancel={onCancel} />
    </form>
  )
}

// ProgramForm
export function ProgramForm({ initial, onSave, loading, onCancel }) {
  const [f, setF] = useState({
    emriProgramit: '',
    pershkrimi: '',
    kohezgjatjaJave: '',
    qellimi: '',
    ushtrimet: '',
    dieta: '',
    ...initial,
  })

  const set = (k) => (e) => setF((p) => ({ ...p, [k]: e.target.value }))

  const submit = (e) => {
    e.preventDefault()
    onSave(f)
  }

  return (
    <form onSubmit={submit} className="space-y-4">
      <Field label="Emri i Programit" required>
        <input className="input" value={emptyStr(f.emriProgramit)} onChange={set('emriProgramit')} required />
      </Field>
      <Field label="Përshkrimi">
        <textarea className="input" rows={3} value={emptyStr(f.pershkrimi)} onChange={set('pershkrimi')} />
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field label="Kohëzgjatja (javë)" required>
          <input className="input" type="number" min="1" value={emptyStr(f.kohezgjatjaJave)} onChange={set('kohezgjatjaJave')} required />
        </Field>
        <Field label="Qëllimi">
          <input className="input" value={emptyStr(f.qellimi)} onChange={set('qellimi')} />
        </Field>
      </div>
      <Field label="Ushtrimet">
        <textarea className="input" rows={3} value={emptyStr(f.ushtrimet)} onChange={set('ushtrimet')} />
      </Field>
      <Field label="Dieta">
        <textarea className="input" rows={3} value={emptyStr(f.dieta)} onChange={set('dieta')} />
      </Field>
      <FormActions loading={loading} onCancel={onCancel} />
    </form>
  )
}

// ProduktForm
export function ProduktForm({ initial, onSave, loading, onCancel }) {
  const [f, setF] = useState({
    emriProduktit: '',
    kategoria: '',
    pershkrimi: '',
    cmimi: '',
    sasiaStok: '',
    aktiv: true,
    ...initial,
  })

  const set = (k) => (e) => setF((p) => ({ ...p, [k]: e.target.value }))

  const submit = (e) => {
    e.preventDefault()
    onSave(f)
  }

  return (
    <form onSubmit={submit} className="space-y-4">
      <Field label="Emri i Produktit" required>
        <input className="input" value={emptyStr(f.emriProduktit)} onChange={set('emriProduktit')} required />
      </Field>
      <Field label="Kategoria" required>
        <select className="input" value={emptyStr(f.kategoria)} onChange={set('kategoria')} required>
          <option value="">-- Zgjidh --</option>
          <option value="Vajra & Kreme">Vajra &amp; Kreme</option>
          <option value="Suplemente">Suplemente</option>
          <option value="Pajisje Wellness">Pajisje Wellness</option>
          <option value="Tekstile">Tekstile</option>
          <option value="Tjeter">Tjetër</option>
        </select>
      </Field>
      <Field label="Përshkrimi">
        <textarea className="input" rows={3} value={emptyStr(f.pershkrimi)} onChange={set('pershkrimi')} />
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field label="Çmimi (€)" required>
          <input className="input" type="number" min="0" step="0.01" value={emptyStr(f.cmimi)} onChange={set('cmimi')} required />
        </Field>
        <Field label="Sasia në Stok" required>
          <input className="input" type="number" min="0" value={emptyStr(f.sasiaStok)} onChange={set('sasiaStok')} required />
        </Field>
      </div>
      <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer">
        <input
          type="checkbox"
          checked={!!f.aktiv}
          onChange={(e) => setF((p) => ({ ...p, aktiv: e.target.checked }))}
          className="rounded border-gray-300 text-green-600"
        />
        Aktiv
      </label>
      <FormActions loading={loading} onCancel={onCancel} />
    </form>
  )
}

// ShitjeForm
export function ShitjeForm({ initial, onSave, loading, onCancel, klientet = [], produktet = [] }) {
  const [f, setF] = useState({
    klientId: '',
    produktId: '',
    sasia: '',
    cmimiTotal: '',
    ...initial,
  })

  const set = (k) => (e) => {
    const updated = { ...f, [k]: e.target.value }
    // Auto-calculate total
    if (k === 'produktId' || k === 'sasia') {
      const produkt = produktet.find((p) => String(p.produktId) === String(k === 'produktId' ? e.target.value : updated.produktId))
      const sasia = Number(k === 'sasia' ? e.target.value : updated.sasia)
      if (produkt && sasia > 0) {
        updated.cmimiTotal = (produkt.cmimi * sasia).toFixed(2)
      }
    }
    setF(updated)
  }

  const submit = (e) => {
    e.preventDefault()
    onSave(f)
  }

  return (
    <form onSubmit={submit} className="space-y-4">
      <Field label="Klienti" required>
        <select className="input" value={emptyStr(f.klientId)} onChange={set('klientId')} required>
          <option value="">-- Zgjidh klientin --</option>
          {klientet.map((k) => (
            <option key={k.klientId} value={k.klientId}>
              {k.emri} {k.mbiemri}
            </option>
          ))}
        </select>
      </Field>
      <Field label="Produkti" required>
        <select className="input" value={emptyStr(f.produktId)} onChange={set('produktId')} required>
          <option value="">-- Zgjidh produktin --</option>
          {produktet.map((p) => (
            <option key={p.produktId} value={p.produktId}>
              {p.emriProduktit} ({p.cmimi}€)
            </option>
          ))}
        </select>
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field label="Sasia" required>
          <input className="input" type="number" min="1" value={emptyStr(f.sasia)} onChange={set('sasia')} required />
        </Field>
        <Field label="Çmimi Total (€)">
          <input className="input bg-gray-50" type="number" step="0.01" value={emptyStr(f.cmimiTotal)} readOnly />
        </Field>
      </div>
      <FormActions loading={loading} onCancel={onCancel} />
    </form>
  )
}

// VleresimForm
export function VleresimForm({ initial, onSave, loading, onCancel, klientet = [], sherbimet = [], terapistet = [] }) {
  const [f, setF] = useState({
    klientId: '',
    sherbimId: '',
    terapistId: '',
    nota: 5,
    komenti: '',
    ...initial,
  })

  const set = (k) => (e) => setF((p) => ({ ...p, [k]: e.target.value }))

  const submit = (e) => {
    e.preventDefault()
    onSave(f)
  }

  return (
    <form onSubmit={submit} className="space-y-4">
      <Field label="Klienti" required>
        <select className="input" value={emptyStr(f.klientId)} onChange={set('klientId')} required>
          <option value="">-- Zgjidh klientin --</option>
          {klientet.map((k) => (
            <option key={k.klientId} value={k.klientId}>
              {k.emri} {k.mbiemri}
            </option>
          ))}
        </select>
      </Field>
      <Field label="Shërbimi">
        <select className="input" value={emptyStr(f.sherbimId)} onChange={set('sherbimId')}>
          <option value="">-- Zgjidh shërbimin --</option>
          {sherbimet.map((s) => (
            <option key={s.sherbimId} value={s.sherbimId}>
              {s.emriSherbimit}
            </option>
          ))}
        </select>
      </Field>
      <Field label="Terapisti">
        <select className="input" value={emptyStr(f.terapistId)} onChange={set('terapistId')}>
          <option value="">-- Zgjidh terapistin --</option>
          {terapistet.map((t) => (
            <option key={t.terapistId} value={t.terapistId}>
              {t.emri} {t.mbiemri}
            </option>
          ))}
        </select>
      </Field>
      <Field label="Nota" required>
        <div className="flex gap-2 mt-1">
          {[1, 2, 3, 4, 5].map((n) => (
            <button
              key={n}
              type="button"
              onClick={() => setF((p) => ({ ...p, nota: n }))}
              className={`w-10 h-10 rounded-lg text-sm font-semibold transition-colors ${
                Number(f.nota) === n
                  ? 'bg-yellow-400 text-white'
                  : 'bg-gray-100 text-gray-600 hover:bg-yellow-100'
              }`}
            >
              {n}
            </button>
          ))}
        </div>
      </Field>
      <Field label="Komenti">
        <textarea className="input" rows={3} value={emptyStr(f.komenti)} onChange={set('komenti')} />
      </Field>
      <FormActions loading={loading} onCancel={onCancel} />
    </form>
  )
}
