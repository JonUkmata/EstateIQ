import { useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { NavLink } from 'react-router-dom'
import { registerUser } from '../services/api'
import type { RegisterRequest, RegisterResponse } from '../types/auth'

type RegisterFormErrors = Partial<Record<keyof RegisterRequest, string>>

const initialForm: RegisterRequest = {
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  confirmPassword: '',
}

function validateForm(form: RegisterRequest) {
  const errors: RegisterFormErrors = {}
  const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

  if (!form.firstName.trim()) {
    errors.firstName = 'First name is required.'
  }

  if (!form.lastName.trim()) {
    errors.lastName = 'Last name is required.'
  }

  if (!form.email.trim()) {
    errors.email = 'Email is required.'
  } else if (!emailPattern.test(form.email.trim())) {
    errors.email = 'Enter a valid email address.'
  }

  if (!form.password) {
    errors.password = 'Password is required.'
  } else if (form.password.length < 8) {
    errors.password = 'Password must be at least 8 characters.'
  }

  if (!form.confirmPassword) {
    errors.confirmPassword = 'Confirm your password.'
  } else if (form.password !== form.confirmPassword) {
    errors.confirmPassword = 'Passwords must match.'
  }

  return errors
}

export default function RegisterPage() {
  const [form, setForm] = useState<RegisterRequest>(initialForm)
  const [errors, setErrors] = useState<RegisterFormErrors>({})
  const [submitError, setSubmitError] = useState('')
  const [success, setSuccess] = useState<RegisterResponse | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const verifyLink = useMemo(() => {
    if (!success?.verificationToken) {
      return ''
    }

    return `/verify-email?token=${encodeURIComponent(success.verificationToken)}`
  }, [success])

  function updateField(field: keyof RegisterRequest, value: string) {
    setForm((current) => ({
      ...current,
      [field]: value,
    }))
    setErrors((current) => ({
      ...current,
      [field]: undefined,
    }))
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const validationErrors = validateForm(form)
    setErrors(validationErrors)
    setSubmitError('')

    if (Object.keys(validationErrors).length > 0) {
      return
    }

    setIsSubmitting(true)
    try {
      const response = await registerUser({
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        email: form.email.trim(),
        password: form.password,
        confirmPassword: form.confirmPassword,
      })
      setSuccess(response)
      setForm(initialForm)
    } catch (error) {
      setSuccess(null)
      setSubmitError(error instanceof Error ? error.message : 'Registration failed. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="login-shell auth-shell">
      <section className="login-card auth-card">
        <div>
          <span className="eyebrow">Register</span>
          <h1>Create your account.</h1>
          <p className="lead">
            Register as a buyer or renter to access user features. Business accounts are verified separately.
          </p>
        </div>

        <form className="property-form auth-form" onSubmit={handleSubmit} noValidate>
          <label className="field">
            <span>First name</span>
            <input
              autoComplete="given-name"
              value={form.firstName}
              onChange={(event) => updateField('firstName', event.target.value)}
            />
            {errors.firstName ? <small>{errors.firstName}</small> : null}
          </label>

          <label className="field">
            <span>Last name</span>
            <input
              autoComplete="family-name"
              value={form.lastName}
              onChange={(event) => updateField('lastName', event.target.value)}
            />
            {errors.lastName ? <small>{errors.lastName}</small> : null}
          </label>

          <label className="field field-wide">
            <span>Email</span>
            <input
              autoComplete="email"
              inputMode="email"
              type="email"
              value={form.email}
              onChange={(event) => updateField('email', event.target.value)}
            />
            {errors.email ? <small>{errors.email}</small> : null}
          </label>

          <label className="field">
            <span>Password</span>
            <input
              autoComplete="new-password"
              type="password"
              value={form.password}
              onChange={(event) => updateField('password', event.target.value)}
            />
            {errors.password ? <small>{errors.password}</small> : null}
          </label>

          <label className="field">
            <span>Confirm password</span>
            <input
              autoComplete="new-password"
              type="password"
              value={form.confirmPassword}
              onChange={(event) => updateField('confirmPassword', event.target.value)}
            />
            {errors.confirmPassword ? <small>{errors.confirmPassword}</small> : null}
          </label>

          {submitError ? <p className="form-message form-message-error field-wide">{submitError}</p> : null}

          {success ? (
            <div className="auth-success field-wide">
              <strong>{success.message || 'Registration successful.'}</strong>
              <span>Verification token</span>
              <code>{success.verificationToken}</code>
              {verifyLink ? (
                <NavLink className="table-action-link" to={verifyLink}>
                  Continue to email verification
                </NavLink>
              ) : null}
            </div>
          ) : null}

          <div className="form-actions auth-actions">
            <NavLink className="cta-link cta-link-secondary" to="/login">
              Login
            </NavLink>
            <button type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Creating account...' : 'Create account'}
            </button>
          </div>
        </form>

        <aside className="company-contact-box">
          <span className="panel-label">Company access</span>
          <p>Jeni kompani? Na kontaktoni për verifikim.</p>
        </aside>
      </section>
    </main>
  )
}
