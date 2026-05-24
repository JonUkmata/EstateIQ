import { useEffect, useMemo, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { Permissions } from '../constants/auth'
import { useAuth } from '../context/AuthContext'
import {
  deleteProperty,
  getProperties,
  getPropertyStatuses,
  getPropertyTypes,
  type PagedResult,
  type Property,
  type PropertyStatus,
  type PropertyType,
} from '../services/api'
import EmptyState from '../components/EmptyState'
import ErrorState from '../components/ErrorState'
import LoadingState from '../components/LoadingState'
import PropertyCard from '../components/properties/PropertyCard'

type LoadState = 'loading' | 'success' | 'error'

type PropertyFilterState = {
  city: string
  propertyTypeId: string
  propertyStatusId: string
  minPrice: string
  maxPrice: string
}

const initialFilterState: PropertyFilterState = {
  city: '',
  propertyTypeId: '',
  propertyStatusId: '',
  minPrice: '',
  maxPrice: '',
}

const propertiesPageSize = 10
const propertyQueryKeys = ['search', 'city', 'propertyTypeId', 'propertyStatusId', 'minPrice', 'maxPrice'] as const

export default function PropertiesPage() {
  const [searchParams] = useSearchParams()
  const { hasPermission } = useAuth()
  const canCreateProperty = hasPermission(Permissions.CreateProperty)
  const canEditProperty = hasPermission(Permissions.EditProperty)
  const canDeleteProperty = hasPermission(Permissions.DeleteProperty)
  const [properties, setProperties] = useState<Property[]>([])
  const [propertyTypes, setPropertyTypes] = useState<PropertyType[]>([])
  const [propertyStatuses, setPropertyStatuses] = useState<PropertyStatus[]>([])
  const [cityOptions, setCityOptions] = useState<string[]>([])
  const [filters, setFilters] = useState<PropertyFilterState>(() => ({
    city: searchParams.get('city') ?? '',
    propertyTypeId: searchParams.get('propertyTypeId') ?? '',
    propertyStatusId: searchParams.get('propertyStatusId') ?? '',
    minPrice: searchParams.get('minPrice') ?? '',
    maxPrice: searchParams.get('maxPrice') ?? '',
  }))
  const [pagination, setPagination] = useState<PagedResult<Property>>({
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: propertiesPageSize,
    totalPages: 0,
  })
  const [currentPage, setCurrentPage] = useState(1)
  const [loadState, setLoadState] = useState<LoadState>('loading')
  const [deletingPropertyId, setDeletingPropertyId] = useState<number | null>(null)
  const [propertyPendingDelete, setPropertyPendingDelete] = useState<Property | null>(null)
  const [errorMessage, setErrorMessage] = useState('')
  const [deleteMessage, setDeleteMessage] = useState<{ text: string; type: 'success' | 'error' } | null>(null)
  const [searchTerm, setSearchTerm] = useState(() => searchParams.get('search') ?? '')
  const debouncedSearchTerm = useDebouncedValue(searchTerm, 350)
  const debouncedFilters = useDebouncedValue(filters, 350)

  useEffect(() => {
    const controller = new AbortController()

    async function loadLookups() {
      try {
        setErrorMessage('')

        const [typesResult, statusesResult, propertiesResult] = await Promise.all([
          getPropertyTypes(controller.signal),
          getPropertyStatuses(controller.signal),
          getProperties(controller.signal, { pageSize: 100 }),
        ])

        setPropertyTypes(typesResult)
        setPropertyStatuses(statusesResult)
        setCityOptions(getUniqueCities(propertiesResult.items))
      } catch (error) {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return
        }

        setErrorMessage(error instanceof Error ? error.message : 'Failed to load properties.')
        setLoadState('error')
      }
    }

    void loadLookups()

    return () => controller.abort()
  }, [])

  useEffect(() => {
    const controller = new AbortController()

    async function loadProperties() {
      try {
        setLoadState('loading')
        setErrorMessage('')

        const propertiesResult = await getProperties(controller.signal, {
          search: debouncedSearchTerm,
          city: debouncedFilters.city,
          propertyTypeId: debouncedFilters.propertyTypeId,
          propertyStatusId: debouncedFilters.propertyStatusId,
          minPrice: debouncedFilters.minPrice,
          maxPrice: debouncedFilters.maxPrice,
          page: currentPage,
          pageSize: propertiesPageSize,
        })

        setProperties(propertiesResult.items)
        setPagination(propertiesResult)
        setLoadState('success')
      } catch (error) {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return
        }

        setErrorMessage(error instanceof Error ? error.message : 'Failed to load properties.')
        setLoadState('error')
      }
    }

    void loadProperties()

    return () => controller.abort()
  }, [currentPage, debouncedFilters, debouncedSearchTerm])

  const propertyCountLabel = useMemo(() => {
    if (loadState === 'loading') {
      return 'Loading'
    }

    return `${pagination.totalCount} ${pagination.totalCount === 1 ? 'property' : 'properties'}`
  }, [loadState, pagination.totalCount])
  const mapSearchPath = useMemo(
    () =>
      buildPropertyQueryPath('/map', {
        search: searchTerm,
        ...filters,
      }),
    [filters, searchTerm],
  )

  function updateFilterField(field: keyof PropertyFilterState, value: string) {
    setCurrentPage(1)
    setFilters((current) => ({
      ...current,
      [field]: value,
    }))
  }

  function updateSearchTerm(value: string) {
    setCurrentPage(1)
    setSearchTerm(value)
  }

  function resetFilters() {
    setCurrentPage(1)
    setSearchTerm('')
    setFilters(initialFilterState)
  }

  function changePage(page: number) {
    const nextPage = Math.min(Math.max(page, 1), Math.max(pagination.totalPages, 1))
    setCurrentPage(nextPage)
  }

  async function handleDeleteProperty() {
    if (!propertyPendingDelete || !canDeleteProperty) {
      return
    }

    const property = propertyPendingDelete

    try {
      setDeletingPropertyId(property.id)
      setDeleteMessage(null)

      await deleteProperty(property.id)

      const nextPage =
        properties.length === 1 && currentPage > 1
          ? currentPage - 1
          : currentPage

      if (nextPage !== currentPage) {
        setCurrentPage(nextPage)
      } else {
        const refreshedProperties = await getProperties(undefined, {
          search: debouncedSearchTerm,
          city: debouncedFilters.city,
          propertyTypeId: debouncedFilters.propertyTypeId,
          propertyStatusId: debouncedFilters.propertyStatusId,
          minPrice: debouncedFilters.minPrice,
          maxPrice: debouncedFilters.maxPrice,
          page: nextPage,
          pageSize: propertiesPageSize,
        })

        setProperties(refreshedProperties.items)
        setPagination(refreshedProperties)
        setCityOptions((current) => getUniqueCities(refreshedProperties.items, current))
      }

      setPropertyPendingDelete(null)
      setDeleteMessage({ text: 'Property deleted successfully.', type: 'success' })
    } catch (error) {
      setDeleteMessage({
        text: error instanceof Error ? error.message : 'Failed to delete property.',
        type: 'error',
      })
    } finally {
      setDeletingPropertyId(null)
    }
  }

  return (
    <section className="content-stack">
      <div className="section-heading">
        <div>
          <span className="eyebrow">Properties</span>
          <h1>Browse Properties</h1>
        </div>
        <div className="section-heading-actions">
          <Link className="table-action-link" to={mapSearchPath}>
            View on Map
          </Link>
          <span className={`response-badge response-badge-${loadState}`}>
            {loadState === 'error' ? 'Error' : propertyCountLabel}
          </span>
        </div>
      </div>

      <section className="search-panel">
        <div className="search-panel-copy">
          <h2>Filter Properties</h2>
        </div>
        <label className="search-field filter-field">
          <span>Search</span>
          <input
            value={searchTerm}
            onChange={(event) => updateSearchTerm(event.target.value)}
            placeholder="Search title or description"
            type="search"
          />
        </label>

        <label className="filter-field">
          <span>City</span>
          <select value={filters.city} onChange={(event) => updateFilterField('city', event.target.value)}>
            <option value="">All cities</option>
            {cityOptions.map((city) => (
              <option key={city} value={city}>
                {city}
              </option>
            ))}
          </select>
        </label>

        <label className="filter-field">
          <span>Property Type</span>
          <select
            value={filters.propertyTypeId}
            onChange={(event) => updateFilterField('propertyTypeId', event.target.value)}
          >
            <option value="">All types</option>
            {propertyTypes.map((type) => (
              <option key={type.id} value={type.id}>
                {type.name}
              </option>
            ))}
          </select>
        </label>

        <label className="filter-field">
          <span>Status</span>
          <select
            value={filters.propertyStatusId}
            onChange={(event) => updateFilterField('propertyStatusId', event.target.value)}
          >
            <option value="">All statuses</option>
            {propertyStatuses.map((status) => (
              <option key={status.id} value={status.id}>
                {status.name}
              </option>
            ))}
          </select>
        </label>

        <label className="filter-field">
          <span>Min Price</span>
          <input
            value={filters.minPrice}
            onChange={(event) => updateFilterField('minPrice', event.target.value)}
            min="0"
            step="0.01"
            type="number"
            placeholder="0"
          />
        </label>

        <label className="filter-field">
          <span>Max Price</span>
          <input
            value={filters.maxPrice}
            onChange={(event) => updateFilterField('maxPrice', event.target.value)}
            min="0"
            step="0.01"
            type="number"
            placeholder="500000"
          />
        </label>

        <div className="filter-actions">
          <button type="button" onClick={resetFilters}>
            Clear
          </button>
        </div>
      </section>

      {canCreateProperty ? (
        <section className="form-panel">
          <div className="form-panel-header">
            <div>
              <span className="panel-label">Create</span>
              <h2>Add a new listing</h2>
            </div>
            <Link className="table-action-link" to="/properties/new">
              Create Property
            </Link>
          </div>
        </section>
      ) : null}

      <section className="data-panel" aria-live="polite">
        {deleteMessage && (
          <div className={`table-message table-message-${deleteMessage.type}`}>
            <span>{deleteMessage.text}</span>
          </div>
        )}

        {loadState === 'loading' && <LoadingState message="Loading properties..." />}
        {loadState === 'error' && <ErrorState message={errorMessage} />}
        {loadState === 'success' && properties.length === 0 && (
          <EmptyState message="No properties match the current filters." />
        )}

        {loadState === 'success' && properties.length > 0 && (
          <div className="property-card-grid">
            {properties.map((property) => (
              <PropertyCard
                key={property.id}
                property={property}
                canEdit={canEditProperty}
                canDelete={canDeleteProperty}
                isDeleting={deletingPropertyId === property.id}
                onDelete={(selectedProperty) => {
                  setDeleteMessage(null)
                  setPropertyPendingDelete(selectedProperty)
                }}
              />
            ))}
          </div>
        )}

        {loadState === 'success' && pagination.totalPages > 1 && (
          <div className="pagination-bar" aria-label="Properties pagination">
            <span className="pagination-summary">
              Page {pagination.page} of {pagination.totalPages}
            </span>
            <div className="pagination-controls">
              <button
                type="button"
                onClick={() => changePage(pagination.page - 1)}
                disabled={pagination.page <= 1}
              >
                Previous
              </button>
              {getPageNumbers(pagination.totalPages, pagination.page).map((page) => (
                <button
                  key={page}
                  type="button"
                  className={page === pagination.page ? 'pagination-page-active' : undefined}
                  aria-current={page === pagination.page ? 'page' : undefined}
                  onClick={() => changePage(page)}
                >
                  {page}
                </button>
              ))}
              <button
                type="button"
                onClick={() => changePage(pagination.page + 1)}
                disabled={pagination.page >= pagination.totalPages}
              >
                Next
              </button>
            </div>
          </div>
        )}
      </section>

      {propertyPendingDelete && canDeleteProperty && (
        <div className="dialog-backdrop" role="presentation">
          <div
            className="confirm-dialog"
            role="dialog"
            aria-modal="true"
            aria-labelledby="delete-property-title"
          >
            <div>
              <span className="panel-label">Confirm</span>
              <h2 id="delete-property-title">Delete Property</h2>
            </div>
            <p>
              Are you sure you want to delete <strong>{propertyPendingDelete.title}</strong>? This action cannot be
              undone.
            </p>
            <div className="confirm-dialog-actions">
              <button
                type="button"
                className="dialog-button-secondary"
                onClick={() => setPropertyPendingDelete(null)}
                disabled={deletingPropertyId === propertyPendingDelete.id}
              >
                Cancel
              </button>
              <button
                type="button"
                className="dialog-button-danger"
                onClick={() => void handleDeleteProperty()}
                disabled={deletingPropertyId === propertyPendingDelete.id}
              >
                {deletingPropertyId === propertyPendingDelete.id ? 'Deleting...' : 'Delete'}
              </button>
            </div>
          </div>
        </div>
      )}
    </section>
  )
}

function getUniqueCities(properties: Property[], existingCities: string[] = []) {
  return Array.from(
    new Set(
      [...existingCities, ...properties.map((property) => property.city.trim())].filter(Boolean),
    ),
  ).sort((first, second) => first.localeCompare(second))
}

function getPageNumbers(totalPages: number, currentPage: number) {
  const visiblePages = 5
  const halfWindow = Math.floor(visiblePages / 2)
  const firstPage = Math.max(1, Math.min(currentPage - halfWindow, totalPages - visiblePages + 1))
  const lastPage = Math.min(totalPages, firstPage + visiblePages - 1)

  return Array.from({ length: lastPage - firstPage + 1 }, (_, index) => firstPage + index)
}

function useDebouncedValue<T>(value: T, delayMs: number) {
  const [debouncedValue, setDebouncedValue] = useState(value)

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setDebouncedValue(value)
    }, delayMs)

    return () => window.clearTimeout(timeoutId)
  }, [value, delayMs])

  return debouncedValue
}

function buildPropertyQueryPath(
  pathname: string,
  values: Record<(typeof propertyQueryKeys)[number], string>,
) {
  const parameters = new URLSearchParams()

  propertyQueryKeys.forEach((key) => {
    const value = values[key]?.trim()
    if (value) {
      parameters.set(key, value)
    }
  })

  const queryString = parameters.toString()
  return queryString ? `${pathname}?${queryString}` : pathname
}
