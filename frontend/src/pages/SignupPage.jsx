import { useState } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { Loader2, CheckCircle2, Lock, Mail, UserRound } from 'lucide-react'
import { useAuth } from '../context/AuthContext'
import { apiErrorMessage } from '../utils/errors'

export default function SignupPage() {
  const { user, register } = useAuth()
  const navigate = useNavigate()
  const [form, setForm] = useState({ name: '', email: '', password: '', confirm: '' })
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  if (user) return <Navigate to="/dashboard" replace />

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    if (form.password.length < 6) {
      setError('Password must be at least 6 characters.')
      return
    }
    if (form.password !== form.confirm) {
      setError('Passwords do not match.')
      return
    }
    setBusy(true)
    try {
      await register({ name: form.name, email: form.email, password: form.password })
      navigate('/dashboard', { replace: true })
    } catch (err) {
      setError(apiErrorMessage(err, 'Registration failed.'))
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
          <h1>Create your account.</h1>
          <p>Start organizing your day in minutes — it&apos;s free and takes seconds to set up.</p>
        </div>
        <ul className="auth-feature-list">
          <li>Create and manage unlimited tasks</li>
          <li>Track progress with a live dashboard</li>
          <li>Admin dashboard shows team-wide stats</li>
        </ul>
      </div>

      <div className="auth-form-panel">
        <div className="auth-card">
          <h2>Create account</h2>
          <p className="auth-sub">Join Task Manager today.</p>

          <form onSubmit={handleSubmit} noValidate>
            <div className="field">
              <label htmlFor="name">Full name</label>
              <div className="input-with-icon">
                <UserRound size={16} />
                <input
                  id="name"
                  type="text"
                  placeholder="Jane Doe"
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                  required
                  autoFocus
                />
              </div>
            </div>

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
                  placeholder="At least 6 characters"
                  value={form.password}
                  onChange={(e) => setForm({ ...form, password: e.target.value })}
                  required
                />
              </div>
            </div>

            <div className="field">
              <label htmlFor="confirm">Confirm password</label>
              <div className="input-with-icon">
                <Lock size={16} />
                <input
                  id="confirm"
                  type="password"
                  placeholder="Repeat your password"
                  value={form.confirm}
                  onChange={(e) => setForm({ ...form, confirm: e.target.value })}
                  required
                />
              </div>
            </div>

            {error && <div className="form-error">{error}</div>}

            <button type="submit" className="btn btn-primary btn-block" disabled={busy}>
              {busy && <Loader2 size={16} className="spin" />}
              {busy ? 'Creating account…' : 'Sign up'}
            </button>
          </form>

          <p className="auth-switch">
            Already have an account? <Link to="/login">Log in</Link>
          </p>
        </div>
      </div>
    </div>
  )
}
