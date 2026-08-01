import { useEffect, useState } from 'react'
import { Search, X } from 'lucide-react'
import api from '../api'

const STATUS_OPTIONS = ['Pending', 'InProgress', 'Completed']
const PRIORITY_OPTIONS = ['Low', 'Medium', 'High']

// Filter bar for the task list (search, status, priority, category, assignee).
export default function TaskFilters({ filters, onChange, showAssignee = true }) {
  const [categories, setCategories] = useState([])
  const [users, setUsers] = useState([])

  useEffect(() => {
    api.getCategories().then(setCategories).catch(() => setCategories([]))
    if (showAssignee) {
      api.getUsers().then(setUsers).catch(() => setUsers([]))
    }
  }, [showAssignee])

  const set = (key, value) => onChange({ ...filters, [key]: value })

  const hasActive =
    filters.search || filters.status || filters.priority || filters.categoryId || filters.assignedUserId

  const clearAll = () =>
    onChange({ search: '', status: '', priority: '', categoryId: '', assignedUserId: '' })

  return (
    <div className="filters-bar">
      <div className="filter-search">
        <Search size={16} />
        <input
          type="search"
          placeholder="Search tasks…"
          value={filters.search || ''}
          onChange={(e) => set('search', e.target.value)}
        />
        {filters.search && (
          <button type="button" className="icon-btn" onClick={() => set('search', '')} title="Clear search">
            <X size={14} />
          </button>
        )}
      </div>

      <select
        className="filter-select"
        value={filters.status || ''}
        onChange={(e) => set('status', e.target.value)}
        aria-label="Filter by status"
      >
        <option value="">All statuses</option>
        {STATUS_OPTIONS.map((s) => (
          <option key={s} value={s}>
            {s === 'InProgress' ? 'In Progress' : s}
          </option>
        ))}
      </select>

      <select
        className="filter-select"
        value={filters.priority || ''}
        onChange={(e) => set('priority', e.target.value)}
        aria-label="Filter by priority"
      >
        <option value="">All priorities</option>
        {PRIORITY_OPTIONS.map((p) => (
          <option key={p} value={p}>
            {p}
          </option>
        ))}
      </select>

      <select
        className="filter-select"
        value={filters.categoryId || ''}
        onChange={(e) => set('categoryId', e.target.value)}
        aria-label="Filter by category"
      >
        <option value="">All categories</option>
        {categories.map((c) => (
          <option key={c.id} value={c.id}>
            {c.name}
          </option>
        ))}
      </select>

      {showAssignee && (
        <select
          className="filter-select"
          value={filters.assignedUserId || ''}
          onChange={(e) => set('assignedUserId', e.target.value)}
          aria-label="Filter by assigned user"
        >
          <option value="">All assignees</option>
          {users.map((u) => (
            <option key={u.id} value={u.id}>
              {u.name}
            </option>
          ))}
        </select>
      )}

      {hasActive && (
        <button type="button" className="btn btn-ghost btn-sm" onClick={clearAll}>
          Clear
        </button>
      )}
    </div>
  )
}
