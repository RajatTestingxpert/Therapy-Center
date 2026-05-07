import { useState, useEffect } from 'react'
import { appointments, patients } from '../../api/api'
import { doctorName, patientName, therapyName } from '../../utils/display'
import { useAuth } from '../../context/AuthContext'

function statusBadge(status) {
  const map = { Scheduled: 'badge-blue', Completed: 'badge-green', Cancelled: 'badge-red' }
  return <span className={`badge ${map[status] || 'badge-gray'}`}>{status}</span>
}

export default function StaffDashboard() {
  const { user } = useAuth()
  const [date, setDate] = useState(new Date().toISOString().slice(0, 10))
  const [appts, setAppts] = useState([])
  const [patientCount, setPatientCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    patients.getAll().then(d => setPatientCount(d?.length || 0)).catch(() => {})
    loadAppts()
  }, [])

  async function loadAppts() {
    setLoading(true)
    try {
      const data = await appointments.getByDate(date)
      setAppts(data || [])
    } catch (e) {
      setError(e.message)
    } finally {
      setLoading(false)
    }
  }

  const scheduled = appts.filter(a => a.status === 'Scheduled').length
  const completed = appts.filter(a => a.status === 'Completed').length

  return (
    <div>
      <div className="page-header">
        <h1>Staff Dashboard</h1>
        <p>Welcome, {user?.fullName}</p>
      </div>

      {error && <div className="alert alert-error">{error}</div>}

      <div className="stats-grid mb-24">
        <div className="stat-card">
          <div className="stat-icon">📋</div>
          <div className="stat-label">Today's Appointments</div>
          <div className="stat-value">{appts.length}</div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">⏳</div>
          <div className="stat-label">Scheduled</div>
          <div className="stat-value">{scheduled}</div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">✅</div>
          <div className="stat-label">Completed</div>
          <div className="stat-value">{completed}</div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">🧑‍🤝‍🧑</div>
          <div className="stat-label">Total Patients</div>
          <div className="stat-value">{patientCount}</div>
        </div>
      </div>

      <div className="card">
        <div className="card-header flex-between">
          <h2>Appointments</h2>
          <div className="flex gap-8" style={{ alignItems: 'center' }}>
            <input type="date" value={date} onChange={e => setDate(e.target.value)} style={{ width: 160 }} />
            <button className="btn btn-secondary btn-sm" onClick={loadAppts} disabled={loading}>
              {loading ? '…' : 'Search'}
            </button>
          </div>
        </div>
        <div className="table-wrapper">
          {appts.length === 0 ? (
            <div className="empty-state"><div className="empty-icon">📋</div><p>No appointments for this date.</p></div>
          ) : (
            <table>
              <thead>
                <tr><th>#</th><th>Patient</th><th>Doctor</th><th>Therapy</th><th>Time</th><th>Status</th></tr>
              </thead>
              <tbody>
                {appts.map(a => (
                  <tr key={a.appointmentId}>
                    <td className="text-muted">{a.appointmentId}</td>
                    <td className="fw-600">{patientName(a.patient)}</td>
                    <td>{doctorName(a.doctor)}</td>
                    <td>{therapyName(a.therapy)}</td>
                    <td>{a.startTime}</td>
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
