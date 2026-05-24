import LoadingSpinner from './LoadingSpinner'

export default function LoadingState({ message = 'Loading...' }: { message?: string }) {
  return (
    <div className="table-state">
      <p className="state-with-spinner">
        <LoadingSpinner label={message} />
        <span>{message}</span>
      </p>
    </div>
  )
}
