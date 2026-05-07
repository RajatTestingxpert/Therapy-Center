import { useState, useEffect, useMemo } from 'react'
import { doctors, patients, appointments, therapies } from '../api/api'
import { useAuth } from '../context/AuthContext'
import { doctorName, doctorSpecialization, patientName, therapyName } from '../utils/display'

export default function BookAppointment({ isOnline = false }) {
  const { user } = useAuth()

  const [doctorList, setDoctorList] = useState([])
  const [therapyList, setTherapyList] = useState([])
  const [patientList, setPatientList] = useState([])
  const [slots, setSlots] = useState([])

  const [form, setForm] = useState({
    patientId: '',
    doctorId: '',
    therapyId: '',
    slotId: '',
    notes: '',
    receptionistId: isOnline ? null : user?.userId,
  })

  const [date, setDate] = useState(new Date().toISOString().slice(0, 10))
  const [loadingPatients, setLoadingPatients] = useState(true)
  const [loadingTherapies, setLoadingTherapies] = useState(true)
  const [loadingSlots, setLoadingSlots] = useState(false)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const isPatient = user?.role === 'Patient'
  const isGuardian = user?.role === 'Guardian'
  const isStaff = user?.role === 'Admin' || user?.role === 'Receptionist'

  useEffect(() => {
    doctors.getAll()
      .then(d => setDoctorList(d || []))
      .catch(() => setDoctorList([]))
  }, [])

  useEffect(() => {
    therapies.getAll()
      .then(t => setTherapyList(t || []))
      .catch(() => setTherapyList([]))
      .finally(() => setLoadingTherapies(false))
  }, [])

  useEffect(() => {
    async function loadPatients() {
      setLoadingPatients(true)
      try {
        if (isPatient) {
          const me = await patients.getMe()
          setPatientList(me ? [me] : [])
          setForm(prev => ({ ...prev, patientId: me?.patientId ?? '' }))
        } else if (isGuardian) {
          const list = await patients.getByGuardian(user.userId)
          setPatientList(list || [])
          setForm(prev => ({ ...prev, patientId: list?.[0]?.patientId ?? '' }))
        } else if (isStaff) {
          const list = await patients.getAll()
          setPatientList(list || [])
        }
      } catch {
        setPatientList([])
      } finally {
        setLoadingPatients(false)
      }
    }
    loadPatients()
  }, [isPatient, isGuardian, isStaff, user])

  useEffect(() => {
    if (!form.doctorId || !date) {
      setSlots([])
      return
    }
    setLoadingSlots(true)
    doctors.getSlots(form.doctorId, date)
      .then(s => setSlots(s || []))
      .catch(() => setSlots([]))
      .finally(() => setLoadingSlots(false))
  }, [form.doctorId, date])

  useEffect(() => {
    if (doctorList.length === 0 || therapyList.length === 0) return
    if (!form.doctorId) return
    const selectedDoctor = doctorList.find(d => String(d.doctorId ?? d.DoctorId) === String(form.doctorId))
    if (!selectedDoctor) return

    const doctorSpec = (selectedDoctor.specialization ?? selectedDoctor.Specialization ?? '').toLowerCase()
    const matchingTherapy = therapyList.find(t => {
      const therapy = (t.name ?? t.Name ?? '').toLowerCase()
      return therapy === doctorSpec || therapy.includes(doctorSpec) || doctorSpec.includes(therapy)
    })

    if (matchingTherapy && !form.therapyId) {
      setForm(prev => ({ ...prev, therapyId: String(matchingTherapy.therapyId ?? matchingTherapy.TherapyId) }))
    }
  }, [doctorList, therapyList, form.doctorId, form.therapyId])

  async function handleSubmit(e) {
    e.preventDefault()
    setSaving(true)
    setError('')
    setSuccess('')
    try {
      const body = {
        patientId: isPatient ? 0 : Number(form.patientId),
        doctorId: Number(form.doctorId),
        therapyId: Number(form.therapyId),
        slotId: Number(form.slotId),
        notes: form.notes || null,
        receptionistId: isOnline ? null : (user?.userId || null),
      }
      if (isOnline) {
        await appointments.bookOnline(body)
      } else {
        await appointments.book(body)
      }
      setSuccess('Appointment booked successfully!')
      setForm({ patientId: '', doctorId: '', therapyId: '', slotId: '', notes: '', receptionistId: null })
      setSlots([])
    } catch (e) {
      setError(e.message)
    } finally {
      setSaving(false)
    }
  }

  function set(field) {
    return e => setForm(prev => ({
      ...prev,
      [field]: e.target.value,
      ...(field === 'doctorId' ? { slotId: '', therapyId: '' } : {}),
      ...(field === 'therapyId' ? { slotId: prev.slotId } : {}),
    }))
  }

  const selectedDoctor = useMemo(() => doctorList.find(d => String(d.doctorId ?? d.DoctorId) === String(form.doctorId)), [doctorList, form.doctorId])
  const selectedTherapy = useMemo(() => therapyList.find(t => String(t.therapyId ?? t.TherapyId) === String(form.therapyId)), [therapyList, form.therapyId])

  return (
    <div>
      <div className="page-header">
        <h1>{isOnline ? 'Book Appointment Online' : 'Book Appointment'}</h1>
        <p>{isOnline ? 'Book your own appointment' : 'Book an appointment for a patient'}</p>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      <div className="card" style={{ maxWidth: 720 }}>
        <div className="card-body">
          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label>Patient</label>
              {loadingPatients ? (
                <div className="text-muted" style={{ padding: '8px 0' }}>Loading patients…</div>
              ) : isPatient ? (
                <input value={patientList[0] ? patientName(patientList[0]) : 'Loading your patient profile…'} disabled />
              ) : patientList.length > 0 ? (
                <select value={form.patientId} onChange={set('patientId')} required>
                  <option value="">Select patient…</option>
                  {patientList.map(p => (
                    <option key={p.patientId ?? p.PatientId} value={p.patientId ?? p.PatientId}>
                      {patientName(p)}
                      {p.dateOfBirth ? ` — ${new Date(p.dateOfBirth).getFullYear()}` : ''}
                    </option>
                  ))}
                </select>
              ) : (
                <div className="alert alert-info">No patient records found. Create a patient first.</div>
              )}
            </div>

            <div className="form-group">
              <label>Doctor</label>
              <select value={form.doctorId} onChange={set('doctorId')} required>
                <option value="">Select doctor…</option>
                {doctorList.map(d => (
                  <option key={d.doctorId ?? d.DoctorId} value={d.doctorId ?? d.DoctorId}>
                    {doctorName(d)} — {doctorSpecialization(d)}
                  </option>
                ))}
              </select>
            </div>

            {selectedDoctor && (
              <div className="alert alert-info" style={{ marginBottom: 14 }}>
                Selected doctor: <strong>{doctorName(selectedDoctor)}</strong> · {doctorSpecialization(selectedDoctor)}
              </div>
            )}

            <div className="form-group">
              <label>Specialization / Therapy</label>
              {loadingTherapies ? (
                <div className="text-muted" style={{ padding: '8px 0' }}>Loading therapies…</div>
              ) : therapyList.length > 0 ? (
                <select value={form.therapyId} onChange={set('therapyId')} required>
                  <option value="">Select therapy…</option>
                  {therapyList.map(t => (
                    <option key={t.therapyId ?? t.TherapyId} value={t.therapyId ?? t.TherapyId}>
                      {therapyName(t)} — {t.durationMinutes ?? t.DurationMinutes} min
                    </option>
                  ))}
                </select>
              ) : (
                <div className="alert alert-info">No therapy options found.</div>
              )}
            </div>

            {selectedTherapy && (
              <div className="alert alert-info" style={{ marginBottom: 14 }}>
                Selected therapy: <strong>{therapyName(selectedTherapy)}</strong>
              </div>
            )}

            {form.doctorId && (
              <>
                <div className="form-group">
                  <label>Date *</label>
                  <input
                    type="date"
                    value={date}
                    onChange={e => { setDate(e.target.value); setForm(prev => ({ ...prev, slotId: '' })) }}
                    required
                  />
                </div>

                <div className="form-group">
                  <label>Available Slot *</label>
                  {loadingSlots ? (
                    <div className="text-muted" style={{ padding: '8px 0' }}>Loading slots…</div>
                  ) : slots.length === 0 ? (
                    <div className="alert alert-info">No available slots for this date. Try another date or doctor.</div>
                  ) : (
                    <div className="slot-grid">
                      {slots.map(slot => (
                        <button
                          key={slot.slotId ?? slot.SlotId}
                          type="button"
                          className={`slot-btn ${slot.isBooked ? 'booked' : ''} ${String(form.slotId) === String(slot.slotId ?? slot.SlotId) ? 'selected' : ''}`}
                          disabled={slot.isBooked}
                          onClick={() => !slot.isBooked && setForm(prev => ({ ...prev, slotId: String(slot.slotId ?? slot.SlotId) }))}
                        >
                          {slot.startTime ?? slot.StartTime}
                        </button>
                      ))}
                    </div>
                  )}
                </div>
              </>
            )}

            <div className="form-group">
              <label>Notes</label>
              <textarea value={form.notes} onChange={set('notes')} placeholder="Any additional notes…" />
            </div>

            <button className="btn btn-primary" disabled={saving || !form.slotId || !form.therapyId || (!isPatient && !form.patientId)}>
              {saving ? 'Booking…' : '✅ Confirm Booking'}
            </button>
          </form>
        </div>
      </div>
    </div>
  )
}
