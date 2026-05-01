type LoadingSpinnerProps = {
  label?: string
  className?: string
}

export default function LoadingSpinner({ label = 'Loading', className = '' }: LoadingSpinnerProps) {
  const classes = className ? `loading-spinner ${className}` : 'loading-spinner'

  return (
    <span className={classes} role="status" aria-label={label}>
      <span className="loading-spinner-dot" aria-hidden="true" />
    </span>
  )
}
