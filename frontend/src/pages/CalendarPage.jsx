import { useState, useEffect } from 'react'
import FullCalendar from '@fullcalendar/react'
import dayGridPlugin from '@fullcalendar/daygrid'
import timeGridPlugin from '@fullcalendar/timegrid'
import interactionPlugin from '@fullcalendar/interaction'
import { terminetApi } from '../api/api'

export default function CalendarPage() {
  const [events, setEvents] = useState([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    terminetApi.getAll().then(({ data }) => {
      const items = data.data || data
      const mapped = items.map(t => ({
        id: String(t.terminId),
        title: `${t.klientEmri} — ${t.sherbimEmri}`,
        start: `${new Date(t.dataTerminit).toISOString().split('T')[0]}T${
          typeof t.oraFillimit === 'string'
            ? t.oraFillimit.substring(0, 5)
            : String(Math.floor(t.oraFillimit / 3600)).padStart(2, '0') + ':' +
              String(Math.floor((t.oraFillimit % 3600) / 60)).padStart(2, '0')
        }:00`,
        end: `${new Date(t.dataTerminit).toISOString().split('T')[0]}T${
          typeof t.oraMbarimit === 'string'
            ? t.oraMbarimit.substring(0, 5)
            : String(Math.floor(t.oraMbarimit / 3600)).padStart(2, '0') + ':' +
              String(Math.floor((t.oraMbarimit % 3600) / 60)).padStart(2, '0')
        }:00`,
        backgroundColor:
          t.statusi === 'Konfirmuar' ? '#58A6FF' :
          t.statusi === 'Planifikuar' ? '#1C2128' :
          t.statusi === 'Anuluar' ? '#E50914' : '#30363D',
        borderColor: 'transparent',
        extendedProps: {
          terapist: t.terapistEmri,
          statusi: t.statusi,
          sherbim: t.sherbimEmri,
        }
      }))
      setEvents(mapped)
    }).catch(() => {}).finally(() => setLoading(false))
  }, [])

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold text-health-primary tracking-tight">Kalendari i Termineve</h1>
        <p className="text-sm text-health-secondary mt-1">Pamja vizuale e të gjitha termineve të klinikës</p>
      </div>
      <div className="card p-6 shadow-2xl">
        {loading ? (
          <div className="flex justify-center py-20">
            <div className="w-12 h-12 border-4 border-health-brand border-t-transparent rounded-full animate-spin" />
          </div>
        ) : (
          <FullCalendar
            plugins={[dayGridPlugin, timeGridPlugin, interactionPlugin]}
            initialView="timeGridWeek"
            headerToolbar={{
              left: 'prev,next today',
              center: 'title',
              right: 'dayGridMonth,timeGridWeek,timeGridDay'
            }}
            events={events}
            height="auto"
            locale="sq"
            buttonText={{
              today: 'Sot',
              month: 'Muaj',
              week: 'Javë',
              day: 'Ditë'
            }}
            eventClick={(info) => {
              const p = info.event.extendedProps
              alert(`Terapist: ${p.terapist}\nShërbim: ${p.sherbim}\nStatusi: ${p.statusi}`)
            }}
            slotMinTime="07:00:00"
            slotMaxTime="21:00:00"
          />
        )}
      </div>
      <div className="flex items-center flex-wrap gap-6 mt-6 px-4">
        {[
          { color: '#1C2128', label: 'Planifikuar', border: '#30363D' },
          { color: '#58A6FF', label: 'Konfirmuar', border: '#58A6FF' },
          { color: '#E50914', label: 'Anuluar', border: '#E50914' },
          { color: '#30363D', label: 'Përfunduar', border: '#30363D' },
        ].map(({ color, label, border }) => (
          <div key={label} className="flex items-center gap-2">
            <div className="w-4 h-4 rounded-md border" style={{ backgroundColor: color, borderColor: border }} />
            <span className="text-xs font-bold text-health-secondary uppercase tracking-widest">{label}</span>
          </div>
        ))}
      </div>
    </div>
  )
}
