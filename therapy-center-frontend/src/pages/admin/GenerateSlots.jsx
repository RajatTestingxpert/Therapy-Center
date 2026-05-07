import { useState, useEffect } from 'react'
import { admin, doctors } from '../../api/api'
import { doctorName, doctorSpecialization } from '../../utils/display'

export default function GenerateSlots() {
  const [doctorList, setDoctorList] = useState([])
  const [form, setForm] = useState({ doctorId: '', fromDate: '', toDate: '' })
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  useEffect(() => {
    doctors.getAll().then(d => setDoctorList(d || [])).finally(() => setLoading(false))
  }, [])

  async function handleGenerate(e) {
    e.preventDefault()
    setSaving(true)
    setError('')
    setSuccess('')
    try {
      const body = {
        doctorId: Number(form.doctorId),
        fromDate: form.fromDate,
        toDate: form.toDate,
      }
      const res = await admin.generateSlots(body)
      setSuccess(res?.message || 'Slots generated successfully.')
    } catch (e) {
      setError(e.message)
    } finally {
      setSaving(false)
    }
  }

  function set(field) { return e => setForm({ ...form, [field]: e.target.value }) }

  return (
    <div>
      <div className="page-header">
        <h1>Generate Slots</h1>
        <p>Generate appointment slots for a doctor over a date range</p>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      <div className="card" style={{ maxWidth: 500 }}>
        <div className="card-header"><h2>Slot Generator</h2></div>
        <div className="card-body">
          {loading ? (
            <div className="loading">Loading doctors…</div>
          ) : (
            <form onSubmit={handleGenerate}>
              <div className="form-group">
                <label>Doctor *</label>
                <select value={form.doctorId} onChange={set('doctorId')} required>
                  <option value="">Select a doctor…</option>
                  {doctorList.map(d => (
                    <option key={d.doctorId ?? d.DoctorId} value={d.doctorId ?? d.DoctorId}>
                      {doctorName(d)} — {doctorSpecialization(d)}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label>From Date *</label>
                  <input type="date" value={form.fromDate} onChange={set('fromDate')} required />
                </div>
                <div className="form-group">
                  <label>To Date *</label>
                  <input type="date" value={form.toDate} onChange={set('toDate')} required />
                </div>
              </div>
              <div className="alert alert-info" style={{ marginBottom: 14 }}>
                Slots will be generated based on the doctor's configured available days and working hours.
              </div>
              <button className="btn btn-primary" disabled={saving}>
                {saving ? 'Generating…' : '📅 Generate Slots'}
              </button>
            </form>
          )}
        </div>
      </div>
    </div>
  )
}
