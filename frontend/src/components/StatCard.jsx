export default function StatCard({ label, value, icon: Icon, tone, hint }) {
  return (
    <div className={`stat-card tone-${tone}`}>
      <div className="stat-icon">
        <Icon size={22} />
      </div>
      <div className="stat-body">
        <span className="stat-value">{value}</span>
        <span className="stat-label">{label}</span>
        {hint && <span className="stat-hint">{hint}</span>}
      </div>
    </div>
  )
}
