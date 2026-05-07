import { useState, useEffect, useMemo } from 'react'
import { doctors, auth, admin, therapies } from '../../api/api'
import Modal from '../../components/Modal'
import { doctorName, doctorSpecialization } from '../../utils/display'

const ACCOUNT_EMPTY = {
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  phoneNumber: '',
  role: 'Doctor',
}

const PROFILE_EMPTY = {
  userId: '',
  specialization: '',
  bio: '',
  availableDays: '',
  startTime: '',
  endTime: '',
}

function getTherapySpecializationOptions(therapyList) {
  const names = (therapyList || [])
    .map((t) => t.name ?? t.Name ?? '')
    .map((s) => String(s).trim())
    .filter(Boolean)

  return [...new Set(names)]
}

export default function ManageDoctors() {
  const [doctorList, setDoctorList] = useState([])
  const [loading, setLoading] = useState(true)
  const [showAccModal, setShowAccModal] = useState(false)
  const [showProfileModal, setShowProfileModal] = useState(false)
  const [accForm, setAccForm] = useState(ACCOUNT_EMPTY)
  const [profileForm, setProfileForm] = useState(PROFILE_EMPTY)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [saving, setSaving] = useState(false)
  const [therapyList, setTherapyList] = useState([])

  useEffect(() => {
    load()
  }, [])

  async function load() {
    setLoading(true)
    try {
      const [docs, therap] = await Promise.all([
        doctors.getAll().catch(() => []),
        therapies.getAll().catch(() => []),
      ])
      setDoctorList(docs || [])
      setTherapyList(therap || [])
    } finally {
      setLoading(false)
    }
  }

  async function handleCreateAccount() {
    setSaving(true)
    setError('')
    setSuccess('')

    try {
      const result = await auth.createStaff({ ...accForm, role: 'Doctor' })
      const userId = result.userId ?? result.UserId

      setSuccess(`Doctor account created. UserId: ${userId}. Now create their profile.`)
      setProfileForm({ ...PROFILE_EMPTY, userId })
      setShowAccModal(false)
      setShowProfileModal(true)
    } catch (e) {
      setError(e.message)
    } finally {
      setSaving(false)
    }
  }

  async function handleCreateProfile() {
    setSaving(true)
    setError('')
    setSuccess('')

    try {
      const body = {
        ...profileForm,
        userId: Number(profileForm.userId),
        startTime: profileForm.startTime || null,
        endTime: profileForm.endTime || null,
        specialization: profileForm.specialization || null,
      }

      await admin.createDoctorProfile(body)
      setSuccess('Doctor profile created successfully.')
      setShowProfileModal(false)
      await load()
    } catch (e) {
      setError(e.message)
    } finally {
      setSaving(false)
    }
  }

  async function handleDeleteDoctor(id) {
  const ok = window.confirm('Delete this doctor?')
  
  if (!ok) return

  setError('')
  setSuccess('')

  try {
    await admin.deleteDoctor(id)

    setDoctorList((prev) =>
      prev.filter((d) => (d.doctorId ?? d.DoctorId) !== id)
    )

    setSuccess('Doctor deleted successfully.')
  } catch (e) {
    const msg =
      e?.response?.data?.message ||
      e?.message ||
      'Failed to delete doctor'

    setError(msg)
  }
}

  function setA(field) {
    return (e) => setAccForm({ ...accForm, [field]: e.target.value })
  }

  function setP(field) {
    return (e) => setProfileForm({ ...profileForm, [field]: e.target.value })
  }

  const specializationOptions = useMemo(() => {
    const options = getTherapySpecializationOptions(therapyList)
    if (profileForm.specialization && !options.includes(profileForm.specialization)) {
      return [profileForm.specialization, ...options]
    }
    return options
  }, [therapyList, profileForm.specialization])

  const DAYS = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']

  function toggleDay(day) {
    const current = profileForm.availableDays
      ? profileForm.availableDays.split(',').filter(Boolean)
      : []
    const updated = current.includes(day)
      ? current.filter((d) => d !== day)
      : [...current, day]
    setProfileForm({ ...profileForm, availableDays: updated.join(',') })
  }

  const selectedDays = profileForm.availableDays
    ? profileForm.availableDays.split(',').filter(Boolean)
    : []

  return (
    <div>
      <div className="page-header flex-between">
        <div>
          <h1>Doctors</h1>
          <p>Manage doctor accounts and profiles</p>
        </div>

        <div className="flex gap-8">
          <button
            className="btn btn-primary"
            onClick={() => {
              setAccForm(ACCOUNT_EMPTY)
              setError('')
              setSuccess('')
              setShowAccModal(true)
            }}
          >
            + Add Doctor Account
          </button>
        </div>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      <div className="card">
        <div className="table-wrapper">
          {loading ? (
            <div className="loading">Loading doctors…</div>
          ) : doctorList.length === 0 ? (
            <div className="empty-state">
              <div className="empty-icon">👨‍⚕️</div>
              <p>No doctors yet.</p>
            </div>
          ) : (
            <table>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Name</th>
                  <th>Specialization</th>
                  <th>Available Days</th>
                  <th>Hours</th>
                  <th>Bio</th>
                  <th>Actions</th>
                </tr>
              </thead>

              <tbody>
                {doctorList.map((d) => {
                  const id = d.doctorId ?? d.DoctorId
                  return (
                    <tr key={id}>
                      <td className="text-muted">{id}</td>
                      <td className="fw-600">{doctorName(d)}</td>
                      <td>{doctorSpecialization(d)}</td>
                      <td>{d.availableDays ?? d.AvailableDays ?? '—'}</td>
                      <td>
                        {(d.startTime ?? d.StartTime) && (d.endTime ?? d.EndTime)
                          ? `${d.startTime ?? d.StartTime} – ${d.endTime ?? d.EndTime}`
                          : '—'}
                      </td>
                      <td
                        className="text-muted"
                        style={{
                          maxWidth: 200,
                          whiteSpace: 'nowrap',
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                        }}
                      >
                        {d.bio ?? d.Bio ?? '—'}
                      </td>
                      <td>
                        <button
                          className="btn btn-danger btn-sm"
                          onClick={() => handleDeleteDoctor(id)}
                        >
                          Delete
                        </button>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          )}
        </div>
      </div>

      {showAccModal && (
        <Modal
          title="Create Doctor Account"
          onClose={() => setShowAccModal(false)}
          footer={
            <>
              <button className="btn btn-secondary" onClick={() => setShowAccModal(false)}>
                Cancel
              </button>
              <button
                className="btn btn-primary"
                onClick={handleCreateAccount}
                disabled={saving}
              >
                {saving ? 'Creating…' : 'Create Account'}
              </button>
            </>
          }
        >
          {error && <div className="alert alert-error">{error}</div>}
          <div className="form-row">
            <div className="form-group">
              <label>First Name *</label>
              <input value={accForm.firstName} onChange={setA('firstName')} />
            </div>
            <div className="form-group">
              <label>Last Name *</label>
              <input value={accForm.lastName} onChange={setA('lastName')} />
            </div>
          </div>

          <div className="form-group">
            <label>Email *</label>
            <input type="email" value={accForm.email} onChange={setA('email')} />
          </div>

          <div className="form-group">
            <label>Password *</label>
            <input type="password" value={accForm.password} onChange={setA('password')} />
          </div>

          <div className="form-group">
            <label>Phone</label>
            <input value={accForm.phoneNumber} onChange={setA('phoneNumber')} />
          </div>

          <div className="alert alert-info">
            After creating the account you'll be prompted to fill in the doctor profile details.
          </div>
        </Modal>
      )}

      {showProfileModal && (
        <Modal
          title="Create Doctor Profile"
          onClose={() => setShowProfileModal(false)}
          footer={
            <>
              <button className="btn btn-secondary" onClick={() => setShowProfileModal(false)}>
                Cancel
              </button>
              <button
                className="btn btn-primary"
                onClick={handleCreateProfile}
                disabled={saving}
              >
                {saving ? 'Saving…' : 'Save Profile'}
              </button>
            </>
          }
        >
          {error && <div className="alert alert-error">{error}</div>}

          <div className="form-group">
            <label>User ID *</label>
            <input type="number" value={profileForm.userId} onChange={setP('userId')} />
          </div>

          <div className="form-group">
            <label>Specialization *</label>
            <select value={profileForm.specialization} onChange={setP('specialization')} required>
              <option value="">Select specialization…</option>
              {specializationOptions.map((spec) => (
                <option key={spec} value={spec}>
                  {spec}
                </option>
              ))}
            </select>
            <div className="text-muted" style={{ fontSize: 12, marginTop: 6 }}>
              This list comes from the Therapy catalog.
            </div>
          </div>

          <div className="form-group">
            <label>Bio</label>
            <textarea value={profileForm.bio} onChange={setP('bio')} />
          </div>

          <div className="form-group">
            <label>Available Days</label>
            <div className="flex gap-8" style={{ flexWrap: 'wrap', marginTop: 6 }}>
              {DAYS.map((day) => (
                <button
                  key={day}
                  type="button"
                  className={`btn btn-sm ${selectedDays.includes(day) ? 'btn-primary' : 'btn-secondary'}`}
                  onClick={() => toggleDay(day)}
                >
                  {day}
                </button>
              ))}
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Start Time</label>
              <input type="time" value={profileForm.startTime} onChange={setP('startTime')} />
            </div>
            <div className="form-group">
              <label>End Time</label>
              <input type="time" value={profileForm.endTime} onChange={setP('endTime')} />
            </div>
          </div>
        </Modal>
      )}
    </div>
  )
}