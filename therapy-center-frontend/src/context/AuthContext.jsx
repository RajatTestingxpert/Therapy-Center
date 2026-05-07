import { createContext, useContext, useState, useEffect } from 'react'

const AuthContext = createContext(null)

function canonicalRole(role) {
  const value = String(role ?? '').trim().toLowerCase()
  const map = { patient: 'Patient', guardian: 'Guardian', admin: 'Admin', receptionist: 'Receptionist', doctor: 'Doctor' }
  return map[value] ?? (role ?? '')
}

function normalizeAuthUser(data) {
  if (!data) return null
  return {
    token: data.token ?? data.Token ?? '',
    role: canonicalRole(data.role ?? data.Role ?? ''),
    userId: data.userId ?? data.UserId ?? null,
    fullName: data.fullName ?? data.FullName ?? '',
    email: data.email ?? data.Email ?? '',
    raw: data,
  }
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const stored = localStorage.getItem('user')
    if (stored) {
      try {
        setUser(normalizeAuthUser(JSON.parse(stored)))
      } catch {
        localStorage.removeItem('user')
        localStorage.removeItem('token')
      }
    }
    setLoading(false)
  }, [])

  function login(userData) {
    const normalized = normalizeAuthUser(userData)
    localStorage.setItem('token', normalized.token)
    localStorage.setItem('user', JSON.stringify(normalized))
    setUser(normalized)
  }

  function logout() {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, login, logout, loading }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  return useContext(AuthContext)
}
