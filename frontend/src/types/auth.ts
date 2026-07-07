export type RegisterRequest = {
  firstName: string
  lastName: string
  email: string
  phone: string
  password: string
  confirmPassword: string
}

export type RegisterResponse = {
  message: string
  verificationEmailSent: boolean
  verificationToken?: string | null
}

export type VerifyEmailRequest = {
  token: string
}

export type VerifyEmailResponse = {
  message: string
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

export type RefreshTokenResponse = {
  accessToken: string
  expiresAt: string
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
