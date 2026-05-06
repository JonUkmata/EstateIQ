import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { NavLink, useSearchParams } from 'react-router-dom'
import { verifyEmail } from '../services/api'

export default function VerifyEmailPage() {
  const [searchParams] = useSearchParams()
  const [token, setToken] = useState(() => searchParams.get('token') ?? '')
  const [tokenError, setTokenError] = useState('')
  const [submitError, setSubmitError] = useState('')
  const [successMessage, setSuccessMessage] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  useEffect(() => {
    const queryToken = searchParams.get('token')
    if (queryToken) {
      setToken(queryToken)
    }
  }, [searchParams])

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const trimmedToken = token.trim()
    setTokenError('')
    setSubmitError('')
    setSuccessMessage('')

    if (!trimmedToken) {
      setTokenError('Verification token is required.')
      return
    }

    setIsSubmitting(true)
    try {
      const response = await verifyEmail({ token: trimmedToken })
      setSuccessMessage(response.message || 'Email verified successfully. You can now login.')
    } catch (error) {
      setSubmitError(error instanceof Error ? error.message : 'Email verification failed. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="login-shell auth-shell">
      <section className="login-card auth-card">
        <div>
          <span className="eyebrow">Verify email</span>
          <h1>Confirm your email.</h1>
          <p className="lead">
            Paste the verification token from registration, or open the verification link generated after register.
          </p>
        </div>

        <form className="property-form auth-form verify-form" onSubmit={handleSubmit} noValidate>
          <label className="field field-wide">
            <span>Verification token</span>
            <textarea
              rows={4}
              value={token}
              onChange={(event) => {
                setToken(event.target.value)
                setTokenError('')
              }}
            />
            {tokenError ? <small>{tokenError}</small> : null}
          </label>

          {submitError ? <p className="form-message form-message-error field-wide">{submitError}</p> : null}

          {successMessage ? (
            <div className="auth-success field-wide">
              <strong>{successMessage}</strong>
              <NavLink className="table-action-link" to="/login">
                Continue to login
              </NavLink>
            </div>
          ) : null}

          <div className="form-actions auth-actions">
            <NavLink className="cta-link cta-link-secondary" to="/register">
              Register
            </NavLink>
            <button type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Verifying...' : 'Verify email'}
            </button>
          </div>
        </form>
      </section>
    </main>
  )
}
