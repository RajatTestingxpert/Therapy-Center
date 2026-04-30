import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { appointments, patients } from '../../api/api'
import { doctorName, therapyName, patientName } from '../../utils/display'
import { useAuth } from '../../context/AuthContext'

function statusBadge(status) {
  const map = { Scheduled: 'badge-blue', Completed: 'badge-green', Cancelled: 'badge-red' }
  return <span className={`badge ${map[status] || 'badge-gray'}`}>{status}</span>
}

export default function PatientDashboard() {
  const { user } = useAuth()
  const [appts, setAppts] = useState([])
  const [loading, setLoading] = useState(true)
  const [selectedPatientId, setSelectedPatientId] = useState('')
  const [patientList, setPatientList] = useState([])
  const [error, setError] = useState('')

  const isPatient = user?.role === 'Patient'
  const isGuardian = user?.role === 'Guardian'

  useEffect(() => {
    async function init() {
      try {
        if (isPatient) {
          const me = await patients.getMe()
          setPatientList(me ? [me] : [])
          setSelectedPatientId(me?.patientId ? String(me.patientId) : '')
          if (me?.patientId) {
            setAppts(await appointments.getByPatient(me.patientId) || [])
          }
        } else if (isGuardian) {
          const list = await patients.getByGuardian(user.userId)
          setPatientList(list || [])
          const firstId = list?.[0]?.patientId
          setSelectedPatientId(firstId ? String(firstId) : '')
          if (firstId) {
            setAppts(await appointments.getByPatient(firstId) || [])
          }
        }
      } catch (e) {
        setError(e.message)
      } finally {
        setLoading(false)
      }
    }
    init()
  }, [isPatient, isGuardian, user])

  async function load() {
    if (!selectedPatientId) return
    setLoading(true)
    setError('')
    try {
      setAppts(await appointments.getByPatient(selectedPatientId) || [])
    } catch (e) {
      setError(e.message)
    } finally {
      setLoading(false)
    }
  }

  const scheduled = appts.filter(a => a.status === 'Scheduled')

  return (
    <div>
      <div className="page-header">
        <h1>My Appointments</h1>
        <p>Welcome, {user?.fullName}</p>
      </div>

      {error && <div className="alert alert-error">{error}</div>}

      {!isPatient && (
        <div className="card mb-24" style={{ maxWidth: 560 }}>
          <div className="card-header"><h2>Select Patient</h2></div>
          <div className="card-body">
            <div className="flex gap-8" style={{ alignItems: 'flex-end' }}>
              <div className="form-group" style={{ flex: 1, margin: 0 }}>
                <label>Patient</label>
                <select value={selectedPatientId} onChange={e => setSelectedPatientId(e.target.value)}>
                  <option value="">Select patient…</option>
                  {patientList.map(p => (
                    <option key={p.patientId} value={p.patientId}>
                      {patientName(p)}
                    </option>
                  ))}
                </select>
              </div>
              <button className="btn btn-primary" onClick={load} disabled={loading || !selectedPatientId}>
                {loading ? 'Loading…' : 'Search'}
              </button>
            </div>
          </div>
        </div>
      )}

      {isPatient && (
        <div className="card mb-24" style={{ maxWidth: 560 }}>
          <div className="card-header"><h2>Your Record</h2></div>
          <div className="card-body">
            <div className="text-muted">Loaded patient profile: <strong>{patientList[0] ? patientName(patientList[0]) : '—'}</strong></div>
          </div>
        </div>
      )}

      {appts.length > 0 && (
        <div className="stats-grid mb-24">
          <div className="stat-card">
            <div className="stat-icon">📋</div>
            <div className="stat-label">Total</div>
            <div className="stat-value">{appts.length}</div>
          </div>
          <div className="stat-card">
            <div className="stat-icon">⏳</div>
            <div className="stat-label">Scheduled</div>
            <div className="stat-value">{scheduled.length}</div>
          </div>
          <div className="stat-card">
            <div className="stat-icon">✅</div>
            <div className="stat-label">Completed</div>
            <div className="stat-value">{appts.filter(a => a.status === 'Completed').length}</div>
          </div>
        </div>
      )}

      <div className="flex-between mb-16">
        <h2 style={{ fontSize: 16, fontWeight: 600 }}>Appointment History</h2>
        <Link to="/patient/book" className="btn btn-primary btn-sm">+ Book New</Link>
      </div>

      <div className="card">
        <div className="table-wrapper">
          {appts.length === 0 ? (
            <div className="empty-state">
              <div className="empty-icon">📋</div>
              <p>{isPatient ? 'No appointments found for your record.' : 'Select a patient above to view appointments.'}</p>
            </div>
          ) : (
            <table>
              <thead>
                <tr><th>#</th><th>Date</th><th>Doctor</th><th>Therapy</th><th>Time</th><th>Status</th></tr>
              </thead>
              <tbody>
                {appts.map(a => (
                  <tr key={a.appointmentId}>
                    <td className="text-muted">{a.appointmentId}</td>
                    <td>{a.appointmentDate}</td>
                    <td className="fw-600">Dr. {doctorName(a.doctor)}</td>
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
