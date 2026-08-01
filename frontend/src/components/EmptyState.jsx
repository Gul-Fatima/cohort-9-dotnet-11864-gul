import { Inbox } from 'lucide-react'

export default function EmptyState({ title, message, action }) {
  return (
    <div className="empty-state">
      <div className="empty-icon">
        <Inbox size={28} />
      </div>
      <h3 className="empty-title">{title}</h3>
      {message && <p className="empty-message">{message}</p>}
      {action}
    </div>
  )
}
