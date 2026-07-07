import { type FormEvent, useEffect, useMemo, useState } from 'react'
import EmptyState from '../components/EmptyState'
import ErrorState from '../components/ErrorState'
import LoadingState from '../components/LoadingState'
import { createAgent, getMyCompanyAgents, type Agent, type CreateAgentPayload } from '../services/api'
import { buildPhoneNumber, isValidLocalPhoneNumber, phoneCountries } from '../utils/phone'

type LoadState = 'loading' | 'success' | 'error'
type SubmitState = 'idle' | 'submitting' | 'success' | 'error'
type AgentFormState = Omit<CreateAgentPayload, 'phone'> & {
  phoneCountryCode: string
  phoneNumber: string
}
type FormErrors = Partial<Record<keyof AgentFormState, string>>

const initialForm: AgentFormState = {
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  phoneCountryCode: '+383',
  phoneNumber: '',
}

const dateFormatter = new Intl.DateTimeFormat('en-US', {
  year: 'numeric',
  month: 'short',
  day: 'numeric',
})

export default function CompanyAgentsPage() {
  const [agents, setAgents] = useState<Agent[]>([])
  const [loadState, setLoadState] = useState<LoadState>('loading')
  const [submitState, setSubmitState] = useState<SubmitState>('idle')
  const [errorMessage, setErrorMessage] = useState('')
  const [submitMessage, setSubmitMessage] = useState('')
  const [form, setForm] = useState<AgentFormState>(initialForm)
  const [formErrors, setFormErrors] = useState<FormErrors>({})

  async function loadAgents(signal?: AbortSignal) {
    try {
      setLoadState('loading')
      setErrorMessage('')

      const result = await getMyCompanyAgents(signal)
      setAgents(result)
      setLoadState('success')
    } catch (error) {
      if (error instanceof DOMException && error.name === 'AbortError') {
        return
      }

      setErrorMessage(error instanceof Error ? error.message : 'Failed to load company agents.')
      setLoadState('error')
    }
  }

  useEffect(() => { document.title = 'Company Agents | EstateIQ' }, [])

  useEffect(() => {
    const controller = new AbortController()
    void loadAgents(controller.signal)

    return () => controller.abort()
  }, [])

  const agentCountLabel = useMemo(() => {
    if (loadState === 'loading') {
      return 'Loading'
    }

    return `${agents.length} ${agents.length === 1 ? 'agent' : 'agents'}`
  }, [agents.length, loadState])

  function updateFormField(field: keyof AgentFormState, value: string) {
    setForm((current) => ({
      ...current,
      [field]: value,
    }))
    setFormErrors((current) => ({
      ...current,
      [field]: undefined,
    }))
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const validation = validateForm(form)
    setFormErrors(validation)
    setSubmitMessage('')

    if (Object.keys(validation).length > 0) {
      setSubmitState('error')
      setSubmitMessage('Please fix the highlighted fields.')
      return
    }

    try {
      setSubmitState('submitting')
      await createAgent({
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        email: form.email.trim(),
        password: form.password,
        phone: buildPhoneNumber(form.phoneCountryCode, form.phoneNumber),
      })
      setForm(initialForm)
      setFormErrors({})
      setSubmitState('success')
      setSubmitMessage('Agent created successfully.')
      await loadAgents()
    } catch (error) {
      setSubmitState('error')
      setSubmitMessage(error instanceof Error ? error.message : 'Failed to create agent.')
    }
  }

  return (
    <section className="content-stack">
      <div className="section-heading">
        <div>
          <span className="eyebrow">CompanyAdmin</span>
          <h1>Company Agents</h1>
        </div>
        <span className={`response-badge response-badge-${loadState}`}>
          {loadState === 'error' ? 'Error' : agentCountLabel}
        </span>
      </div>

      <section className="form-panel">
        <div className="form-panel-header">
          <div>
            <span className="panel-label">Create</span>
            <h2>Add Agent</h2>
          </div>
          {submitMessage ? (
            <span
              className={`form-message ${
                submitState === 'success' ? 'form-message-success' : 'form-message-error'
              }`}
            >
              {submitMessage}
            </span>
          ) : null}
        </div>

        <form className="property-form" onSubmit={handleSubmit} noValidate>
          <label className="field">
            <span>First name</span>
            <input
              value={form.firstName}
              onChange={(event) => updateFormField('firstName', event.target.value)}
              autoComplete="given-name"
            />
            {formErrors.firstName ? <small>{formErrors.firstName}</small> : null}
          </label>

          <label className="field">
            <span>Last name</span>
            <input
              value={form.lastName}
              onChange={(event) => updateFormField('lastName', event.target.value)}
              autoComplete="family-name"
            />
            {formErrors.lastName ? <small>{formErrors.lastName}</small> : null}
          </label>

          <label className="field">
            <span>Email</span>
            <input
              value={form.email}
              onChange={(event) => updateFormField('email', event.target.value)}
              autoComplete="email"
              inputMode="email"
              type="email"
            />
            {formErrors.email ? <small>{formErrors.email}</small> : null}
          </label>

          <label className="field">
            <span>Temporary password</span>
            <input
              value={form.password}
              onChange={(event) => updateFormField('password', event.target.value)}
              autoComplete="new-password"
              type="password"
            />
            {formErrors.password ? <small>{formErrors.password}</small> : null}
          </label>

          <div className="field field-wide">
            <span>Phone</span>
            <div className="phone-input-row">
              <select
                aria-label="Phone country code"
                value={form.phoneCountryCode}
                onChange={(event) => updateFormField('phoneCountryCode', event.target.value)}
              >
                {phoneCountries.map((country) => (
                  <option key={country.code} value={country.code}>
                    {country.label}
                  </option>
                ))}
              </select>
              <input
                aria-invalid={Boolean(formErrors.phoneNumber)}
                value={form.phoneNumber}
                onChange={(event) => updateFormField('phoneNumber', event.target.value)}
                autoComplete="tel-national"
                inputMode="tel"
                placeholder="44 123 456"
              />
            </div>
            {formErrors.phoneNumber ? <small>{formErrors.phoneNumber}</small> : null}
          </div>

          <div className="form-actions">
            <button type="submit" disabled={submitState === 'submitting'}>
              {submitState === 'submitting' ? 'Creating...' : 'Create Agent'}
            </button>
          </div>
        </form>
      </section>

      <section className="data-panel" aria-live="polite">
        {loadState === 'loading' ? <LoadingState message="Loading company agents..." /> : null}
        {loadState === 'error' ? <ErrorState message={errorMessage} /> : null}
        {loadState === 'success' && agents.length === 0 ? (
          <EmptyState message="No agents are connected to your company yet." />
        ) : null}

        {loadState === 'success' && agents.length > 0 ? (
          <div className="properties-table-wrap">
            <table className="properties-table">
              <thead>
                <tr>
                  <th>First name</th>
                  <th>Last name</th>
                  <th>Email</th>
                  <th>Phone</th>
                  <th>Status</th>
                  <th>Created</th>
                </tr>
              </thead>
              <tbody>
                {agents.map((agent) => (
                  <tr key={agent.id}>
                    <td data-label="First name">{agent.firstName}</td>
                    <td data-label="Last name">{agent.lastName}</td>
                    <td data-label="Email">{agent.email}</td>
                    <td data-label="Phone">{agent.phone || '-'}</td>
                    <td data-label="Status">
                      <span className="status-pill">{agent.isActive ? 'Active' : 'Inactive'}</span>
                    </td>
                    <td data-label="Created">{formatCreatedAt(agent.createdAt)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : null}
      </section>
    </section>
  )
}

function formatCreatedAt(createdAt?: string) {
  if (!createdAt) {
    return 'Not available'
  }

  const parsed = new Date(createdAt)
  if (Number.isNaN(parsed.getTime())) {
    return 'Not available'
  }

  return dateFormatter.format(parsed)
}

function validateForm(form: AgentFormState) {
  const errors: FormErrors = {}
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
    errors.password = 'Temporary password is required.'
  } else if (form.password.length < 8) {
    errors.password = 'Temporary password must be at least 8 characters.'
  }

  if (!form.phoneNumber.trim()) {
    errors.phoneNumber = 'Phone number is required.'
  } else if (!isValidLocalPhoneNumber(form.phoneNumber)) {
    errors.phoneNumber = 'Enter a valid phone number.'
  }

  return errors
}
