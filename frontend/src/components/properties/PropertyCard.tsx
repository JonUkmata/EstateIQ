import { Link } from 'react-router-dom'
import { buildFileUrl, type Property } from '../../services/api'

type PropertyCardProps = {
  property: Property
  canEdit: boolean
  canDelete: boolean
  isDeleting: boolean
  onDelete: (property: Property) => void
}

const currencyFormatter = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  maximumFractionDigits: 0,
})

const descriptionMaxLength = 120

export default function PropertyCard({
  property,
  canEdit,
  canDelete,
  isDeleting,
  onDelete,
}: PropertyCardProps) {
  const statusName = property.propertyStatus?.name ?? 'Unknown'
  const statusColor = property.propertyStatus?.colorCode
  const typeName = property.propertyType?.name
  const coverUrl = property.coverImageUrl ? buildFileUrl(property.coverImageUrl) : null
  const locationLabel = [property.address, property.city].filter(Boolean).join(', ')
  const specs = buildSpecs(property)
  const description = truncateDescription(property.description)

  return (
    <article className="property-card">
      <Link className="property-card-media" to={`/properties/${property.id}`} aria-label={`View ${property.title}`}>
        {coverUrl ? (
          <img src={coverUrl} alt="" loading="lazy" />
        ) : (
          <div className="property-card-placeholder" aria-hidden="true">
            <span>No image</span>
          </div>
        )}
        <span
          className="property-card-status"
          style={statusColor ? { backgroundColor: `${statusColor}22`, color: statusColor } : undefined}
        >
          {statusName}
        </span>
      </Link>

      <div className="property-card-body">
        <p className="property-card-price">{currencyFormatter.format(property.price)}</p>
        <h2 className="property-card-title">{property.title || `${typeName ?? 'Property'} in ${property.city}`}</h2>
        {typeName ? <p className="property-card-type">{typeName}</p> : null}
        {locationLabel ? <p className="property-card-location">{locationLabel}</p> : null}
        {specs.length > 0 ? (
          <ul className="property-card-specs" aria-label="Property specifications">
            {specs.map((spec) => (
              <li key={spec}>{spec}</li>
            ))}
          </ul>
        ) : null}
        {description ? <p className="property-card-description">{description}</p> : null}
      </div>

      <div className="property-card-actions">
        <Link className="property-card-details" to={`/properties/${property.id}`}>
          View Details
        </Link>
        {canEdit ? (
          <Link className="property-card-edit" to={`/properties/${property.id}/edit`}>
            Edit
          </Link>
        ) : null}
        {canDelete ? (
          <button
            type="button"
            className="property-card-delete"
            onClick={() => onDelete(property)}
            disabled={isDeleting}
          >
            {isDeleting ? 'Deleting...' : 'Delete'}
          </button>
        ) : null}
      </div>
    </article>
  )
}

function buildSpecs(property: Property) {
  const specs: string[] = []

  if (property.area > 0) {
    specs.push(`${property.area} m²`)
  }

  if (typeof property.bedrooms === 'number') {
    specs.push(`${property.bedrooms} bed${property.bedrooms === 1 ? '' : 's'}`)
  }

  if (typeof property.bathrooms === 'number') {
    specs.push(`${property.bathrooms} bath${property.bathrooms === 1 ? '' : 's'}`)
  }

  return specs
}

function truncateDescription(description?: string | null) {
  if (!description?.trim()) {
    return null
  }

  const trimmed = description.trim()

  if (trimmed.length <= descriptionMaxLength) {
    return trimmed
  }

  return `${trimmed.slice(0, descriptionMaxLength).trimEnd()}…`
}
