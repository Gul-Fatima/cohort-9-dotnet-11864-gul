import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  CheckCircle2,
  Clock3,
  ListTodo,
  Layers,
  ArrowRight,
  CalendarDays,
} from 'lucide-react'
import api from '../api'
import StatCard from '../components/StatCard'
import Spinner from '../components/Spinner'
import EmptyState from '../components/EmptyState'
import { StatusBadge, PriorityBadge } from '../components/Badge'
import { useAuth } from '../context/AuthContext'
import { apiErrorMessage } from '../utils/errors'

const fmtDate = (iso) =>
  iso
    ? new Date(iso).toLocaleDateString(undefined, { month: 'short', day: 'numeric' })
    : '—'

export default function DashboardPage() {
  const { user } = useAuth()
  const [stats, setStats] = useState(null)
  const [tasks, setTasks] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true
    Promise.all([api.getDashboardStats(), api.getTasks()])
      .then(([s, t]) => {
        if (!active) return
        setStats(s)
        setTasks(t.slice(0, 5))
      })
      .catch((err) => active && setError(apiErrorMessage(err, 'Failed to load dashboard.')))
      .finally(() => active && setLoading(false))
    return () => {
      active = false
    }
  }, [])

  const greeting = (() => {
    const h = new Date().getHours()
    if (h < 12) return 'Good morning'
    if (h < 18) return 'Good afternoon'
    return 'Good evening'
  })()

  if (loading) return <Spinner label="Loading dashboard…" />

  return (
    <div className="page">
      <header className="page-header">
        <div>
          <h1 className="page-title">
            {greeting}, {user?.name?.split(' ')[0]} 👋
          </h1>
          <p className="page-subtitle">
            {user?.role === 'Admin'
              ? 'Here is your team-wide overview.'
              : 'Here is what’s happening with your tasks.'}
          </p>
        </div>
        <Link to="/tasks/new" className="btn btn-primary">
          <Layers size={16} /> New Task
        </Link>
      </header>

      {error && <div className="form-error">{error}</div>}

      <section className="stats-grid">
        <StatCard
          label="Completed"
          value={stats?.completed ?? 0}
          icon={CheckCircle2}
          tone="green"
        />
        <StatCard
          label="In Progress"
          value={stats?.inProgress ?? 0}
          icon={Clock3}
          tone="blue"
        />
        <StatCard label="Pending" value={stats?.pending ?? 0} icon={ListTodo} tone="amber" />
        <StatCard label="Total Tasks" value={stats?.total ?? 0} icon={Layers} tone="violet" />
      </section>

      <section className="card">
        <div className="card-header">
          <h2 className="card-title">Recent tasks</h2>
          <Link to="/tasks" className="link-with-icon">
            View all <ArrowRight size={14} />
          </Link>
        </div>

        {tasks.length === 0 ? (
          <EmptyState
            title="No tasks yet"
            message="Create your first task to get started."
            action={
              <Link to="/tasks/new" className="btn btn-primary btn-sm">
                Create a task
              </Link>
            }
          />
        ) : (
          <div className="task-table">
            <div className="task-row task-row-head">
              <span>Title</span>
              <span>Status</span>
              <span>Priority</span>
              <span>Due</span>
              <span />
            </div>
            {tasks.map((t) => (
              <Link to={`/tasks/${t.id}`} key={t.id} className="task-row task-row-body">
                <span className="task-title-cell">
                  <span className="task-title">{t.title}</span>
                  <span className="task-category">{t.category}</span>
                </span>
                <span>
                  <StatusBadge status={t.status} />
                </span>
                <span>
                  <PriorityBadge priority={t.priority} />
                </span>
                <span className="task-due">
                  <CalendarDays size={14} />
                  {fmtDate(t.dueDate)}
                </span>
                <span className="task-row-arrow">
                  <ArrowRight size={16} />
                </span>
              </Link>
            ))}
          </div>
        )}
      </section>
    </div>
  )
}
