import PagePlaceholder from '../components/PagePlaceholder'

export default function AdminUsersPage() {
  return (
    <PagePlaceholder
      badge="Admin"
      title="Admin Users"
      description="Admin workspace for user and company administrator management."
      highlights={[
        'Visible to Admin users only.',
        'Protected by the ManageUsers permission.',
        'User management requests are still authorized by the backend.',
      ]}
    />
  )
}
