import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import type { ReactNode } from 'react'

type ProtectedRouteProps = {
  children: ReactNode
  roles?: string[]
  permissions?: string[]
  fallbackPath?: string
}

export default function ProtectedRoute({
  children,
  roles = [],
  permissions = [],
  fallbackPath = '/',
}: ProtectedRouteProps) {
  const location = useLocation()
  const { hasPermission, hasRole, isAuthenticated } = useAuth()

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  const hasRequiredRole = roles.length === 0 || roles.some((role) => hasRole(role))
  const hasRequiredPermission =
    permissions.length === 0 || permissions.some((permission) => hasPermission(permission))

  if (!hasRequiredRole || !hasRequiredPermission) {
    return <Navigate to={fallbackPath} replace />
  }

  return children
}
