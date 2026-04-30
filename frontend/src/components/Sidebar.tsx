import { NavLink } from 'react-router-dom'

const links = [
  { label: 'Home', to: '/' },
  { label: 'Properties', to: '/properties' },
  { label: 'Map', to: '/map' },
  { label: 'Dashboard', to: '/dashboard' },
]

export default function Sidebar() {
  return (
    <aside className="sidebar">
      <div className="sidebar-card">
        <p className="sidebar-title">Navigation</p>
        <nav className="sidebar-nav" aria-label="Sidebar navigation">
          {links.map((link) => (
            <NavLink
              key={link.to}
              className={({ isActive }) =>
                isActive ? 'sidebar-link sidebar-link-active' : 'sidebar-link'
              }
              to={link.to}
            >
              {link.label}
            </NavLink>
          ))}
        </nav>
      </div>
    </aside>
  )
}
