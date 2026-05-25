import { useEffect } from 'react'
import PagePlaceholder from '../components/PagePlaceholder'

export default function MyPropertiesPage() {
  useEffect(() => { document.title = 'My Properties | EstateIQ' }, [])

  return (
    <PagePlaceholder
      badge="Agent"
      title="My Properties"
      description="Agent workspace for managing assigned and created property listings."
      highlights={[
        'Visible to Agent users only.',
        'Protected by the Agent role and property creation permission.',
        'Backend authorization remains the source of truth for property actions.',
      ]}
    />
  )
}
