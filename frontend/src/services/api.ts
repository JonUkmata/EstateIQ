const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/$/, '')

function buildApiUrl(path: string) {
  return `${apiBaseUrl}${path}`
}

export async function getApiTestMessage(signal?: AbortSignal) {
  const response = await fetch(buildApiUrl('/api/test'), {
    headers: {
      Accept: 'text/plain',
    },
    signal,
  })

  if (!response.ok) {
    throw new Error(`Request failed with status ${response.status}`)
  }

  return response.text()
}
