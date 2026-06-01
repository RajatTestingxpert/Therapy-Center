import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import Modal from '../../components/Modal'
import { appointments, patients } from '../../api/api'
import { doctorName, therapyName, patientName } from '../../utils/display'
import { useAuth } from '../../context/AuthContext'

function statusBadge(status) {
  const map = { Scheduled: 'badge-blue', Completed: 'badge-green', Cancelled: 'badge-red' }
  return <span className={`badge ${map[status] || 'badge-gray'}`}>{status}</span>
}

const EMPTY_PATIENT_FORM = {
  firstName: '',
  lastName: '',
  dateOfBirth: '',
  gender: '',
  medicalHistory: '',
}

export default function PatientDashboard() {
  const { user } = useAuth()
  const [appts, setAppts] = useState([])
  const [loading, setLoading] = useState(true)
  const [selectedPatientId, setSelectedPatientId] = useState('')
  const [patientList, setPatientList] = useState([])
  const [error, setError] = useState('')

  const [showPatientModal, setShowPatientModal] = useState(false)
  const [savingPatient, setSavingPatient] = useState(false)
  const [patientForm, setPatientForm] = useState(EMPTY_PATIENT_FORM)

  const isPatient = user?.role === 'Patient'
  const isGuardian = user?.role === 'Guardian'

  useEffect(() => {
    async function init() {
      try {
        if (isPatient) {
          const me = await patients.getMe()
          setPatientList(me ? [me] : [])
          setSelectedPatientId(me?.patientId ? String(me.patientId) : '')
          if (me?.patientId) {
            setAppts((await appointments.getByPatient(me.patientId)) || [])
          }
        } else if (isGuardian) {
          const list = await patients.getByGuardian(user.userId)
          setPatientList(list || [])
          const firstId = list?.[0]?.patientId
          setSelectedPatientId(firstId ? String(firstId) : '')
          if (firstId) {
            setAppts((await appointments.getByPatient(firstId)) || [])
          }
        }
      } catch (e) {
        setError(e.message)
      } finally {
        setLoading(false)
      }
    }
    init()
  }, [isPatient, isGuardian, user])

  async function load() {
    if (!selectedPatientId) return
    setLoading(true)
    setError('')
    try {
      setAppts((await appointments.getByPatient(selectedPatientId)) || [])
    } catch (e) {
      setError(e.message)
    } finally {
      setLoading(false)
    }
  }

  async function handleCreatePatient() {
    setSavingPatient(true)
    setError('')
    try {
      const body = {
        ...patientForm,
        dateOfBirth: patientForm.dateOfBirth || null,
        guardianId: user.userId,
      }

      await patients.create(body)

      const list = await patients.getByGuardian(user.userId)
      setPatientList(list || [])

      const newlyCreated = list?.[list.length - 1]
      if (newlyCreated?.patientId) {
        setSelectedPatientId(String(newlyCreated.patientId))
        setAppts((await appointments.getByPatient(newlyCreated.patientId)) || [])
      }

      setShowPatientModal(false)
      setPatientForm(EMPTY_PATIENT_FORM)
    } catch (e) {
      setError(e.message)
    } finally {
      setSavingPatient(false)
    }
  }

  function setPatientField(field) {
    return (e) => setPatientForm((prev) => ({ ...prev, [field]: e.target.value }))
  }

  const scheduled = appts.filter(a => a.status === 'Scheduled')

  return (
    <div>
      <div className="page-header">
        <h1>My Appointments</h1>
        <p>Welcome, {user?.fullName}</p>
      </div>

      {error && <div className="alert alert-error">{error}</div>}

      {!isPatient && (
        <div className="card mb-24" style={{ maxWidth: 560 }}>
          <div className="card-header"><h2>Select Patient</h2></div>
          <div className="card-body">
            <div className="flex gap-8" style={{ alignItems: 'flex-end' }}>
              <div className="form-group" style={{ flex: 1, margin: 0 }}>
                <label>Patient</label>
                <select value={selectedPatientId} onChange={e => setSelectedPatientId(e.target.value)}>
                  <option value="">Select patient…</option>
                  {patientList.map(p => (
                    <option key={p.patientId} value={p.patientId}>
                      {patientName(p)}
                    </option>
                  ))}
                </select>
              </div>
              <button className="btn btn-primary" onClick={load} disabled={loading || !selectedPatientId}>
                {loading ? 'Loading…' : 'Search'}
              </button>
            </div>
          </div>
        </div>
      )}

      {isGuardian && (
        <div className="card mb-24" style={{ maxWidth: 560 }}>
          <div className="card-header flex-between">
            <h2>Your Patients</h2>
            <button
              className="btn btn-primary btn-sm"
              onClick={() => {
                setPatientForm(EMPTY_PATIENT_FORM)
                setShowPatientModal(true)
              }}
            >
              + Add Patient Profile
            </button>
          </div>
          <div className="card-body">
            <div className="text-muted" style={{ marginBottom: 10 }}>
              Add child/patient profiles under your guardian account.
            </div>
            <div className="form-group" style={{ marginBottom: 0 }}>
              <label>Linked Patients</label>
              <select value={selectedPatientId} onChange={e => setSelectedPatientId(e.target.value)}>
                <option value="">Select patient…</option>
                {patientList.map(p => (
                  <option key={p.patientId} value={p.patientId}>
                    {patientName(p)}
                  </option>
                ))}
              </select>
            </div>
            <div style={{ marginTop: 12 }}>
              <button className="btn btn-secondary btn-sm" onClick={load} disabled={loading || !selectedPatientId}>
                {loading ? 'Loading…' : 'Load Appointments'}
              </button>
            </div>
          </div>
        </div>
      )}

      {isPatient && (
        <div className="card mb-24" style={{ maxWidth: 560 }}>
          <div className="card-header"><h2>Your Record</h2></div>
          <div className="card-body">
            <div className="text-muted">
              Loaded patient profile: <strong>{patientList[0] ? patientName(patientList[0]) : '—'}</strong>
            </div>
          </div>
        </div>
      )}

      {appts.length > 0 && (
        <div className="stats-grid mb-24">
          <div className="stat-card">
            <div className="stat-icon">📋</div>
            <div className="stat-label">Total</div>
            <div className="stat-value">{appts.length}</div>
          </div>
          <div className="stat-card">
            <div className="stat-icon">⏳</div>
            <div className="stat-label">Scheduled</div>
            <div className="stat-value">{scheduled.length}</div>
          </div>
          <div className="stat-card">
            <div className="stat-icon">✅</div>
            <div className="stat-label">Completed</div>
            <div className="stat-value">{appts.filter(a => a.status === 'Completed').length}</div>
          </div>
        </div>
      )}

      <div className="flex-between mb-16">
        <h2 style={{ fontSize: 16, fontWeight: 600 }}>Appointment History</h2>
        <Link to="/patient/book" className="btn btn-primary btn-sm">+ Book New</Link>
      </div>

      <div className="card">
        <div className="table-wrapper">
          {appts.length === 0 ? (
            <div className="empty-state">
              <div className="empty-icon">📋</div>
              <p>{isPatient ? 'No appointments found for your record.' : 'Select a patient above to view appointments.'}</p>
            </div>
          ) : (
            <table>
              <thead>
                <tr>
                  <th>#</th>
                  <th>Date</th>
                  <th>Doctor</th>
                  <th>Therapy</th>
                  <th>Time</th>
                  <th>Status</th>
                  <th>Payment</th>
                  <th>Report</th>
                </tr>
              </thead>
              <tbody>
                {appts.map(a => {
                  const paymentStatus = a.payment?.status ?? a.payment?.Status ?? 'Pending'
                  const report = a.finding

                  return (
                    <tr key={a.appointmentId}>
                      <td className="text-muted">{a.appointmentId}</td>
                      <td>{a.appointmentDate}</td>
                      <td className="fw-600">Dr. {doctorName(a.doctor)}</td>
                      <td>{therapyName(a.therapy)}</td>
                      <td>{a.startTime}</td>
                      <td>{statusBadge(a.status)}</td>
                      <td>
                        <span
                          className={`badge ${
                            paymentStatus === 'Paid' ? 'badge-green' : 'badge-yellow'
                          }`}
                        >
                          {paymentStatus}
                        </span>
                      </td>
                      <td>
                        {report ? (
                          <div style={{ fontSize: 12 }}>
                            <div><strong>Obs:</strong> {report.observations || '—'}</div>
                            <div><strong>Rec:</strong> {report.recommendations || '—'}</div>
                            <div><strong>Next:</strong> {report.nextSessionDate || '—'}</div>
                          </div>
                        ) : (
                          '—'
                        )}
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          )}
        </div>
      </div>

      {showPatientModal && (
        <Modal
          title="Add Patient Profile"
          onClose={() => setShowPatientModal(false)}
          footer={
            <>
              <button className="btn btn-secondary" onClick={() => setShowPatientModal(false)}>
                Cancel
              </button>
              <button className="btn btn-primary" onClick={handleCreatePatient} disabled={savingPatient}>
                {savingPatient ? 'Saving…' : 'Save Patient'}
              </button>
            </>
          }
        >
          <div className="form-row">
            <div className="form-group">
              <label>First Name *</label>
              <input value={patientForm.firstName} onChange={setPatientField('firstName')} />
            </div>
            <div className="form-group">
              <label>Last Name *</label>
              <input value={patientForm.lastName} onChange={setPatientField('lastName')} />
            </div>
          </div>

          <div className="form-group">
            <label>Date of Birth</label>
            <input type="date" value={patientForm.dateOfBirth} onChange={setPatientField('dateOfBirth')} />
          </div>

          <div className="form-group">
            <label>Gender</label>
            <select value={patientForm.gender} onChange={setPatientField('gender')}>
              <option value="">Select gender…</option>
              <option value="Male">Male</option>
              <option value="Female">Female</option>
              <option value="Other">Other</option>
            </select>
          </div>

          <div className="form-group">
            <label>Medical History</label>
            <textarea value={patientForm.medicalHistory} onChange={setPatientField('medicalHistory')} />
          </div>
        </Modal>
      )}
    </div>
  )
}