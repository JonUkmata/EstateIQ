import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { NavLink, useSearchParams } from 'react-router-dom'
import { verifyEmail } from '../services/api'

type VerificationStatus = 'idle' | 'verifying' | 'verified' | 'failed'

export default function VerifyEmailPage() {
  const [searchParams] = useSearchParams()
  const [token, setToken] = useState(() => searchParams.get('token') ?? '')
  const [tokenError, setTokenError] = useState('')
  const [submitError, setSubmitError] = useState('')
  const [successMessage, setSuccessMessage] = useState('')
  const [status, setStatus] = useState<VerificationStatus>('idle')
  const [showFallbackForm, setShowFallbackForm] = useState(false)
  const autoVerifiedTokenRef = useRef('')

  const queryToken = searchParams.get('token')?.trim() ?? ''
  const hasQueryToken = Boolean(queryToken)

  useEffect(() => {
    if (!queryToken || autoVerifiedTokenRef.current === queryToken) {
      return
    }

    autoVerifiedTokenRef.current = queryToken
    setToken(queryToken)
    setTokenError('')
    setSubmitError('')
    setSuccessMessage('')
    setShowFallbackForm(false)

    async function verifyQueryToken() {
      setStatus('verifying')
      try {
        const response = await verifyEmail({ token: queryToken })
        setSuccessMessage(response.message || 'Email verified successfully. You can now login.')
        setStatus('verified')
      } catch (error) {
        setSubmitError(error instanceof Error ? error.message : 'Email verification failed. Please try again.')
        setStatus('failed')
      }
    }

    void verifyQueryToken()
  }, [queryToken])

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

    setStatus('verifying')
    try {
      const response = await verifyEmail({ token: trimmedToken })
      setSuccessMessage(response.message || 'Email verified successfully. You can now login.')
      setStatus('verified')
    } catch (error) {
      setSubmitError(error instanceof Error ? error.message : 'Email verification failed. Please try again.')
      setStatus('failed')
    }
  }

  const isVerifying = status === 'verifying'
  const isVerified = status === 'verified'
  const isFailed = status === 'failed'
  const showManualTokenForm = showFallbackForm

  return (
    <main className="login-shell auth-flow-shell verify-flow-shell">
      <section className="login-card auth-flow-panel verify-flow-panel">
        <div className="auth-flow-copy">
          <span className="eyebrow">Verify email</span>
          <h1>Confirm your email address.</h1>
          <p className="lead">
            Open the verification link from your inbox and EstateIQ will activate the account
            automatically.
          </p>

          <div className="auth-flow-note">
            <span className="panel-label">Secure verification</span>
            <p>
              {hasQueryToken
                ? 'The verification link is being checked. You can sign in after it is confirmed.'
                : 'This page expects a verification link from your email. Local development can still use the fallback token.'}
            </p>
          </div>
        </div>

        <div className="auth-form-card">
          <div className="login-form-heading">
            <h2>Email verification</h2>
            <p>
              {isVerified
                ? 'Your account is active.'
                : hasQueryToken
                  ? 'Please wait while we confirm your email.'
                  : 'Use the link from your email to complete registration.'}
            </p>
          </div>

          {isVerifying ? (
            <div className="verify-status-card">
              <span className="loading-spinner" aria-hidden="true">
                <span className="loading-spinner-dot" />
              </span>
              <strong>Verifying email...</strong>
              <p>This usually takes a moment.</p>
            </div>
          ) : null}

          {isVerified ? (
            <div className="auth-success demo-token-card field-wide">
              <strong>{successMessage}</strong>
              <p>Your email has been confirmed. You can now sign in.</p>
              <NavLink className="table-action-link" to="/login">
                Continue to login
              </NavLink>
            </div>
          ) : null}

          {isFailed ? (
            <div className="verify-status-card verify-status-card-error">
              <strong>Email verification failed</strong>
              <p>{submitError}</p>
              {hasQueryToken ? (
                <button
                  type="button"
                  onClick={() => {
                    autoVerifiedTokenRef.current = ''
                    setSubmitError('')
                    setStatus('idle')
                    setToken(queryToken)
                    setShowFallbackForm(true)
                  }}
                >
                  Try manually
                </button>
              ) : null}
            </div>
          ) : null}

          {showManualTokenForm ? (
            <form className="property-form auth-form verify-form" onSubmit={handleSubmit} noValidate>
              <label className="field field-wide">
                <span>Verification token</span>
                <textarea
                  aria-invalid={Boolean(tokenError)}
                  placeholder="Paste the verification token"
                  rows={5}
                  value={token}
                  onChange={(event) => {
                    setToken(event.target.value)
                    setTokenError('')
                  }}
                />
                {tokenError ? <small>{tokenError}</small> : null}
              </label>

              {submitError && !isFailed ? (
                <p className="form-message form-message-error login-error-message field-wide">
                  {submitError}
                </p>
              ) : null}

              {successMessage && !isVerified ? (
                <div className="auth-success demo-token-card field-wide">
                <strong>{successMessage}</strong>
                <p>Your email has been confirmed. You can now sign in.</p>
                <NavLink className="table-action-link" to="/login">
                  Continue to login
                </NavLink>
              </div>
              ) : null}

              <div className="form-actions auth-actions">
                <NavLink className="cta-link cta-link-secondary" to="/register">
                  Register
                </NavLink>
                <button type="submit" disabled={isVerifying}>
                  {isVerifying ? 'Verifying...' : 'Verify email'}
                </button>
              </div>
            </form>
          ) : null}

          {!hasQueryToken && !showManualTokenForm && !isVerified ? (
            <div className="form-actions auth-actions">
              <NavLink className="cta-link cta-link-secondary" to="/register">
                Register
              </NavLink>
              <button type="button" onClick={() => setShowFallbackForm(true)}>
                Enter fallback token
              </button>
            </div>
          ) : null}
        </div>
      </section>
    </main>
  )
}
