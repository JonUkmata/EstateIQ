import PagePlaceholder from '../components/PagePlaceholder'

export default function DashboardPage() {
  return (
    <PagePlaceholder
      badge="Dashboard"
      title="Dashboard route placeholder."
      description="This page is the base container for future KPIs, charts, and management widgets."
      highlights={[
        'Route path: /dashboard',
        'Shares the same base layout as Home and Properties.',
        'Prepared for analytics-focused UI sections later.',
      ]}
    />
  )
}
