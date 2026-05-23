import { useState } from 'react'
import type { FormEvent } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { getAuthenticatedHomePath } from '../utils/authRedirects'

type LoginForm = {
  email: string
  password: string
}

type LoginFormErrors = Partial<Record<keyof LoginForm, string>>

const initialForm: LoginForm = {
  email: '',
  password: '',
}

function validateForm(form: LoginForm) {
  const errors: LoginFormErrors = {}

  if (!form.email.trim()) {
    errors.email = 'Email is required.'
  }

  if (!form.password) {
    errors.password = 'Password is required.'
  }

  return errors
}

function getLoginErrorMessage(error: unknown) {
  const message = error instanceof Error ? error.message : 'Login failed. Please try again.'

  if (message.toLowerCase().includes('email is not verified')) {
    return 'Your email is not verified yet. Please verify your email before logging in.'
  }

  return message
}

export default function LoginPage() {
  const navigate = useNavigate()
  const { login } = useAuth()
  const [form, setForm] = useState<LoginForm>(initialForm)
  const [errors, setErrors] = useState<LoginFormErrors>({})
  const [submitError, setSubmitError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  function updateField(field: keyof LoginForm, value: string) {
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
      const authenticatedUser = await login({
        email: form.email.trim(),
        password: form.password,
      })
      navigate(getAuthenticatedHomePath(authenticatedUser), { replace: true })
    } catch (error) {
      setSubmitError(getLoginErrorMessage(error))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="login-shell login-page-shell">
      <section className="login-card login-panel">
        <div className="login-panel-copy">
          <span className="eyebrow">Login</span>
          <h1>Welcome back to EstateIQ.</h1>
          <p className="lead">
            Sign in with your verified account to continue to your dashboard or property workspace.
          </p>

          <div className="login-support-panel">
            <span className="panel-label">Secure access</span>
            <p>Role-based navigation starts after login, so every user lands in the right place.</p>
          </div>
        </div>

        <div className="login-form-card">
          <div className="login-form-heading">
            <h2>Sign in</h2>
            <p>Use the email and password connected to your EstateIQ account.</p>
          </div>

          <form className="property-form auth-form login-form" onSubmit={handleSubmit} noValidate>
            <label className="field field-wide">
              <span>Email</span>
              <input
                aria-invalid={Boolean(errors.email)}
                autoComplete="email"
                inputMode="email"
                placeholder="you@example.com"
                type="email"
                value={form.email}
                onChange={(event) => updateField('email', event.target.value)}
              />
              {errors.email ? <small>{errors.email}</small> : null}
            </label>

            <label className="field field-wide">
              <span>Password</span>
              <input
                aria-invalid={Boolean(errors.password)}
                autoComplete="current-password"
                placeholder="Enter your password"
                type="password"
                value={form.password}
                onChange={(event) => updateField('password', event.target.value)}
              />
              {errors.password ? <small>{errors.password}</small> : null}
            </label>

            {submitError ? (
              <p className="form-message form-message-error login-error-message field-wide">
                {submitError}
              </p>
            ) : null}

            <div className="form-actions login-form-actions">
              <button type="submit" disabled={isSubmitting}>
                {isSubmitting ? (
                  <span className="state-with-spinner">
                    <span className="loading-spinner" aria-hidden="true">
                      <span className="loading-spinner-dot" />
                    </span>
                    <span>Signing in...</span>
                  </span>
                ) : (
                  'Login'
                )}
              </button>
            </div>
          </form>

          <p className="login-register-prompt">
            Do not have an account? <NavLink to="/register">Create account</NavLink>
          </p>
        </div>
      </section>
    </main>
  )
}
