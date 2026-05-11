import { NavLink } from 'react-router-dom'
import { Permissions, staffRoles } from '../constants/auth'
import { useAuth } from '../context/AuthContext'

const links = [
  { label: 'Home', to: '/' },
  { label: 'Properties', to: '/properties' },
  { label: 'Map', to: '/map' },
]

export default function Sidebar() {
  const { hasPermission, hasRole, user } = useAuth()
  const canUseDashboard = staffRoles.some((role) => hasRole(role))
  const canManageProperties =
    hasPermission(Permissions.CreateProperty)
    || hasPermission(Permissions.EditProperty)
    || hasPermission(Permissions.DeleteProperty)
    || hasPermission(Permissions.UploadPropertyImages)
  const visibleLinks = [
    ...links,
    ...(canUseDashboard ? [{ label: 'Dashboard', to: '/dashboard' }] : []),
  ]

  return (
    <aside className="sidebar">
      <div className="sidebar-card">
        <p className="sidebar-title">Navigation</p>
        <nav className="sidebar-nav" aria-label="Sidebar navigation">
          {visibleLinks.map((link) => (
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
        {canManageProperties ? (
          <div className="sidebar-auth-card">
            <span>Property management</span>
            <small>Actions enabled</small>
          </div>
        ) : null}
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
