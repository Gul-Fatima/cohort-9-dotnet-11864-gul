import { useNavigate } from 'react-router-dom'
import { LogOut, Mail, ShieldCheck, UserRound } from 'lucide-react'
import { useAuth } from '../context/AuthContext'

const fmtDate = (ts) =>
  ts ? new Date(ts).toLocaleDateString(undefined, { year: 'numeric', month: 'long', day: 'numeric' }) : '—'

export default function ProfilePage() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const initials = (user?.name || 'U')
    .split(' ')
    .map((p) => p[0])
    .slice(0, 2)
    .join('')
    .toUpperCase()

  const handleLogout = () => {
    logout()
    navigate('/login', { replace: true })
  }

  return (
    <div className="page page-narrow">
      <header className="page-header">
        <h1 className="page-title">Profile</h1>
        <p className="page-subtitle">Your account details.</p>
      </header>

      <div className="card profile-card">
        <div className="profile-hero">
          <span className="avatar avatar-lg">{initials}</span>
          <div>
            <h2 className="profile-name">{user?.name}</h2>
            <span className={`badge ${user?.role === 'Admin' ? 'badge-violet' : 'badge-slate'}`}>
              {user?.role}
            </span>
          </div>
        </div>

        <dl className="detail-meta profile-meta">
          <div>
            <dt>
              <UserRound size={15} /> Name
            </dt>
            <dd>{user?.name}</dd>
          </div>
          <div>
            <dt>
              <Mail size={15} /> Email
            </dt>
            <dd>{user?.email}</dd>
          </div>
          <div>
            <dt>
              <ShieldCheck size={15} /> Role
            </dt>
            <dd>{user?.role}</dd>
          </div>
          <div>
            <dt>
              <UserRound size={15} /> Member since
            </dt>
            <dd>{fmtDate(user?.createdAt)}</dd>
          </div>
        </dl>

        <div className="profile-actions">
          <button type="button" className="btn btn-danger" onClick={handleLogout}>
            <LogOut size={15} /> Log out
          </button>
        </div>
      </div>
    </div>
  )
}
