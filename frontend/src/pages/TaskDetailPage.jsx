import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import {
  ArrowLeft,
  Pencil,
  Trash2,
  CalendarDays,
  UserRound,
  Tag,
  Clock3,
} from 'lucide-react'
import api from '../api'
import Spinner from '../components/Spinner'
import { StatusBadge, PriorityBadge } from '../components/Badge'
import { useAuth } from '../context/AuthContext'
import { apiErrorMessage } from '../utils/errors'

const fmtDate = (iso) =>
  iso
    ? new Date(iso).toLocaleDateString(undefined, { weekday: 'short', year: 'numeric', month: 'long', day: 'numeric' })
    : 'No due date'

export default function TaskDetailPage() {
  const { id } = useParams()
  const { user } = useAuth()
  const navigate = useNavigate()
  const [task, setTask] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [deleting, setDeleting] = useState(false)

  useEffect(() => {
    let active = true
    api
      .getTask(id)
      .then((t) => active && setTask(t))
      .catch((err) => active && setError(apiErrorMessage(err, 'Task not found.')))
      .finally(() => active && setLoading(false))
    return () => {
      active = false
    }
  }, [id])

  const canManage = user?.role === 'Admin' || task?.assignedUserId === user?.id

  const handleDelete = async () => {
    if (!window.confirm('Delete this task? This cannot be undone.')) return
    setDeleting(true)
    try {
      await api.deleteTask(id)
      navigate('/tasks', { replace: true })
    } catch (err) {
      setError(apiErrorMessage(err, 'Failed to delete task.'))
      setDeleting(false)
    }
  }

  if (loading) return <Spinner label="Loading task…" />

  if (error || !task) {
    return (
      <div className="page">
        <div className="card">
          <p className="form-error">{error || 'Task not found.'}</p>
          <Link to="/tasks" className="btn btn-ghost btn-sm">
            <ArrowLeft size={14} /> Back to tasks
          </Link>
        </div>
      </div>
    )
  }

  return (
    <div className="page">
      <header className="page-header">
        <Link to="/tasks" className="btn btn-ghost btn-sm">
          <ArrowLeft size={14} /> Back to tasks
        </Link>
        <div className="page-actions">
          {canManage && (
            <>
              <Link to={`/tasks/${task.id}/edit`} className="btn btn-outline">
                <Pencil size={15} /> Edit
              </Link>
              <button
                type="button"
                className="btn btn-danger"
                onClick={handleDelete}
                disabled={deleting}
              >
                <Trash2 size={15} /> {deleting ? 'Deleting…' : 'Delete'}
              </button>
            </>
          )}
        </div>
      </header>

      <div className="detail-layout">
        <div className="card detail-main">
          <div className="detail-top">
            <h1 className="detail-title">{task.title}</h1>
            <div className="detail-badges">
              <StatusBadge status={task.status} />
              <PriorityBadge priority={task.priority} />
            </div>
          </div>

          <p className="detail-description">
            {task.description || 'No description provided.'}
          </p>

          <dl className="detail-meta">
            <div>
              <dt>
                <CalendarDays size={15} /> Due date
              </dt>
              <dd>{fmtDate(task.dueDate)}</dd>
            </div>
            <div>
              <dt>
                <Tag size={15} /> Category
              </dt>
              <dd>{task.category}</dd>
            </div>
            <div>
              <dt>
                <UserRound size={15} /> Assigned to
              </dt>
              <dd>{task.assignedTo?.name ?? 'Unassigned'}</dd>
            </div>
            <div>
              <dt>
                <Clock3 size={15} /> Updated
              </dt>
              <dd>
                {task.updatedAt
                  ? new Date(task.updatedAt).toLocaleDateString()
                  : '—'}
              </dd>
            </div>
          </dl>
        </div>
      </div>
    </div>
  )
}
