import { useState, useEffect } from 'react'
import { admin, auth } from '../../api/api'
import Modal from '../../components/Modal'

const EMPTY = { firstName: '', lastName: '', email: '', password: '', phoneNumber: '', role: 'Receptionist' }

export default function ManageStaff() {
  const [receptionists, setReceptionists] = useState([])
  const [loading, setLoading] = useState(true)
  const [showModal, setShowModal] = useState(false)
  const [form, setForm] = useState(EMPTY)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [saving, setSaving] = useState(false)

  useEffect(() => { load() }, [])

  async function load() {
    setLoading(true)
    try {
      setReceptionists(await admin.getReceptionists() || [])
    } finally {
      setLoading(false)
    }
  }

  async function handleCreate() {
    setSaving(true)
    setError('')
    try {
      await auth.createStaff(form)
      setSuccess('Staff account created successfully.')
      setShowModal(false)
      load()
    } catch (e) {
      setError(e.message)
    } finally {
      setSaving(false)
    }
  }

  async function handleRemove(id) {
    if (!confirm('Deactivate this staff account?')) return
    setError('')
    try {
      await admin.deleteStaff(id)
      setSuccess('Staff account removed.')
      load()
    } catch (e) {
      setError(e.message)
    }
  }

  function set(field) {
    return e => setForm({ ...form, [field]: e.target.value })
  }

  return (
    <div>
      <div className="page-header flex-between">
        <div>
          <h1>Staff Management</h1>
          <p>Create and view receptionist accounts</p>
        </div>
        <button
          className="btn btn-primary"
          onClick={() => { setForm(EMPTY); setError(''); setShowModal(true) }}
        >
          + Add Staff
        </button>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      <div className="card">
        <div className="card-header"><h2>Receptionists</h2></div>
        <div className="table-wrapper">
          {loading ? (
            <div className="loading">Loading…</div>
          ) : receptionists.length === 0 ? (
            <div className="empty-state">
              <div className="empty-icon">👥</div>
              <p>No receptionists yet.</p>
            </div>
          ) : (
            <table>
              <thead>
                <tr><th>ID</th><th>Name</th><th>Email</th><th>Phone</th><th>Status</th><th>Actions</th></tr>
              </thead>
              <tbody>
                {receptionists.map(r => (
                  <tr key={r.userId}>
                    <td className="text-muted">{r.userId}</td>
                    <td className="fw-600">{r.firstName} {r.lastName}</td>
                    <td>{r.email}</td>
                    <td>{r.phoneNumber || '—'}</td>
                    <td>
                      <span className={`badge ${r.isActive ? 'badge-green' : 'badge-red'}`}>
                        {r.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td>
                      <button className="btn btn-danger btn-sm" onClick={() => handleRemove(r.userId)}>
                        Remove
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>

      {showModal && (
        <Modal
          title="Create Staff Account"
          onClose={() => setShowModal(false)}
          footer={
            <>
              <button className="btn btn-secondary" onClick={() => setShowModal(false)}>Cancel</button>
              <button className="btn btn-primary" onClick={handleCreate} disabled={saving}>
                {saving ? 'Creating…' : 'Create'}
              </button>
            </>
          }
        >
          {error && <div className="alert alert-error">{error}</div>}
          <div className="form-group">
            <label>Role *</label>
            <select value={form.role} onChange={set('role')}>
              <option value="Receptionist">Receptionist</option>
              <option value="Doctor">Doctor</option>
            </select>
          </div>
          <div className="form-row">
            <div className="form-group"><label>First Name *</label><input value={form.firstName} onChange={set('firstName')} /></div>
            <div className="form-group"><label>Last Name *</label><input value={form.lastName} onChange={set('lastName')} /></div>
          </div>
          <div className="form-group"><label>Email *</label><input type="email" value={form.email} onChange={set('email')} /></div>
          <div className="form-group"><label>Password *</label><input type="password" value={form.password} onChange={set('password')} /></div>
          <div className="form-group"><label>Phone</label><input value={form.phoneNumber} onChange={set('phoneNumber')} /></div>
        </Modal>
      )}
    </div>
  )
}