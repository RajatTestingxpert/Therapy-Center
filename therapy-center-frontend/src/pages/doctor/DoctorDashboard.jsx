import { useState, useEffect } from 'react'
import { doctors } from '../../api/api'
import { patientName, therapyName } from '../../utils/display'
import { useAuth } from '../../context/AuthContext'

function statusBadge(status) {
  const map = { Scheduled: 'badge-blue', Completed: 'badge-green', Cancelled: 'badge-red' }
  return <span className={`badge ${map[status] || 'badge-gray'}`}>{status}</span>
}

export default function DoctorDashboard() {
  const { user } = useAuth()
  const [appts, setAppts] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    doctors.getMyAppointments()
      .then(data => setAppts(data || []))
      .catch(e => setError(e.message))
      .finally(() => setLoading(false))
  }, [])

  const today = new Date().toISOString().slice(0, 10)
  const todayAppts = appts.filter(a => a.appointmentDate === today)
  const upcoming = appts.filter(a => a.appointmentDate > today && a.status === 'Scheduled')

  if (loading) return <div className="loading">Loading…</div>

  return (
    <div>
      <div className="page-header">
        <h1>My Dashboard</h1>
        <p>Welcome, Dr. {user?.fullName}</p>
      </div>

      {error && <div className="alert alert-error">{error}</div>}

      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-icon">📅</div>
          <div className="stat-label">Today</div>
          <div className="stat-value">{todayAppts.length}</div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">⏳</div>
          <div className="stat-label">Upcoming</div>
          <div className="stat-value">{upcoming.length}</div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">✅</div>
          <div className="stat-label">Completed</div>
          <div className="stat-value">{appts.filter(a => a.status === 'Completed').length}</div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">📋</div>
          <div className="stat-label">Total</div>
          <div className="stat-value">{appts.length}</div>
        </div>
      </div>

      <div className="card mb-24">
        <div className="card-header"><h2>Today's Appointments — {today}</h2></div>
        <div className="table-wrapper">
          {todayAppts.length === 0 ? (
            <div className="empty-state"><p>No appointments today.</p></div>
          ) : (
            <table>
              <thead>
                <tr><th>Time</th><th>Patient</th><th>Therapy</th><th>Notes</th><th>Status</th></tr>
              </thead>
              <tbody>
                {todayAppts.map(a => (
                  <tr key={a.appointmentId}>
                    <td>{a.startTime} – {a.endTime}</td>
                    <td className="fw-600">{patientName(a.patient)}</td>
                    <td>{therapyName(a.therapy)}</td>
                    <td className="text-muted">{a.notes || '—'}</td>
                    <td>{statusBadge(a.status)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>

      <div className="card">
        <div className="card-header"><h2>All Appointments</h2></div>
        <div className="table-wrapper">
          {appts.length === 0 ? (
            <div className="empty-state"><div className="empty-icon">📋</div><p>No appointments found.</p></div>
          ) : (
            <table>
              <thead>
                <tr><th>#</th><th>Date</th><th>Time</th><th>Patient</th><th>Therapy</th><th>Status</th></tr>
              </thead>
              <tbody>
                {appts.map(a => (
                  <tr key={a.appointmentId}>
                    <td className="text-muted">{a.appointmentId}</td>
                    <td>{a.appointmentDate}</td>
                    <td>{a.startTime}</td>
                    <td className="fw-600">{patientName(a.patient)}</td>
                    <td>{therapyName(a.therapy)}</td>
                    <td>{statusBadge(a.status)}</td>
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
