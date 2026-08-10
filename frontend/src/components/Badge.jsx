const STATUS_META = {
  Pending: { label: 'Pending', cls: 'badge-amber' },
  InProgress: { label: 'In Progress', cls: 'badge-blue' },
  Completed: { label: 'Completed', cls: 'badge-green' },
}

const PRIORITY_META = {
  Low: { label: 'Low', cls: 'badge-slate' },
  Medium: { label: 'Medium', cls: 'badge-amber' },
  High: { label: 'High', cls: 'badge-red' },
}

export function StatusBadge({ status }) {
  const meta = STATUS_META[status] ?? { label: status, cls: 'badge-slate' }
  return <span className={`badge ${meta.cls}`}>{meta.label}</span>
}

export function PriorityBadge({ priority }) {
  const meta = PRIORITY_META[priority] ?? { label: priority, cls: 'badge-slate' }
  return <span className={`badge ${meta.cls}`}>{meta.label}</span>
}
