import { useState, useEffect } from 'react'
import { patients } from '../../api/api'
import Modal from '../../components/Modal'

const EMPTY = { firstName: '', lastName: '', dateOfBirth: '', gender: '', medicalHistory: '', guardianId: '' }

export default function StaffPatients() {
  const [list, setList] = useState([])
  const [loading, setLoading] = useState(true)
  const [showModal, setShowModal] = useState(false)
  const [form, setForm] = useState(EMPTY)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [saving, setSaving] = useState(false)
  const [search, setSearch] = useState('')

  useEffect(() => { load() }, [])

  async function load() {
    setLoading(true)
    try { setList(await patients.getAll() || []) } finally { setLoading(false) }
  }

  async function handleCreate() {
    setSaving(true)
    setError('')
    try {
      await patients.create({
        ...form,
        dateOfBirth: form.dateOfBirth || null,
        guardianId: form.guardianId ? Number(form.guardianId) : null,
      })
      setSuccess('Patient created successfully.')
      setShowModal(false)
      load()
    } catch (e) {
      setError(e.message)
    } finally {
      setSaving(false)
    }
  }

  function set(field) { return e => setForm({ ...form, [field]: e.target.value }) }
  const filtered = list.filter(p => `${p.firstName} ${p.lastName}`.toLowerCase().includes(search.toLowerCase()))

  return (
    <div>
      <div className="page-header flex-between">
        <div><h1>Patients</h1><p>Manage patient records</p></div>
        <button className="btn btn-primary" onClick={() => { setForm(EMPTY); setError(''); setShowModal(true) }}>+ New Patient</button>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      <div className="mb-16">
        <input value={search} onChange={e => setSearch(e.target.value)} placeholder="Search patients…" style={{ maxWidth: 300 }} />
      </div>

      <div className="card">
        <div className="table-wrapper">
          {loading ? (
            <div className="loading">Loading…</div>
          ) : filtered.length === 0 ? (
            <div className="empty-state"><div className="empty-icon">🧑‍🤝‍🧑</div><p>No patients found.</p></div>
          ) : (
            <table>
              <thead>
                <tr><th>ID</th><th>Name</th><th>DOB</th><th>Gender</th><th>Medical History</th></tr>
              </thead>
              <tbody>
                {filtered.map(p => (
                  <tr key={p.patientId}>
                    <td className="text-muted">{p.patientId}</td>
                    <td className="fw-600">{p.firstName} {p.lastName}</td>
                    <td>{p.dateOfBirth ? new Date(p.dateOfBirth).toLocaleDateString() : '—'}</td>
                    <td>{p.gender || '—'}</td>
                    <td className="text-muted" style={{ maxWidth: 200 }}>{p.medicalHistory || '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>

      {showModal && (
        <Modal title="New Patient" onClose={() => setShowModal(false)}
          footer={
            <>
              <button className="btn btn-secondary" onClick={() => setShowModal(false)}>Cancel</button>
              <button className="btn btn-primary" onClick={handleCreate} disabled={saving}>{saving ? 'Saving…' : 'Create'}</button>
            </>
          }
        >
          {error && <div className="alert alert-error">{error}</div>}
          <div className="form-row">
            <div className="form-group"><label>First Name *</label><input value={form.firstName} onChange={set('firstName')} /></div>
            <div className="form-group"><label>Last Name *</label><input value={form.lastName} onChange={set('lastName')} /></div>
          </div>
          <div className="form-row">
            <div className="form-group"><label>Date of Birth</label><input type="date" value={form.dateOfBirth} onChange={set('dateOfBirth')} /></div>
            <div className="form-group">
              <label>Gender</label>
              <select value={form.gender} onChange={set('gender')}>
                <option value="">Select…</option>
                <option>Male</option><option>Female</option><option>Other</option>
              </select>
            </div>
          </div>
          <div className="form-group"><label>Guardian User ID (optional)</label><input type="number" value={form.guardianId} onChange={set('guardianId')} /></div>
          <div className="form-group"><label>Medical History</label><textarea value={form.medicalHistory} onChange={set('medicalHistory')} /></div>
        </Modal>
      )}
    </div>
  )
}
