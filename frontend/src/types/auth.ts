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
