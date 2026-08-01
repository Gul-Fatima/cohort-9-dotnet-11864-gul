import { useState } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { Loader2, CheckCircle2, Lock, Mail } from 'lucide-react'
import { useAuth } from '../context/AuthContext'
import { apiErrorMessage } from '../utils/errors'

export default function LoginPage() {
  const { user, login } = useAuth()
  const navigate = useNavigate()
  const [form, setForm] = useState({ email: '', password: '' })
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  if (user) return <Navigate to="/dashboard" replace />

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setBusy(true)
    try {
      await login(form)
      navigate('/dashboard', { replace: true })
    } catch (err) {
      setError(apiErrorMessage(err, 'Login failed. Please try again.'))
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-brand-panel">
        <div className="brand brand-light">
          <span className="brand-logo">
            <CheckCircle2 size={24} />
          </span>
          <span className="brand-name">Task Manager</span>
        </div>
        <div className="auth-brand-copy">
          <h1>Stay on top of every task.</h1>
          <p>
            Organize, prioritize and track your work in one clean place — from
            personal errands to team projects.
          </p>
        </div>
        <ul className="auth-feature-list">
          <li>Role-based access for Admins &amp; Users</li>
          <li>Filters, priorities and due dates</li>
          <li>Live dashboard statistics</li>
        </ul>
      </div>

      <div className="auth-form-panel">
        <div className="auth-card">
          <h2>Welcome back</h2>
          <p className="auth-sub">Log in to your account to continue.</p>

          <form onSubmit={handleSubmit} noValidate>
            <div className="field">
              <label htmlFor="email">Email</label>
              <div className="input-with-icon">
                <Mail size={16} />
                <input
                  id="email"
                  type="email"
                  placeholder="you@example.com"
                  value={form.email}
                  onChange={(e) => setForm({ ...form, email: e.target.value })}
                  required
                  autoFocus
                />
              </div>
            </div>

            <div className="field">
              <label htmlFor="password">Password</label>
              <div className="input-with-icon">
                <Lock size={16} />
                <input
                  id="password"
                  type="password"
                  placeholder="••••••••"
                  value={form.password}
                  onChange={(e) => setForm({ ...form, password: e.target.value })}
                  required
                />
              </div>
            </div>

            {error && <div className="form-error">{error}</div>}

            <button type="submit" className="btn btn-primary btn-block" disabled={busy}>
              {busy && <Loader2 size={16} className="spin" />}
              {busy ? 'Logging in…' : 'Log in'}
            </button>
          </form>

          <p className="auth-switch">
            Don&apos;t have an account? <Link to="/signup">Sign up</Link>
          </p>

          <div className="auth-demo">
            <strong>Demo accounts</strong>
            <p>Admin — admin@example.com / Admin@123</p>
            <p>User — user@example.com / User@123</p>
          </div>
        </div>
      </div>
    </div>
  )
}
