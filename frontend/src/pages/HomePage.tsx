import { useEffect, useState } from 'react'
import PagePlaceholder from '../components/PagePlaceholder'
import { getApiTestMessage } from '../services/api'

export default function HomePage() {
  const [apiResponse, setApiResponse] = useState('Loading response from backend...')
  const [apiStatus, setApiStatus] = useState<'loading' | 'success' | 'error'>('loading')

  useEffect(() => {
    const controller = new AbortController()

    async function loadApiResponse() {
      try {
        const response = await getApiTestMessage(controller.signal)
        setApiResponse(response)
        setApiStatus('success')
      } catch (error) {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return
        }

        const errorMessage =
          error instanceof Error ? error.message : 'Failed to load backend response.'

        setApiResponse(errorMessage)
        setApiStatus('error')
      }
    }

    void loadApiResponse()

    return () => controller.abort()
  }, [])

  return (
    <PagePlaceholder
      badge="Home"
      title="Welcome to the EstateIQ frontend."
      description="This home page now calls the backend test endpoint and shows the response directly in the UI."
      highlights={[
        'Navbar is visible across the main application routes.',
        'Sidebar links switch between placeholder pages.',
        'Backend response is loaded from GET /api/test through the frontend API service.',
      ]}
    >
      <article className="response-card">
        <div className="response-header">
          <div>
            <p className="panel-label">Backend response</p>
            <h2 className="response-title">GET /api/test</h2>
          </div>

          <span
            className={
              apiStatus === 'success'
                ? 'response-badge response-badge-success'
                : apiStatus === 'error'
                  ? 'response-badge response-badge-error'
                  : 'response-badge response-badge-loading'
            }
          >
            {apiStatus}
          </span>
        </div>

        <pre className="response-output">{apiResponse}</pre>
      </article>
    </PagePlaceholder>
  )
}
