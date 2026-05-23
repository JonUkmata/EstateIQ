import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import type { ReactNode } from 'react'

type ProtectedRouteProps = {
  children: ReactNode
  roles?: string[]
  permissions?: string[]
  requiredRoles?: string[]
  requiredPermissions?: string[]
  fallbackPath?: string
}

export default function ProtectedRoute({
  children,
  roles = [],
  permissions = [],
  requiredRoles,
  requiredPermissions,
  fallbackPath = '/',
}: ProtectedRouteProps) {
  const location = useLocation()
  const { hasPermission, hasRole, isAuthenticated } = useAuth()
  const allowedRoles = requiredRoles ?? roles
  const allowedPermissions = requiredPermissions ?? permissions

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  const hasRequiredRole =
    allowedRoles.length === 0 || allowedRoles.some((role) => hasRole(role))
  const hasRequiredPermission =
    allowedPermissions.length === 0
    || allowedPermissions.some((permission) => hasPermission(permission))

  if (!hasRequiredRole || !hasRequiredPermission) {
    return <Navigate to={fallbackPath} replace />
  }

  return children
}
