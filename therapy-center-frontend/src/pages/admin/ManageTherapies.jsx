import { useState, useEffect } from 'react'
import { admin } from '../../api/api'
import Modal from '../../components/Modal'

const EMPTY = { name: '', description: '', durationMinutes: 30, cost: 0 }

export default function ManageTherapies() {
  const [therapies, setTherapies] = useState([])
  const [loading, setLoading] = useState(true)
  const [showModal, setShowModal] = useState(false)
  const [editing, setEditing] = useState(null)   // null = create, object = edit
  const [form, setForm] = useState(EMPTY)
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)
  const [success, setSuccess] = useState('')

  useEffect(() => { load() }, [])

  async function load() {
    setLoading(true)
    try {
      const data = await admin.getTherapies()
      setTherapies(data || [])
    } catch (e) {
      setError(e.message)
    } finally {
      setLoading(false)
    }
  }

  function openCreate() {
    setEditing(null)
    setForm(EMPTY)
    setError('')
    setShowModal(true)
  }

  function openEdit(therapy) {
    setEditing(therapy)
    setForm({
      name: therapy.name,
      description: therapy.description || '',
      durationMinutes: therapy.durationMinutes,
      cost: therapy.cost,
    })
    setError('')
    setShowModal(true)
  }

  async function handleSave() {
    setSaving(true)
    setError('')
    try {
      if (editing) {
        await admin.updateTherapy(editing.therapyId, form)
        setSuccess('Therapy updated.')
      } else {
        await admin.createTherapy({ ...form, durationMinutes: Number(form.durationMinutes), cost: Number(form.cost) })
        setSuccess('Therapy created.')
      }
      setShowModal(false)
      load()
    } catch (e) {
      setError(e.message)
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete(id) {
    if (!confirm('Delete this therapy?')) return
    try {
      await admin.deleteTherapy(id)
      setSuccess('Therapy deleted.')
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
          <h1>Therapies</h1>
          <p>Manage available therapy types</p>
        </div>
        <button className="btn btn-primary" onClick={openCreate}>+ New Therapy</button>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      <div className="card">
        <div className="table-wrapper">
          {loading ? (
            <div className="loading">Loading therapies…</div>
          ) : therapies.length === 0 ? (
            <div className="empty-state">
              <div className="empty-icon">💊</div>
              <p>No therapies yet. Create one to get started.</p>
            </div>
          ) : (
            <table>
              <thead>
                <tr>
                  <th>#</th>
                  <th>Name</th>
                  <th>Description</th>
                  <th>Duration</th>
                  <th>Cost</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {therapies.map(t => (
                  <tr key={t.therapyId}>
                    <td className="text-muted">{t.therapyId}</td>
                    <td className="fw-600">{t.name}</td>
                    <td className="text-muted">{t.description || '—'}</td>
                    <td>{t.durationMinutes} min</td>
                    <td>Rs.{Number(t.cost).toFixed(2)}</td>
                    <td>
                      <div className="flex gap-8">
                        <button className="btn btn-secondary btn-sm" onClick={() => openEdit(t)}>Edit</button>
                        <button className="btn btn-danger btn-sm" onClick={() => handleDelete(t.therapyId)}>Delete</button>
                      </div>
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
          title={editing ? 'Edit Therapy' : 'New Therapy'}
          onClose={() => setShowModal(false)}
          footer={
            <>
              <button className="btn btn-secondary" onClick={() => setShowModal(false)}>Cancel</button>
              <button className="btn btn-primary" onClick={handleSave} disabled={saving}>
                {saving ? 'Saving…' : 'Save'}
              </button>
            </>
          }
        >
          {error && <div className="alert alert-error">{error}</div>}
          <div className="form-group">
            <label>Name *</label>
            <input value={form.name} onChange={set('name')} placeholder="Cognitive Behavioural Therapy" required />
          </div>
          <div className="form-group">
            <label>Description</label>
            <textarea value={form.description} onChange={set('description')} placeholder="Short description…" />
          </div>
          <div className="form-row">
            <div className="form-group">
              <label>Duration (minutes) *</label>
              <input type="number" min="1" value={form.durationMinutes} onChange={set('durationMinutes')} />
            </div>
            <div className="form-group">
              <label>Cost (Rs) *</label>
              <input type="number" min="0" step="0.01" value={form.cost} onChange={set('cost')} />
            </div>
          </div>
        </Modal>
      )}
    </div>
  )
}
