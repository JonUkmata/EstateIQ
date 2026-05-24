import { useEffect, useMemo, useState } from 'react'
import LoadingSpinner from '../components/LoadingSpinner'
import { getMyCompanyAgents, type Agent } from '../services/api'

type LoadState = 'loading' | 'success' | 'error'

const dateFormatter = new Intl.DateTimeFormat('en-US', {
  year: 'numeric',
  month: 'short',
  day: 'numeric',
})

export default function CompanyAgentsPage() {
  const [agents, setAgents] = useState<Agent[]>([])
  const [loadState, setLoadState] = useState<LoadState>('loading')
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    const controller = new AbortController()

    async function loadAgents() {
      try {
        setLoadState('loading')
        setErrorMessage('')

        const result = await getMyCompanyAgents(controller.signal)
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

    void loadAgents()

    return () => controller.abort()
  }, [])

  const agentCountLabel = useMemo(() => {
    if (loadState === 'loading') {
      return 'Loading'
    }

    return `${agents.length} ${agents.length === 1 ? 'agent' : 'agents'}`
  }, [agents.length, loadState])

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

      <section className="data-panel" aria-live="polite">
        {loadState === 'loading' ? (
          <div className="table-state">
            <p className="state-with-spinner">
              <LoadingSpinner label="Loading company agents" />
              <span>Loading company agents...</span>
            </p>
          </div>
        ) : null}

        {loadState === 'error' ? (
          <div className="table-state table-state-error">
            <p>{errorMessage}</p>
          </div>
        ) : null}

        {loadState === 'success' && agents.length === 0 ? (
          <div className="table-state">
            <p>No agents are connected to your company yet.</p>
          </div>
        ) : null}

        {loadState === 'success' && agents.length > 0 ? (
          <div className="properties-table-wrap">
            <table className="properties-table">
              <thead>
                <tr>
                  <th>First name</th>
                  <th>Last name</th>
                  <th>Email</th>
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
