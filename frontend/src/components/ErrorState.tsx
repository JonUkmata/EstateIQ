export default function ErrorState({ message }: { message: string }) {
  return (
    <div className="table-state table-state-error">
      <p>{message}</p>
    </div>
  )
}
