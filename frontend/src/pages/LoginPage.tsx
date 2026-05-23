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
    return 'Email is not verified. Please verify your email before logging in.'
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
    <main className="login-shell auth-shell">
      <section className="login-card auth-card">
        <div>
          <span className="eyebrow">Login</span>
          <h1>Welcome back.</h1>
          <p className="lead">
            Sign in with a verified account to access protected tools and role-based navigation.
          </p>
        </div>

        <form className="property-form auth-form login-form" onSubmit={handleSubmit} noValidate>
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

          <label className="field field-wide">
            <span>Password</span>
            <input
              autoComplete="current-password"
              type="password"
              value={form.password}
              onChange={(event) => updateField('password', event.target.value)}
            />
            {errors.password ? <small>{errors.password}</small> : null}
          </label>

          {submitError ? <p className="form-message form-message-error field-wide">{submitError}</p> : null}

          <div className="form-actions auth-actions">
            <NavLink className="cta-link cta-link-secondary" to="/register">
              Create account
            </NavLink>
            <button type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Signing in...' : 'Login'}
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
