import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { Permissions } from '../constants/auth'
import { useAuth } from '../context/AuthContext'
import { getPropertyById, getPropertyImages, type PropertyDetails } from '../services/api'
import ErrorState from '../components/ErrorState'
import LoadingSpinner from '../components/LoadingSpinner'
import PropertyImageGallery from '../components/properties/PropertyImageGallery'
import PropertyImageUpload from '../components/properties/PropertyImageUpload'
import type { PropertyImage } from '../types/files'

type LoadState = 'loading' | 'success' | 'error'

const currencyFormatter = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  maximumFractionDigits: 0,
})

export default function PropertyDetailsPage() {
  const { id } = useParams()
  const { hasPermission } = useAuth()
  const canEditProperty = hasPermission(Permissions.EditProperty)
  const canUploadPropertyImages = hasPermission(Permissions.UploadPropertyImages)
  const [property, setProperty] = useState<PropertyDetails | null>(null)
  const [images, setImages] = useState<PropertyImage[]>([])
  const [imagesLoading, setImagesLoading] = useState(false)
  const [imagesError, setImagesError] = useState('')
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
        document.title = `${result.title} | EstateIQ`
        setImages(result.images ?? [])
        setLoadState('success')

        setImagesLoading(true)
        try {
          const imageResult = await getPropertyImages(parsedId, controller.signal)
          setImages(imageResult)
          setImagesError('')
        } catch (imageError) {
          if (imageError instanceof DOMException && imageError.name === 'AbortError') {
            return
          }

          setImagesError(imageError instanceof Error ? imageError.message : 'Failed to load property images.')
        } finally {
          setImagesLoading(false)
        }
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

  async function refreshImages() {
    const parsedId = Number(id)
    if (!Number.isInteger(parsedId) || parsedId <= 0) {
      return
    }

    try {
      setImagesLoading(true)
      setImagesError('')
      const imageResult = await getPropertyImages(parsedId)
      setImages(imageResult)
    } catch (error) {
      setImagesError(error instanceof Error ? error.message : 'Failed to refresh property images.')
    } finally {
      setImagesLoading(false)
    }
  }

  if (loadState === 'loading') {
    return (
      <section className="content-stack">
        <div className="section-heading">
          <h1>Property Details</h1>
          <span className="response-badge response-badge-loading state-with-spinner">
            <LoadingSpinner label="Loading property details" />
            <span>Loading</span>
          </span>
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
          <ErrorState message={errorMessage || 'Property was not found.'} />
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
        {canEditProperty ? (
          <Link className="top-nav-link" to={`/properties/${property.id}/edit`}>
            Edit property
          </Link>
        ) : null}
      </div>

      <section className="details-grid">
        <article className="details-card">
          <h2>Overview</h2>
          <p>{property.description || 'No description provided.'}</p>
          <dl>
            <div><dt>Price</dt><dd>{currencyFormatter.format(property.price)}</dd></div>
            <div><dt>Bedrooms</dt><dd>{property.bedrooms ?? '-'}</dd></div>
            <div><dt>Bathrooms</dt><dd>{property.bathrooms ?? '-'}</dd></div>
            <div><dt>Area</dt><dd>{property.area} m2</dd></div>
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

      {imagesError ? (
        <section className="data-panel">
          <ErrorState message={imagesError} />
        </section>
      ) : null}

      <PropertyImageGallery images={images} isLoading={imagesLoading} />

      {canUploadPropertyImages ? (
        <PropertyImageUpload
          propertyId={property.id}
          currentImageCount={images.length}
          onUploaded={refreshImages}
        />
      ) : null}
    </section>
  )
}
