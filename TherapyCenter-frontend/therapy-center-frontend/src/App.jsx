import { Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider, useAuth } from './context/AuthContext'
import Sidebar from './components/Sidebar'
import ProtectedRoute from './components/ProtectedRoute'

// Auth Pages
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'

// Admin Pages
import AdminDashboard from './pages/admin/AdminDashboard'
import ManageTherapies from './pages/admin/ManageTherapies'
import ManageDoctors from './pages/admin/ManageDoctors'
import ManageStaff from './pages/admin/ManageStaff'
import GenerateSlots from './pages/admin/GenerateSlots'
import AdminPatients from './pages/admin/AdminPatients'
import AdminAppointments from './pages/admin/AdminAppointments'

// Doctor Pages
import DoctorDashboard from './pages/doctor/DoctorDashboard'
import DoctorAppointments from './pages/doctor/DoctorAppointments'

// Staff Pages
import StaffDashboard from './pages/staff/StaffDashboard'
import StaffPatients from './pages/staff/StaffPatients'
import StaffAppointments from './pages/staff/StaffAppointments'

// Patient Pages
import PatientDashboard from './pages/patient/PatientDashboard'
import PatientAppointments from './pages/patient/PatientAppointments'

// Shared
import BookAppointment from './pages/BookAppointment'

function RootRedirect() {
  const { user } = useAuth()

  if (!user) {
    return <Navigate to="/login" replace />
  }

  const map = {
    Admin: '/admin',
    Receptionist: '/staff',
    Doctor: '/doctor',
    Patient: '/patient',
    Guardian: '/patient',
  }

  return <Navigate to={map[user.role] || '/login'} replace />
}

function AppLayout() {
  const { user } = useAuth()

  if (!user) return null

  return (
    <div className="layout">
      <Sidebar />

      <main className="main-content">
        <Routes>

          {/* ================= ADMIN ================= */}

          <Route
            path="/admin"
            element={
              <ProtectedRoute roles={['Admin']}>
                <AdminDashboard />
              </ProtectedRoute>
            }
          />

          <Route
            path="/admin/therapies"
            element={
              <ProtectedRoute roles={['Admin']}>
                <ManageTherapies />
              </ProtectedRoute>
            }
          />

          <Route
            path="/admin/doctors"
            element={
              <ProtectedRoute roles={['Admin']}>
                <ManageDoctors />
              </ProtectedRoute>
            }
          />

          <Route
            path="/admin/staff"
            element={
              <ProtectedRoute roles={['Admin']}>
                <ManageStaff />
              </ProtectedRoute>
            }
          />

          <Route
            path="/admin/slots"
            element={
              <ProtectedRoute roles={['Admin']}>
                <GenerateSlots />
              </ProtectedRoute>
            }
          />

          <Route
            path="/admin/patients"
            element={
              <ProtectedRoute roles={['Admin']}>
                <AdminPatients />
              </ProtectedRoute>
            }
          />

          <Route
            path="/admin/appointments"
            element={
              <ProtectedRoute roles={['Admin']}>
                <AdminAppointments />
              </ProtectedRoute>
            }
          />

          {/* ================= DOCTOR ================= */}

          <Route
            path="/doctor"
            element={
              <ProtectedRoute roles={['Doctor']}>
                <DoctorDashboard />
              </ProtectedRoute>
            }
          />

          <Route
            path="/doctor/appointments"
            element={
              <ProtectedRoute roles={['Doctor']}>
                <DoctorAppointments />
              </ProtectedRoute>
            }
          />

          {/* ================= STAFF ================= */}

          <Route
            path="/staff"
            element={
              <ProtectedRoute roles={['Receptionist']}>
                <StaffDashboard />
              </ProtectedRoute>
            }
          />

          <Route
            path="/staff/patients"
            element={
              <ProtectedRoute roles={['Receptionist']}>
                <StaffPatients />
              </ProtectedRoute>
            }
          />

          <Route
            path="/staff/appointments"
            element={
              <ProtectedRoute roles={['Receptionist']}>
                <StaffAppointments />
              </ProtectedRoute>
            }
          />

          <Route
            path="/staff/book"
            element={
              <ProtectedRoute roles={['Receptionist']}>
                <BookAppointment isOnline={false} />
              </ProtectedRoute>
            }
          />

          {/* ================= PATIENT / GUARDIAN ================= */}

          <Route
            path="/patient"
            element={
              <ProtectedRoute roles={['Patient', 'Guardian']}>
                <PatientDashboard />
              </ProtectedRoute>
            }
          />

          <Route
            path="/patient/appointments"
            element={
              <ProtectedRoute roles={['Patient', 'Guardian']}>
                <PatientAppointments />
              </ProtectedRoute>
            }
          />

          <Route
            path="/patient/book"
            element={
              <ProtectedRoute roles={['Patient', 'Guardian']}>
                <BookAppointment isOnline={true} />
              </ProtectedRoute>
            }
          />

          {/* ================= FALLBACK ================= */}

          <Route path="*" element={<Navigate to="/" replace />} />

        </Routes>
      </main>
    </div>
  )
}

function AuthGate() {
  const { user, loading } = useAuth()

  if (loading) {
    return <div className="loading">Loading…</div>
  }

  if (!user) {
    return <Navigate to="/login" replace />
  }

  return <AppLayout />
}

export default function App() {
  return (
    <AuthProvider>
      <Routes>

        {/* Public Routes */}

        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />

        {/* Root Redirect */}

        <Route path="/" element={<RootRedirect />} />

        {/* Protected App */}

        <Route path="/*" element={<AuthGate />} />

      </Routes>
    </AuthProvider>
  )
}