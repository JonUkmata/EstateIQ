import PagePlaceholder from '../components/PagePlaceholder'

export default function CompanyAgentsPage() {
  return (
    <PagePlaceholder
      badge="CompanyAdmin"
      title="Company Agents"
      description="Company administrator workspace for managing agents connected to the company."
      highlights={[
        'Visible to CompanyAdmin users only.',
        'Protected by the ManageAgents permission.',
        'Agent management API authorization remains enforced by the backend.',
      ]}
    />
  )
}
