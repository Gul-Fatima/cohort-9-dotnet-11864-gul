import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { ArrowLeft } from 'lucide-react'
import api from '../api'
import TaskForm from '../components/TaskForm'
import Spinner from '../components/Spinner'

// Serves both /tasks/new (create) and /tasks/:id/edit (update).
export default function NewTaskPage() {
  const { id } = useParams()
  const isEdit = Boolean(id)
  const navigate = useNavigate()
  const [initial, setInitial] = useState(null)
  const [loading, setLoading] = useState(isEdit)

  useEffect(() => {
    if (!isEdit) return
    let active = true
    api
      .getTask(id)
      .then((t) => active && setInitial(t))
      .catch(() => active && navigate('/tasks'))
      .finally(() => active && setLoading(false))
    return () => {
      active = false
    }
  }, [id, isEdit, navigate])

  const handleSubmit = async (payload) => {
    if (isEdit) {
      await api.updateTask(id, payload)
    } else {
      await api.createTask(payload)
    }
    navigate(isEdit ? `/tasks/${id}` : '/tasks')
  }

  if (loading) return <Spinner label="Loading task…" />

  return (
    <div className="page page-narrow">
      <header className="page-header">
        <Link to={isEdit ? `/tasks/${id}` : '/tasks'} className="btn btn-ghost btn-sm">
          <ArrowLeft size={14} /> Back
        </Link>
        <h1 className="page-title">{isEdit ? 'Edit Task' : 'New Task'}</h1>
        <p className="page-subtitle">
          {isEdit ? 'Update the task details below.' : 'Fill in the details to create a new task.'}
        </p>
      </header>

      <div className="card">
        <TaskForm
          key={id ?? 'new'}
          initial={initial}
          onSubmit={handleSubmit}
          submitLabel={isEdit ? 'Save Changes' : 'Create Task'}
          onCancel={() => navigate(isEdit ? `/tasks/${id}` : '/tasks')}
        />
      </div>
    </div>
  )
}
