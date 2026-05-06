import { NavLink } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const links = [
  { label: 'Home', to: '/' },
  { label: 'Properties', to: '/properties' },
  { label: 'Map', to: '/map' },
  { label: 'Dashboard', to: '/dashboard' },
]

export default function Sidebar() {
  const { user } = useAuth()

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
        {user ? (
          <div className="sidebar-auth-card">
            <span>{user.email}</span>
            <small>{user.roles.length > 0 ? user.roles.join(', ') : 'User'}</small>
          </div>
        ) : null}
      </div>
    </aside>
  )
}
