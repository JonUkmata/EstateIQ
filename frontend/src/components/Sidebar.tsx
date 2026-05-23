import { NavLink } from 'react-router-dom'
import { Permissions, Roles } from '../constants/auth'
import { useAuth } from '../context/AuthContext'

type NavigationLink = {
  label: string
  to: string
}

function addLink(links: NavigationLink[], link: NavigationLink) {
  if (!links.some((item) => item.to === link.to)) {
    links.push(link)
  }
}

export default function Sidebar() {
  const { hasPermission, hasRole, isAuthenticated, user } = useAuth()
  const canManageProperties =
    hasPermission(Permissions.CreateProperty)
    || hasPermission(Permissions.EditProperty)
    || hasPermission(Permissions.DeleteProperty)
    || hasPermission(Permissions.UploadPropertyImages)
  const visibleLinks: NavigationLink[] = []

  if (!isAuthenticated) {
    visibleLinks.push(
      { label: 'Login', to: '/login' },
      { label: 'Register', to: '/register' },
    )
  } else {
    if (hasRole(Roles.User)) {
      addLink(visibleLinks, { label: 'Properties', to: '/properties' })
      addLink(visibleLinks, { label: 'Map Search', to: '/map' })
      addLink(visibleLinks, { label: 'Dashboard', to: '/dashboard' })
    }

    if (hasRole(Roles.Agent)) {
      addLink(visibleLinks, { label: 'Dashboard', to: '/dashboard' })
      addLink(visibleLinks, { label: 'Properties', to: '/properties' })
      addLink(visibleLinks, { label: 'Map Search', to: '/map' })
      addLink(visibleLinks, { label: 'My Properties', to: '/my-properties' })
    }

    if (hasRole(Roles.CompanyAdmin)) {
      addLink(visibleLinks, { label: 'Dashboard', to: '/dashboard' })
      addLink(visibleLinks, { label: 'Properties', to: '/properties' })
      addLink(visibleLinks, { label: 'Map Search', to: '/map' })
      addLink(visibleLinks, { label: 'Company Agents', to: '/company/agents' })
    }

    if (hasRole(Roles.Admin)) {
      addLink(visibleLinks, { label: 'Dashboard', to: '/dashboard' })
      addLink(visibleLinks, { label: 'Properties', to: '/properties' })
      addLink(visibleLinks, { label: 'Map Search', to: '/map' })
      addLink(visibleLinks, { label: 'Admin Users', to: '/admin/users' })
    }
  }

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
