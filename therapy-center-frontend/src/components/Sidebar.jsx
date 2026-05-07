import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const ROLE_LINKS = {
  Admin: [
    { to: '/admin', label: 'Dashboard', icon: '🏠' },
    { to: '/admin/therapies', label: 'Therapies', icon: '💊' },
    { to: '/admin/doctors', label: 'Doctors', icon: '👨‍⚕️' },
    { to: '/admin/staff', label: 'Staff', icon: '👥' },
    { to: '/admin/slots', label: 'Generate Slots', icon: '📅' },
    { to: '/admin/patients', label: 'Patients', icon: '🧑‍🤝‍🧑' },
    { to: '/admin/appointments', label: 'Appointments', icon: '📋' },
  ],
  Receptionist: [
    { to: '/staff', label: 'Dashboard', icon: '🏠' },
    { to: '/staff/patients', label: 'Patients', icon: '🧑‍🤝‍🧑' },
    { to: '/staff/appointments', label: 'Appointments', icon: '📋' },
    { to: '/staff/book', label: 'Book Appt', icon: '➕' },
  ],
  Doctor: [
    { to: '/doctor', label: 'Dashboard', icon: '🏠' },
    { to: '/doctor/appointments', label: 'My Schedule', icon: '📋' },
  ],
  Patient: [
    { to: '/patient', label: 'Dashboard', icon: '🏠' },
    { to: '/patient/appointments', label: 'My Appointments', icon: '📋' },
    { to: '/patient/book', label: 'Book Online', icon: '➕' },
  ],
  Guardian: [
    { to: '/patient', label: 'Dashboard', icon: '🏠' },
    { to: '/patient/appointments', label: 'Appointments', icon: '📋' },
    { to: '/patient/book', label: 'Book Online', icon: '➕' },
  ],
}

export default function Sidebar() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  if (!user) return null

  const links = ROLE_LINKS[user.role] || []

  function handleLogout() {
    logout()
    navigate('/login')
  }

  return (
    <aside className="sidebar">
      <div className="sidebar-logo">
        <span>🏥</span>
        <span>TherapyCenter</span>
      </div>

      <nav className="sidebar-nav">
        <div className="sidebar-section">Navigation</div>
        {links.map(link => (
          <NavLink
            key={link.to}
            to={link.to}
            end={link.to.split('/').length <= 2}
            className={({ isActive }) => 'sidebar-link' + (isActive ? ' active' : '')}
          >
            <span className="icon">{link.icon}</span>
            <span>{link.label}</span>
          </NavLink>
        ))}
      </nav>

      <div className="sidebar-footer">
        <div className="sidebar-user">{user.fullName} · {user.role}</div>
        <button className="sidebar-link btn-danger" style={{ borderRadius: 6 }} onClick={handleLogout}>
          <span className="icon">🚪</span>
          <span>Logout</span>
        </button>
      </div>
    </aside>
  )
}
