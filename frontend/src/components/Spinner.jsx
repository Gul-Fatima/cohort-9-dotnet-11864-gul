export default function Spinner({ label }) {
  return (
    <div className="spinner-wrap" role="status">
      <span className="spinner" />
      {label && <span className="spinner-label">{label}</span>}
    </div>
  )
}
