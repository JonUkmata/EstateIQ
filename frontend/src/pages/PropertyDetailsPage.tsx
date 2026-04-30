import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { getPropertyById, type PropertyDetails } from '../services/api'

type LoadState = 'loading' | 'success' | 'error'

const currencyFormatter = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  maximumFractionDigits: 0,
})

export default function PropertyDetailsPage() {
  const { id } = useParams()
  const [property, setProperty] = useState<PropertyDetails | null>(null)
  const [loadState, setLoadState] = useState<LoadState>('loading')
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    const parsedId = Number(id)
    if (!Number.isInteger(parsedId) || parsedId <= 0) {
      setLoadState('error')
      setErrorMessage('Invalid property id.')
      return
    }

    const controller = new AbortController()

    async function loadProperty() {
      try {
        setLoadState('loading')
        setErrorMessage('')
        const result = await getPropertyById(parsedId, controller.signal)
        setProperty(result)
        setLoadState('success')
      } catch (error) {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return
        }

        setLoadState('error')
        setErrorMessage(error instanceof Error ? error.message : 'Failed to load property details.')
      }
    }

    void loadProperty()
    return () => controller.abort()
  }, [id])

  if (loadState === 'loading') {
    return (
      <section className="content-stack">
        <div className="section-heading">
          <h1>Property Details</h1>
          <span className="response-badge response-badge-loading">Loading</span>
        </div>
      </section>
    )
  }

  if (loadState === 'error' || !property) {
    return (
      <section className="content-stack">
        <div className="section-heading">
          <h1>Property Details</h1>
          <span className="response-badge response-badge-error">Error</span>
        </div>
        <section className="data-panel">
          <div className="table-state table-state-error">
            <p>{errorMessage || 'Property was not found.'}</p>
          </div>
        </section>
      </section>
    )
  }

  return (
    <section className="content-stack">
      <div className="section-heading">
        <div>
          <span className="eyebrow">Property</span>
          <h1>{property.title}</h1>
        </div>
        <Link className="top-nav-link" to="/properties">
          Back to list
        </Link>
        <Link className="top-nav-link" to={`/properties/${property.id}/edit`}>
          Edit property
        </Link>
      </div>

      <section className="details-grid">
        <article className="details-card">
          <h2>Overview</h2>
          <p>{property.description || 'No description provided.'}</p>
          <dl>
            <div><dt>Price</dt><dd>{currencyFormatter.format(property.price)}</dd></div>
            <div><dt>Bedrooms</dt><dd>{property.bedrooms ?? '-'}</dd></div>
            <div><dt>Bathrooms</dt><dd>{property.bathrooms ?? '-'}</dd></div>
            <div><dt>Area</dt><dd>{property.area} m²</dd></div>
          </dl>
        </article>

        <article className="details-card">
          <h2>Location</h2>
          <dl>
            <div><dt>Address</dt><dd>{property.address}</dd></div>
            <div><dt>City</dt><dd>{property.city}</dd></div>
            <div><dt>Latitude</dt><dd>{property.latitude ?? '-'}</dd></div>
            <div><dt>Longitude</dt><dd>{property.longitude ?? '-'}</dd></div>
          </dl>
        </article>

        <article className="details-card">
          <h2>Classification</h2>
          <dl>
            <div><dt>Property Type</dt><dd>{property.propertyType.name}</dd></div>
            <div><dt>Property Status</dt><dd>{property.propertyStatus.name}</dd></div>
          </dl>
        </article>

        <article className="details-card">
          <h2>Ownership</h2>
          <dl>
            <div><dt>Company</dt><dd>{property.company.name}</dd></div>
            <div><dt>Agent</dt><dd>{property.agent.firstName} {property.agent.lastName}</dd></div>
          </dl>
        </article>
      </section>
    </section>
  )
}
