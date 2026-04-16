import PagePlaceholder from '../components/PagePlaceholder'

export default function PropertiesPage() {
  return (
    <PagePlaceholder
      badge="Properties"
      title="Properties route placeholder."
      description="This page reserves the space where the property listing and filtering UI will be added."
      highlights={[
        'Route path: /properties',
        'Accessible through the sidebar navigation.',
        'Ready for future cards, tables, or search controls.',
      ]}
    />
  )
}
