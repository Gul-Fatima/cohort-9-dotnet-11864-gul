import { useEffect, useState } from 'react'
import api from '../api'
import { AuthContext } from './AuthContext'

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    try {
      const raw = localStorage.getItem('tm_user')
      return raw ? JSON.parse(raw) : null
    } catch {
      return null
    }
  })
  const [loading, setLoading] = useState(true)

  // Restore session on first load (validates token against the backend).
  useEffect(() => {
    let active = true
    const token = localStorage.getItem('tm_token')
    if (!token) {
      setLoading(false)
      return
    }
    api
      .getMe()
      .then((me) => {
        if (!active) return
        setUser(me)
        localStorage.setItem('tm_user', JSON.stringify(me))
      })
      .catch(() => {
        if (!active) return
        localStorage.removeItem('tm_token')
        localStorage.removeItem('tm_user')
        setUser(null)
      })
      .finally(() => active && setLoading(false))
    return () => {
      active = false
    }
  }, [])

  const login = async (credentials) => {
    const { token, user: loggedIn } = await api.login(credentials)
    localStorage.setItem('tm_token', token)
    localStorage.setItem('tm_user', JSON.stringify(loggedIn))
    setUser(loggedIn)
    return loggedIn
  }

  const register = async (details) => {
    const { token, user: created } = await api.register(details)
    localStorage.setItem('tm_token', token)
    localStorage.setItem('tm_user', JSON.stringify(created))
    setUser(created)
    return created
  }

  const logout = () => {
    localStorage.removeItem('tm_token')
    localStorage.removeItem('tm_user')
    localStorage.removeItem('tm_mock_session')
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, loading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  )
}
