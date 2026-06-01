import { useState } from 'react'
import { appointments, payments } from '../../api/api'
import { doctorName, patientName, therapyName } from '../../utils/display'

const STATUS_OPTIONS = ['Scheduled', 'Completed', 'Cancelled']

function statusBadge(status) {
  const map = { Scheduled: 'badge-blue', Completed: 'badge-green', Cancelled: 'badge-red' }
  return <span className={`badge ${map[status] || 'badge-gray'}`}>{status}</span>
}

function paymentBadge(payment) {
  const status = payment?.status ?? payment?.Status ?? 'Pending'
  const map = {
    Pending: 'badge-yellow',
    Paid: 'badge-green',
    Failed: 'badge-red',
    Refunded: 'badge-gray',
  }
  return <span className={`badge ${map[status] || 'badge-gray'}`}>{status}</span>
}

export default function AdminAppointments() {
  const [date, setDate] = useState(new Date().toISOString().slice(0, 10))
  const [list, setList] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [updatingId, setUpdatingId] = useState(null)

  async function loadByDate() {
    setLoading(true)
    setError('')
    try {
      const data = await appointments.getByDate(date)
      setList(data || [])
    } catch (e) {
      setError(e.message)
    } finally {
      setLoading(false)
    }
  }

  async function updateStatus(id, status) {
    setUpdatingId(id)
    setError('')
    try {
      await appointments.updateStatus(id, { status })
      setSuccess(`Appointment #${id} marked as ${status}.`)
      loadByDate()
    } catch (e) {
      setError(e.message)
    } finally {
      setUpdatingId(null)
    }
  }

  async function markPaid(id) {
    setUpdatingId(id)
    setError('')
    try {
      await payments.markPaid(id, { transactionId: null })
      setSuccess(`Payment for appointment #${id} marked as Paid.`)
      loadByDate()
    } catch (e) {
      setError(e.message)
    } finally {
      setUpdatingId(null)
    }
  }

  return (
    <div>
      <div className="page-header">
        <h1>Appointments</h1>
        <p>View appointments by date and update their status</p>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      <div className="flex gap-8 mb-16" style={{ alignItems: 'flex-end' }}>
        <div className="form-group" style={{ margin: 0 }}>
          <label>Date</label>
          <input type="date" value={date} onChange={e => setDate(e.target.value)} />
        </div>
        <button className="btn btn-primary" onClick={loadByDate} disabled={loading}>
          {loading ? 'Loading…' : 'Search'}
        </button>
      </div>

      <div className="card">
        <div className="table-wrapper">
          {!loading && list.length === 0 ? (
            <div className="empty-state">
              <div className="empty-icon">📋</div>
              <p>Select a date and press Search to view appointments.</p>
            </div>
          ) : (
            <table>
              <thead>
                <tr>
                  <th>#</th>
                  <th>Patient</th>
                  <th>Doctor</th>
                  <th>Therapy</th>
                  <th>Time</th>
                  <th>Status</th>
                  <th>Payment</th>
                  <th>Action</th>
                  <th>Update Status</th>
                </tr>
              </thead>
              <tbody>
                {list.map(a => {
                  const paymentStatus = a.payment?.status ?? a.payment?.Status ?? 'Pending'

                  return (
                    <tr key={a.appointmentId}>
                      <td className="text-muted">{a.appointmentId}</td>
                      <td className="fw-600">{patientName(a.patient)}</td>
                      <td>{doctorName(a.doctor)}</td>
                      <td>{therapyName(a.therapy)}</td>
                      <td>
                        {a.startTime} – {a.endTime}
                      </td>
                      <td>{statusBadge(a.status)}</td>
                      <td>{paymentBadge(a.payment)}</td>
                      <td>
                        {paymentStatus !== 'Paid' ? (
                          <button
                            className="btn btn-secondary btn-sm"
                            onClick={() => markPaid(a.appointmentId)}
                            disabled={updatingId === a.appointmentId}
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
                          disabled={updatingId === a.appointmentId}
                          onChange={e => updateStatus(a.appointmentId, e.target.value)}
                          style={{ padding: '4px 8px', fontSize: 12 }}
                        >
                          {STATUS_OPTIONS.map(s => (
                            <option key={s}>{s}</option>
                          ))}
                        </select>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </div>
  )
}