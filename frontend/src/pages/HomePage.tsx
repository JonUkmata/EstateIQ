import PagePlaceholder from '../components/PagePlaceholder'

export default function HomePage() {
  return (
    <PagePlaceholder
      badge="Home"
      title="Welcome to the EstateIQ frontend."
      description="This is the base home page wired into the new layout and routing structure."
      highlights={[
        'Navbar is visible across the main application routes.',
        'Sidebar links switch between placeholder pages.',
        'This page can be extended later without changing the routing base.',
      ]}
    />
  )
}
