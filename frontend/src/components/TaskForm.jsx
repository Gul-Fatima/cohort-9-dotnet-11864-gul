import { useEffect, useState } from 'react'
import { Loader2 } from 'lucide-react'
import api from '../api'
import { useAuth } from '../context/AuthContext'
import { apiErrorMessage } from '../utils/errors'

const STATUS_OPTIONS = ['Pending', 'InProgress', 'Completed']
const PRIORITY_OPTIONS = ['Low', 'Medium', 'High']

// Shared create/edit task form.
export default function TaskForm({ initial, onSubmit, submitLabel = 'Save Task', onCancel }) {
  const { user } = useAuth()
  const [categories, setCategories] = useState([])
  const [users, setUsers] = useState([])
  const [form, setForm] = useState({
    title: initial?.title ?? '',
    description: initial?.description ?? '',
    status: initial?.status ?? 'Pending',
    priority: initial?.priority ?? 'Medium',
    dueDate: initial?.dueDate ? initial.dueDate.slice(0, 10) : '',
    categoryId: initial?.categoryId ?? '',
    assignedUserId: initial?.assignedUserId ?? '',
  })
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    api.getCategories().then(setCategories).catch(() => setCategories([]))
    // Only admins may assign tasks to others; regular users see themselves.
    if (user?.role === 'Admin') {
      api.getUsers().then(setUsers).catch(() => setUsers([]))
    }
  }, [user])

  const handleChange = (e) => {
    const { name, value } = e.target
    setForm((f) => ({ ...f, [name]: value }))
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    if (!form.title.trim()) {
      setError('Title is required.')
      return
    }
    setSaving(true)
    try {
      await onSubmit({
        ...form,
        title: form.title.trim(),
        categoryId: form.categoryId ? Number(form.categoryId) : null,
        assignedUserId: form.assignedUserId || user.id,
      })
    } catch (err) {
      setError(apiErrorMessage(err))
      setSaving(false)
    }
  }

  return (
    <form className="task-form" onSubmit={handleSubmit} noValidate>
      <div className="form-grid">
        <div className="field field-span-2">
          <label htmlFor="title">Title *</label>
          <input
            id="title"
            name="title"
            type="text"
            placeholder="e.g. Build login page"
            value={form.title}
            onChange={handleChange}
            autoFocus
          />
        </div>

        <div className="field field-span-2">
          <label htmlFor="description">Description</label>
          <textarea
            id="description"
            name="description"
            rows={4}
            placeholder="Add more details about this task…"
            value={form.description}
            onChange={handleChange}
          />
        </div>

        <div className="field">
          <label htmlFor="status">Status</label>
          <select id="status" name="status" value={form.status} onChange={handleChange}>
            {STATUS_OPTIONS.map((s) => (
              <option key={s} value={s}>
                {s === 'InProgress' ? 'In Progress' : s}
              </option>
            ))}
          </select>
        </div>

        <div className="field">
          <label htmlFor="priority">Priority</label>
          <select id="priority" name="priority" value={form.priority} onChange={handleChange}>
            {PRIORITY_OPTIONS.map((p) => (
              <option key={p} value={p}>
                {p}
              </option>
            ))}
          </select>
        </div>

        <div className="field">
          <label htmlFor="dueDate">Due Date</label>
          <input
            id="dueDate"
            name="dueDate"
            type="date"
            value={form.dueDate}
            onChange={handleChange}
          />
        </div>

        <div className="field">
          <label htmlFor="categoryId">Category</label>
          <select id="categoryId" name="categoryId" value={form.categoryId} onChange={handleChange}>
            <option value="">Uncategorized</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
        </div>

        {user?.role === 'Admin' && (
          <div className="field field-span-2">
            <label htmlFor="assignedUserId">Assigned To</label>
            <select
              id="assignedUserId"
              name="assignedUserId"
              value={form.assignedUserId}
              onChange={handleChange}
            >
              <option value="">Assign to self</option>
              {users.map((u) => (
                <option key={u.id} value={u.id}>
                  {u.name} ({u.role})
                </option>
              ))}
            </select>
          </div>
        )}
      </div>

      {error && <div className="form-error">{error}</div>}

      <div className="form-actions">
        <button type="submit" className="btn btn-primary" disabled={saving}>
          {saving && <Loader2 size={16} className="spin" />}
          {saving ? 'Saving…' : submitLabel}
        </button>
        {onCancel && (
          <button type="button" className="btn btn-ghost" onClick={onCancel}>
            Cancel
          </button>
        )}
      </div>
    </form>
  )
}
