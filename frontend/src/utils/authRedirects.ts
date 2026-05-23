import { Roles, staffRoles } from '../constants/auth'
import type { AuthUser } from '../types/auth'

export function getAuthenticatedHomePath(user: AuthUser | null) {
  if (!user) {
    return '/'
  }

  if (staffRoles.some((role) => user.roles.includes(role))) {
    return '/dashboard'
  }

  if (user.roles.includes(Roles.User) || user.roles.length === 0) {
    return '/properties'
  }

  return '/properties'
}
