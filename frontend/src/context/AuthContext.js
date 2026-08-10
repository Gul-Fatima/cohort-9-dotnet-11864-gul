import { createContext, useContext } from 'react'

// Plain module (no components) so fast-refresh rules are happy.
export const AuthContext = createContext(null)

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within an AuthProvider')
  return ctx
}
