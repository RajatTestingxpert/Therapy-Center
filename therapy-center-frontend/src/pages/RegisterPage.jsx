import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { auth } from '../api/api'
import { useAuth } from '../context/AuthContext'

export default function RegisterPage() {
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    phoneNumber: '',
    role: 'Patient',
  })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const { login } = useAuth()
  const navigate = useNavigate()

  const ROLE_HOME = { Patient: '/patient', Guardian: '/patient' }

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      const data = await auth.register(form)
      login(data)
      const role = data.role ?? data.Role
      navigate(ROLE_HOME[role] || '/')
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  function set(field) {
    return e => setForm({ ...form, [field]: e.target.value })
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-logo">🏥</div>
        <h1>Create Account</h1>
        <p>Register as a patient or guardian</p>

        {error && <div className="alert alert-error">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-row">
            <div className="form-group">
              <label>First Name</label>
              <input value={form.firstName} onChange={set('firstName')} placeholder="Jane" required />
            </div>
            <div className="form-group">
              <label>Last Name</label>
              <input value={form.lastName} onChange={set('lastName')} placeholder="Doe" required />
            </div>
          </div>
          <div className="form-group">
            <label>Email</label>
            <input type="email" value={form.email} onChange={set('email')} placeholder="you@example.com" required />
          </div>
          <div className="form-group">
            <label>Password</label>
            <input type="password" value={form.password} onChange={set('password')} placeholder="At least 8 characters" required />
          </div>
          <div className="form-group">
            <label>Phone Number</label>
            <input type="text" value={form.phoneNumber} onChange={set('phoneNumber')} placeholder="+1 555 000 0000" />
          </div>
          <div className="form-group">
            <label>I am a…</label>
            <select value={form.role} onChange={set('role')}>
              <option value="Patient">Patient</option>
              <option value="Guardian">Guardian (booking for a child/dependent)</option>
            </select>
          </div>

          <button className="btn btn-primary" style={{ width: '100%' }} disabled={loading}>
            {loading ? 'Creating account…' : 'Create Account'}
          </button>
        </form>

        <div className="auth-footer">
          Already have an account? <Link to="/login">Sign in</Link>
        </div>
      </div>
    </div>
  )
}
