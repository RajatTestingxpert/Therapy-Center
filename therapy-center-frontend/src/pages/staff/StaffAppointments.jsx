import { useState } from 'react'
import { appointments } from '../../api/api'
import { doctorName, patientName, therapyName } from '../../utils/display'

const STATUS_OPTIONS = ['Scheduled', 'Completed', 'Cancelled']

function statusBadge(status) {
  const map = { Scheduled: 'badge-blue', Completed: 'badge-green', Cancelled: 'badge-red' }
  return <span className={`badge ${map[status] || 'badge-gray'}`}>{status}</span>
}

export default function StaffAppointments() {
  const [date, setDate] = useState(new Date().toISOString().slice(0, 10))
  const [list, setList] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [searched, setSearched] = useState(false)

  async function load() {
    setLoading(true)
    setError('')
    setSearched(true)
    try {
      setList(await appointments.getByDate(date) || [])
    } catch (e) {
      setError(e.message)
    } finally {
      setLoading(false)
    }
  }

  async function updateStatus(id, status) {
    try {
      await appointments.updateStatus(id, { status })
      setSuccess(`Updated to ${status}.`)
      load()
    } catch (e) {
      setError(e.message)
    }
  }

  return (
    <div>
      <div className="page-header">
        <h1>Appointments</h1>
        <p>View and update appointment statuses</p>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      <div className="flex gap-8 mb-16" style={{ alignItems: 'flex-end' }}>
        <div className="form-group" style={{ margin: 0 }}>
          <label>Date</label>
          <input type="date" value={date} onChange={e => setDate(e.target.value)} />
        </div>
        <button className="btn btn-primary" onClick={load} disabled={loading}>
          {loading ? 'Loading…' : 'Search'}
        </button>
      </div>

      <div className="card">
        <div className="table-wrapper">
          {!searched ? (
            <div className="empty-state"><div className="empty-icon">📋</div><p>Select a date to view appointments.</p></div>
          ) : list.length === 0 ? (
            <div className="empty-state"><p>No appointments found for {date}.</p></div>
          ) : (
            <table>
              <thead>
  <tr><th>#</th><th>Patient</th><th>Doctor</th><th>Therapy</th><th>Time</th><th>Notes</th><th>Status</th><th>Payment</th><th>Action</th><th>Update</th></tr>
</thead>
              <tbody>
                {list.map(a => (
                  <tr key={a.appointmentId}>
  <td className="text-muted">{a.appointmentId}</td>
  <td className="fw-600">{patientName(a.patient)}</td>
  <td>{doctorName(a.doctor)}</td>
  <td>{therapyName(a.therapy)}</td>
  <td>{a.startTime}</td>
  <td className="text-muted">{a.notes || '—'}</td>
  <td>{statusBadge(a.status)}</td>
  <td>{paymentBadge(a.payment)}</td>
  <td>
    {(a.payment?.status ?? a.payment?.Status) !== 'Paid' ? (
      <button
        className="btn btn-secondary btn-sm"
        onClick={() => markPaid(a.appointmentId)}
      >
        Mark Paid
      </button>
    ) : (
      '—'
    )}
  </td>
  <td>
    <select
      value={a.status}
      onChange={e => updateStatus(a.appointmentId, e.target.value)}
      style={{ padding: '4px 8px', fontSize: 12 }}
    >
      {STATUS_OPTIONS.map(s => <option key={s}>{s}</option>)}
    </select>
  </td>
</tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </div>
  )
}
