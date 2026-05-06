export type RegisterRequest = {
  firstName: string
  lastName: string
  email: string
  password: string
  confirmPassword: string
}

export type RegisterResponse = {
  message: string
  verificationToken: string
}

export type LoginRequest = {
  email: string
  password: string
}

export type AuthUser = {
  id: string
  firstName: string
  lastName: string
  email: string
  roles: string[]
  permissions: string[]
}

export type LoginResponse = {
  accessToken: string
  expiresAt: string
  refreshToken: string
  refreshTokenExpiresAt: string
  user: AuthUser
}

export type LogoutResponse = {
  message: string
}

export type AuthSession = {
  accessToken: string
  expiresAt: string
  refreshToken: string
  refreshTokenExpiresAt: string
  user: AuthUser
}
