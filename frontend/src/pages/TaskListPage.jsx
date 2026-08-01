import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Plus, CalendarDays, ArrowRight } from 'lucide-react'
import api from '../api'
import Spinner from '../components/Spinner'
import EmptyState from '../components/EmptyState'
import TaskFilters from '../components/TaskFilters'
import { StatusBadge, PriorityBadge } from '../components/Badge'
import { useAuth } from '../context/AuthContext'
import { apiErrorMessage } from '../utils/errors'

const fmtDate = (iso) =>
  iso
    ? new Date(iso).toLocaleDateString(undefined, { month: 'short', day: 'numeric' })
    : '—'

const EMPTY_FILTERS = { search: '', status: '', priority: '', categoryId: '', assignedUserId: '' }

export default function TaskListPage() {
  const { user } = useAuth()
  const [tasks, setTasks] = useState([])
  const [filters, setFilters] = useState(EMPTY_FILTERS)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const fetchTasks = useCallback(() => {
    setLoading(true)
    api
      .getTasks(filters)
      .then(setTasks)
      .catch((err) => setError(apiErrorMessage(err, 'Failed to load tasks.')))
      .finally(() => setLoading(false))
  }, [filters])

  useEffect(() => {
    fetchTasks()
  }, [fetchTasks])

  return (
    <div className="page">
      <header className="page-header">
        <div>
          <h1 className="page-title">Tasks</h1>
          <p className="page-subtitle">Browse, filter and manage all your tasks.</p>
        </div>
        <Link to="/tasks/new" className="btn btn-primary">
          <Plus size={16} /> New Task
        </Link>
      </header>

      <TaskFilters filters={filters} onChange={setFilters} showAssignee={user?.role === 'Admin'} />

      {error && <div className="form-error">{error}</div>}

      {loading ? (
        <Spinner label="Loading tasks…" />
      ) : tasks.length === 0 ? (
        <div className="card">
          <EmptyState
            title="No tasks found"
            message={
              Object.values(filters).some(Boolean)
                ? 'Try adjusting your filters, or clear them to see everything.'
                : 'Create your first task to get started.'
            }
            action={
              <Link to="/tasks/new" className="btn btn-primary btn-sm">
                <Plus size={14} /> New Task
              </Link>
            }
          />
        </div>
      ) : (
        <div className="card">
          <div className="task-table">
            <div className="task-row task-row-head">
              <span>Title</span>
              <span>Status</span>
              <span>Priority</span>
              <span>Assignee</span>
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
                <span className="task-assignee">{t.assignedTo?.name ?? 'Unassigned'}</span>
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
        </div>
      )}
    </div>
  )
}
