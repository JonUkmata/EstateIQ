import { useEffect, useMemo, useState } from 'react'
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

  const hasQueryToken = useMemo(() => Boolean(searchParams.get('token')), [searchParams])

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
    <main className="login-shell auth-flow-shell verify-flow-shell">
      <section className="login-card auth-flow-panel verify-flow-panel">
        <div className="auth-flow-copy">
          <span className="eyebrow">Verify email</span>
          <h1>Confirm the demo verification token.</h1>
          <p className="lead">
            Verification accepts the token from the registration link or a token pasted manually
            from the demo registration result.
          </p>

          <div className="auth-flow-note">
            <span className="panel-label">Token source</span>
            <p>
              {hasQueryToken
                ? 'A token was detected in the URL and has been placed in the form.'
                : 'Paste the simulated verification token from the Register page to continue.'}
            </p>
          </div>
        </div>

        <div className="auth-form-card">
          <div className="login-form-heading">
            <h2>Email verification</h2>
            <p>This demo verifies accounts with a generated token. No real email is sent.</p>
          </div>

          <form className="property-form auth-form verify-form" onSubmit={handleSubmit} noValidate>
            <label className="field field-wide">
              <span>Verification token</span>
              <textarea
                aria-invalid={Boolean(tokenError)}
                placeholder="Paste the simulated verification token"
                rows={5}
                value={token}
                onChange={(event) => {
                  setToken(event.target.value)
                  setTokenError('')
                }}
              />
              {tokenError ? <small>{tokenError}</small> : null}
            </label>

            {submitError ? (
              <p className="form-message form-message-error login-error-message field-wide">
                {submitError}
              </p>
            ) : null}

            {successMessage ? (
              <div className="auth-success demo-token-card field-wide">
                <strong>{successMessage}</strong>
                <p>The account is verified for this demo flow. You can now sign in.</p>
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
                {isSubmitting ? (
                  <span className="state-with-spinner">
                    <span className="loading-spinner" aria-hidden="true">
                      <span className="loading-spinner-dot" />
                    </span>
                    <span>Verifying...</span>
                  </span>
                ) : (
                  'Verify email'
                )}
              </button>
            </div>
          </form>
        </div>
      </section>
    </main>
  )
}
