export default function EmptyState({ message }: { message: string }) {
  return (
    <div className="table-state">
      <p>{message}</p>
    </div>
  )
}
