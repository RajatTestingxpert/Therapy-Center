const BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api'

function getToken() {
  return localStorage.getItem('token')
}

async function request(method, path, body = null) {
  const headers = { 'Content-Type': 'application/json' }
  const token = getToken()
  if (token) headers['Authorization'] = `Bearer ${token}`

  const options = { method, headers }
  if (body !== null && body !== undefined) options.body = JSON.stringify(body)

  const res = await fetch(`${BASE_URL}${path}`, options)

  if (res.status === 204) return null

  const text = await res.text()
  let data = {}
  if (text) {
    try {
      data = JSON.parse(text)
    } catch {
      data = { message: text }
    }
  }

  if (!res.ok) {
    const msg = data?.message || data?.title || `Request failed (${res.status})`
    throw new Error(msg)
  }

  return data
}

export const auth = {
  login: (body) => request('POST', '/auth/login', body),
  register: (body) => request('POST', '/auth/register', body),
  createStaff: (body) => request('POST', '/auth/create-staff', body),
}

export const admin = {
  getTherapies: () => request('GET', '/admin/therapies'),
  createTherapy: (body) => request('POST', '/admin/therapies', body),
  updateTherapy: (id, body) => request('PUT', `/admin/therapies/${id}`, body),
  deleteTherapy: (id) => request('DELETE', `/admin/therapies/${id}`),

  deleteDoctor: (id) => request('DELETE', `/admin/doctors/${id}`),
  deleteStaff: (id) => request('DELETE', `/admin/staff/${id}`),

  createDoctorProfile: (body) => request('POST', '/admin/doctors/profile', body),
  getReceptionists: () => request('GET', '/admin/receptionists'),
  generateSlots: (body) => request('POST', '/admin/slots/generate', body),
}

export const therapies = {
  getAll: () => request('GET', '/therapy'),
}

export const doctors = {
  getAll: () => request('GET', '/doctor'),
  getById: (id) => request('GET', `/doctor/${id}`),
  getSlots: (id, date) => request('GET', `/doctor/${id}/slots?date=${date}`),
  getAppointments: (id) => request('GET', `/doctor/${id}/appointments`),
  getMyAppointments: () => request('GET', '/doctor/my-appointments'),
}

export const patients = {
  getAll: () => request('GET', '/patient'),
  getById: (id) => request('GET', `/patient/${id}`),
  getByGuardian: (guardianId) => request('GET', `/patient/guardian/${guardianId}`),
  getMe: () => request('GET', '/patient/me'),
  create: (body) => request('POST', '/patient', body),
}

export const appointments = {
  book: (body) => request('POST', '/appointment/book', body),
  bookOnline: (body) => request('POST', '/appointment/book-online', body),
  getById: (id) => request('GET', `/appointment/${id}`),
  getByPatient: (patientId) => request('GET', `/appointment/patient/${patientId}`),
  getByDoctor: (doctorId) => request('GET', `/appointment/doctor/${doctorId}`),
  getByDate: (date) => request('GET', `/appointment/date/${date}`),
  updateStatus: (id, body) => request('PATCH', `/appointment/${id}/status`, body),
}
export const payments = {
  getByAppointment: (appointmentId) => request('GET', `/payment/appointment/${appointmentId}`),
  getByPatient: (patientId) => request('GET', `/payment/patient/${patientId}`),
  record: (body) => request('POST', '/payment', body),
  markPaid: (appointmentId, body = {}) => request('PATCH', `/payment/${appointmentId}/paid`, body),
}
export const findings = {
  getByAppointment: (appointmentId) => request('GET', `/doctor/appointments/${appointmentId}/finding`),
  save: (appointmentId, body) => request('PUT', `/doctor/appointments/${appointmentId}/finding`, body),
  
}