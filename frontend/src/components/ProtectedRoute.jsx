import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

// Guards private pages: redirects to /login when there is no session.
export default function ProtectedRoute() {
  const { user, loading } = useAuth()
  if (loading) {
    return (
      <div className="screen-loader">
        <span className="spinner" />
      </div>
    )
  }
  if (!user) return <Navigate to="/login" replace />
  return <Outlet />
}
