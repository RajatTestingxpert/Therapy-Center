import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { admin, doctors, patients } from '../../api/api'
import { useAuth } from '../../context/AuthContext'

export default function AdminDashboard() {
  const { user } = useAuth()
  const [stats, setStats] = useState({ therapies: 0, doctors: 0, receptionists: 0, patients: 0 })
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    Promise.all([
      admin.getTherapies(),
      doctors.getAll(),
      admin.getReceptionists(),
      patients.getAll(),
    ]).then(([therapies, docs, recs, pts]) => {
      setStats({
        therapies: therapies?.length ?? 0,
        doctors: docs?.length ?? 0,
        receptionists: recs?.length ?? 0,
        patients: pts?.length ?? 0,
      })
    }).catch(() => {}).finally(() => setLoading(false))
  }, [])

  if (loading) return <div className="loading">Loading…</div>

  return (
    <div>
      <div className="page-header">
        <h1>Admin Dashboard</h1>
        <p>Welcome back, {user?.fullName}</p>
      </div>

      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-icon">💊</div>
          <div className="stat-label">Therapies</div>
          <div className="stat-value">{stats.therapies}</div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">👨‍⚕️</div>
          <div className="stat-label">Doctors</div>
          <div className="stat-value">{stats.doctors}</div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">🧑‍💼</div>
          <div className="stat-label">Receptionists</div>
          <div className="stat-value">{stats.receptionists}</div>
        </div>
        <div className="stat-card">
          <div className="stat-icon">🧑‍🤝‍🧑</div>
          <div className="stat-label">Patients</div>
          <div className="stat-value">{stats.patients}</div>
        </div>
      </div>

      <div className="grid-2">
        <div className="card">
          <div className="card-header"><h2>Quick Actions</h2></div>
          <div className="card-body" style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
            <Link to="/admin/therapies" className="btn btn-secondary">💊 Manage Therapies</Link>
            <Link to="/admin/doctors" className="btn btn-secondary">👨‍⚕️ Manage Doctors</Link>
            <Link to="/admin/staff" className="btn btn-secondary">👥 Manage Staff</Link>
            <Link to="/admin/slots" className="btn btn-secondary">📅 Generate Slots</Link>
            <Link to="/admin/appointments" className="btn btn-secondary">📋 View Appointments</Link>
          </div>
        </div>

        <div className="card">
          <div className="card-header"><h2>System Info</h2></div>
          <div className="card-body">
            <table>
              <tbody>
                <tr>
                  <td className="text-muted">Your Role</td>
                  <td><span className="badge badge-blue">{user?.role}</span></td>
                </tr>
                <tr>
                  <td className="text-muted">Email</td>
                  <td>{user?.email || '—'}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  )
}
