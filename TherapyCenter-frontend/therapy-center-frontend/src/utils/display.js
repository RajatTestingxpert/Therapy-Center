export function personName(source) {
  if (!source) return '—'
  const first = source.firstName ?? source.FirstName ?? source.user?.firstName ?? source.user?.FirstName ?? ''
  const last = source.lastName ?? source.LastName ?? source.user?.lastName ?? source.user?.LastName ?? ''
  const full = source.fullName ?? source.FullName
  return (full || `${first} ${last}`).trim() || '—'
}

export function doctorName(source) {
  return personName(source)
}

export function doctorSpecialization(source) {
  return source?.specialization ?? source?.Specialization ?? '—'
}

export function patientName(source) {
  return personName(source)
}

export function therapyName(source) {
  return source?.name ?? source?.Name ?? '—'
}
