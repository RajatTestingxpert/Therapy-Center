import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { auth } from '../api/api'
import { useAuth } from '../context/AuthContext'

const ROLE_HOME = {
  Admin: '/admin',
  Receptionist: '/staff',
  Doctor: '/doctor',
  Patient: '/patient',
  Guardian: '/patient',
}

export default function LoginPage() {
  const [form, setForm] = useState({ email: '', password: '' })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const { login } = useAuth()
  const navigate = useNavigate()

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      const data = await auth.login(form)
      login(data)
      const role = data.role ?? data.Role
      navigate(ROLE_HOME[role] || '/')
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-logo">🏥</div>
        <h1>TherapyCenter</h1>
        <p>Sign in to your account</p>

        {error && <div className="alert alert-error">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Email</label>
            <input
              type="email"
              value={form.email}
              onChange={e => setForm({ ...form, email: e.target.value })}
              placeholder="you@example.com"
              required
            />
          </div>
          <div className="form-group">
            <label>Password</label>
            <input
              type="password"
              value={form.password}
              onChange={e => setForm({ ...form, password: e.target.value })}
              placeholder="••••••••"
              required
            />
          </div>
          <button className="btn btn-primary" style={{ width: '100%' }} disabled={loading}>
            {loading ? 'Signing in…' : 'Sign In'}
          </button>
        </form>

        <div className="auth-footer">
          New patient? <Link to="/register">Create an account</Link>
        </div>
      </div>
    </div>
  )
}
