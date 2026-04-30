import { type FormEvent, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  createProperty,
  deleteProperty,
  getAgents,
  getCompanies,
  getProperties,
  getPropertyStatuses,
  getPropertyTypes,
  type Agent,
  type Company,
  type CreatePropertyPayload,
  type PagedResult,
  type Property,
  type PropertyStatus,
  type PropertyType,
} from '../services/api'

type LoadState = 'loading' | 'success' | 'error'
type SubmitState = 'idle' | 'submitting' | 'success' | 'error'

type PropertyFormState = {
  title: string
  description: string
  price: string
  area: string
  bedrooms: string
  bathrooms: string
  floors: string
  yearBuilt: string
  propertyTypeId: string
  propertyStatusId: string
  companyId: string
  agentId: string
  address: string
  city: string
  latitude: string
  longitude: string
}

type FormErrors = Partial<Record<keyof PropertyFormState, string>>

type PropertyFilterState = {
  city: string
  propertyTypeId: string
  propertyStatusId: string
  minPrice: string
  maxPrice: string
}

const initialFormState: PropertyFormState = {
  title: '',
  description: '',
  price: '',
  area: '',
  bedrooms: '',
  bathrooms: '',
  floors: '',
  yearBuilt: '',
  propertyTypeId: '',
  propertyStatusId: '',
  companyId: '',
  agentId: '',
  address: '',
  city: '',
  latitude: '',
  longitude: '',
}

const initialFilterState: PropertyFilterState = {
  city: '',
  propertyTypeId: '',
  propertyStatusId: '',
  minPrice: '',
  maxPrice: '',
}

const propertiesPageSize = 10

const currencyFormatter = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  maximumFractionDigits: 0,
})

export default function PropertiesPage() {
  const [properties, setProperties] = useState<Property[]>([])
  const [propertyTypes, setPropertyTypes] = useState<PropertyType[]>([])
  const [propertyStatuses, setPropertyStatuses] = useState<PropertyStatus[]>([])
  const [cityOptions, setCityOptions] = useState<string[]>([])
  const [companies, setCompanies] = useState<Company[]>([])
  const [agents, setAgents] = useState<Agent[]>([])
  const [form, setForm] = useState<PropertyFormState>(initialFormState)
  const [filters, setFilters] = useState<PropertyFilterState>(initialFilterState)
  const [pagination, setPagination] = useState<PagedResult<Property>>({
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: propertiesPageSize,
    totalPages: 0,
  })
  const [currentPage, setCurrentPage] = useState(1)
  const [formErrors, setFormErrors] = useState<FormErrors>({})
  const [loadState, setLoadState] = useState<LoadState>('loading')
  const [submitState, setSubmitState] = useState<SubmitState>('idle')
  const [deletingPropertyId, setDeletingPropertyId] = useState<number | null>(null)
  const [propertyPendingDelete, setPropertyPendingDelete] = useState<Property | null>(null)
  const [errorMessage, setErrorMessage] = useState('')
  const [submitMessage, setSubmitMessage] = useState('')
  const [deleteMessage, setDeleteMessage] = useState<{ text: string; type: 'success' | 'error' } | null>(null)
  const [searchTerm, setSearchTerm] = useState('')
  const debouncedSearchTerm = useDebouncedValue(searchTerm, 350)
  const debouncedFilters = useDebouncedValue(filters, 350)

  useEffect(() => {
    const controller = new AbortController()

    async function loadLookups() {
      try {
        setErrorMessage('')

        const [typesResult, statusesResult, companiesResult, agentsResult, propertiesResult] =
          await Promise.all([
            getPropertyTypes(controller.signal),
            getPropertyStatuses(controller.signal),
            getCompanies(controller.signal),
            getAgents(controller.signal),
            getProperties(controller.signal, { pageSize: 100 }),
          ])

        setPropertyTypes(typesResult)
        setPropertyStatuses(statusesResult)
        setCompanies(companiesResult)
        setAgents(agentsResult)
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

  function updateFormField(field: keyof PropertyFormState, value: string) {
    setForm((current) => ({
      ...current,
      [field]: value,
      ...(field === 'companyId' ? { agentId: '' } : {}),
    }))
    setFormErrors((current) => {
      const next = { ...current }
      delete next[field]
      if (field === 'companyId') {
        delete next.agentId
      }
      return next
    })
  }

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

  async function handleCompanyChange(value: string) {
    updateFormField('companyId', value)

    if (!value) {
      const allAgents = await getAgents()
      setAgents(allAgents)
      return
    }

    const companyAgents = await getAgents(undefined, Number(value))
    setAgents(companyAgents)
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const validation = validateForm(form)
    setFormErrors(validation)

    if (Object.keys(validation).length > 0) {
      setSubmitState('error')
      setSubmitMessage('Please fix the highlighted fields.')
      return
    }

    try {
      setSubmitState('submitting')
      setSubmitMessage('')

      const createdProperty = await createProperty(buildPayload(form))
      const refreshedProperties = await getProperties(undefined, {
        search: debouncedSearchTerm,
        city: debouncedFilters.city,
        propertyTypeId: debouncedFilters.propertyTypeId,
        propertyStatusId: debouncedFilters.propertyStatusId,
        minPrice: debouncedFilters.minPrice,
        maxPrice: debouncedFilters.maxPrice,
        page: currentPage,
        pageSize: propertiesPageSize,
      })

      setProperties(refreshedProperties.items)
      setPagination(refreshedProperties)
      setCityOptions((current) => getUniqueCities([...refreshedProperties.items, createdProperty], current))
      setForm(initialFormState)
      setFormErrors({})
      setSubmitState('success')
      setSubmitMessage('Property created successfully.')
    } catch (error) {
      setSubmitState('error')
      setSubmitMessage(error instanceof Error ? error.message : 'Failed to create property.')
    }
  }

  async function handleDeleteProperty() {
    if (!propertyPendingDelete) {
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
          <h1>Property Listings</h1>
        </div>
        <span className={`response-badge response-badge-${loadState}`}>
          {loadState === 'error' ? 'Error' : propertyCountLabel}
        </span>
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

      <section className="form-panel">
        <div className="form-panel-header">
          <div>
            <span className="panel-label">Create</span>
            <h2>New Property</h2>
          </div>
          {submitMessage && (
            <span
              className={`form-message ${
                submitState === 'success' ? 'form-message-success' : 'form-message-error'
              }`}
            >
              {submitMessage}
            </span>
          )}
        </div>

        <form className="property-form" onSubmit={handleSubmit} noValidate>
          <label className="field">
            <span>Title</span>
            <input
              value={form.title}
              onChange={(event) => updateFormField('title', event.target.value)}
              maxLength={200}
              placeholder="Modern Apartment"
            />
            {formErrors.title && <small>{formErrors.title}</small>}
          </label>

          <label className="field">
            <span>City</span>
            <input
              value={form.city}
              onChange={(event) => updateFormField('city', event.target.value)}
              maxLength={100}
              placeholder="Tirane"
            />
            {formErrors.city && <small>{formErrors.city}</small>}
          </label>

          <label className="field field-wide">
            <span>Address</span>
            <input
              value={form.address}
              onChange={(event) => updateFormField('address', event.target.value)}
              maxLength={300}
              placeholder="Rruga e Kavajes"
            />
            {formErrors.address && <small>{formErrors.address}</small>}
          </label>

          <label className="field">
            <span>Property Type</span>
            <select
              value={form.propertyTypeId}
              onChange={(event) => updateFormField('propertyTypeId', event.target.value)}
            >
              <option value="">Select type</option>
              {propertyTypes.map((type) => (
                <option key={type.id} value={type.id}>
                  {type.name}
                </option>
              ))}
            </select>
            {formErrors.propertyTypeId && <small>{formErrors.propertyTypeId}</small>}
          </label>

          <label className="field">
            <span>Status</span>
            <select
              value={form.propertyStatusId}
              onChange={(event) => updateFormField('propertyStatusId', event.target.value)}
            >
              <option value="">Select status</option>
              {propertyStatuses.map((status) => (
                <option key={status.id} value={status.id}>
                  {status.name}
                </option>
              ))}
            </select>
            {formErrors.propertyStatusId && <small>{formErrors.propertyStatusId}</small>}
          </label>

          <label className="field">
            <span>Company</span>
            <select
              value={form.companyId}
              onChange={(event) => void handleCompanyChange(event.target.value)}
            >
              <option value="">Select company</option>
              {companies.map((company) => (
                <option key={company.id} value={company.id}>
                  {company.name}
                </option>
              ))}
            </select>
            {formErrors.companyId && <small>{formErrors.companyId}</small>}
          </label>

          <label className="field">
            <span>Agent</span>
            <select
              value={form.agentId}
              onChange={(event) => updateFormField('agentId', event.target.value)}
              disabled={!form.companyId}
            >
              <option value="">{form.companyId ? 'Select agent' : 'Select company first'}</option>
              {agents.map((agent) => (
                <option key={agent.id} value={agent.id}>
                  {agent.firstName} {agent.lastName}
                </option>
              ))}
            </select>
            {formErrors.agentId && <small>{formErrors.agentId}</small>}
          </label>

          <label className="field">
            <span>Price</span>
            <input
              value={form.price}
              onChange={(event) => updateFormField('price', event.target.value)}
              min="0.01"
              step="0.01"
              type="number"
              placeholder="120000"
            />
            {formErrors.price && <small>{formErrors.price}</small>}
          </label>

          <label className="field">
            <span>Area</span>
            <input
              value={form.area}
              onChange={(event) => updateFormField('area', event.target.value)}
              min="0.01"
              step="0.01"
              type="number"
              placeholder="78"
            />
            {formErrors.area && <small>{formErrors.area}</small>}
          </label>

          <label className="field">
            <span>Bedrooms</span>
            <input
              value={form.bedrooms}
              onChange={(event) => updateFormField('bedrooms', event.target.value)}
              min="0"
              max="100"
              step="1"
              type="number"
            />
            {formErrors.bedrooms && <small>{formErrors.bedrooms}</small>}
          </label>

          <label className="field">
            <span>Bathrooms</span>
            <input
              value={form.bathrooms}
              onChange={(event) => updateFormField('bathrooms', event.target.value)}
              min="0"
              max="50"
              step="1"
              type="number"
            />
            {formErrors.bathrooms && <small>{formErrors.bathrooms}</small>}
          </label>

          <label className="field">
            <span>Floors</span>
            <input
              value={form.floors}
              onChange={(event) => updateFormField('floors', event.target.value)}
              min="0"
              max="200"
              step="1"
              type="number"
            />
            {formErrors.floors && <small>{formErrors.floors}</small>}
          </label>

          <label className="field">
            <span>Year Built</span>
            <input
              value={form.yearBuilt}
              onChange={(event) => updateFormField('yearBuilt', event.target.value)}
              min="1800"
              max={new Date().getFullYear()}
              step="1"
              type="number"
            />
            {formErrors.yearBuilt && <small>{formErrors.yearBuilt}</small>}
          </label>

          <label className="field">
            <span>Latitude</span>
            <input
              value={form.latitude}
              onChange={(event) => updateFormField('latitude', event.target.value)}
              min="-90"
              max="90"
              step="0.00000001"
              type="number"
              placeholder="41.3275"
            />
            {formErrors.latitude && <small>{formErrors.latitude}</small>}
          </label>

          <label className="field">
            <span>Longitude</span>
            <input
              value={form.longitude}
              onChange={(event) => updateFormField('longitude', event.target.value)}
              min="-180"
              max="180"
              step="0.00000001"
              type="number"
              placeholder="19.8187"
            />
            {formErrors.longitude && <small>{formErrors.longitude}</small>}
          </label>

          <label className="field field-wide">
            <span>Description</span>
            <textarea
              value={form.description}
              onChange={(event) => updateFormField('description', event.target.value)}
              maxLength={5000}
              rows={3}
              placeholder="Short property description"
            />
          </label>

          <div className="form-actions">
            <button type="submit" disabled={submitState === 'submitting' || loadState !== 'success'}>
              {submitState === 'submitting' ? 'Creating...' : 'Create Property'}
            </button>
          </div>
        </form>
      </section>

      <section className="data-panel" aria-live="polite">
        {deleteMessage && (
          <div className={`table-message table-message-${deleteMessage.type}`}>
            <span>{deleteMessage.text}</span>
          </div>
        )}

        {loadState === 'loading' && (
          <div className="table-state">
            <p>Loading properties...</p>
          </div>
        )}

        {loadState === 'error' && (
          <div className="table-state table-state-error">
            <p>{errorMessage}</p>
          </div>
        )}

        {loadState === 'success' && properties.length === 0 && (
          <div className="table-state">
            <p>No properties found.</p>
          </div>
        )}

        {loadState === 'success' && properties.length > 0 && (
          <div className="properties-table-wrap">
            <table className="properties-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Title</th>
                  <th>Price</th>
                  <th>City</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {properties.map((property) => (
                  <tr key={property.id}>
                    <td data-label="ID">{property.id}</td>
                    <td data-label="Title">{property.title}</td>
                    <td data-label="Price">{currencyFormatter.format(property.price)}</td>
                    <td data-label="City">{property.city}</td>
                    <td data-label="Status">
                      <span className="status-pill">{property.propertyStatus?.name ?? 'Unknown'}</span>
                    </td>
                    <td data-label="Actions">
                      <div className="table-actions">
                        <Link className="table-action-link" to={`/properties/${property.id}`}>
                          Details
                        </Link>
                        <button
                          type="button"
                          className="table-action-danger"
                          onClick={() => {
                            setDeleteMessage(null)
                            setPropertyPendingDelete(property)
                          }}
                          disabled={deletingPropertyId === property.id}
                        >
                          {deletingPropertyId === property.id ? 'Deleting...' : 'Delete'}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
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

      {propertyPendingDelete && (
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

function validateForm(form: PropertyFormState) {
  const errors: FormErrors = {}

  if (!form.title.trim()) {
    errors.title = 'Title is required.'
  }

  if (!form.city.trim()) {
    errors.city = 'City is required.'
  }

  if (!form.address.trim()) {
    errors.address = 'Address is required.'
  }

  validateRequiredPositiveNumber(form.price, 'price', errors)
  validateRequiredPositiveNumber(form.area, 'area', errors)
  validateRequiredId(form.propertyTypeId, 'propertyTypeId', errors)
  validateRequiredId(form.propertyStatusId, 'propertyStatusId', errors)
  validateRequiredId(form.companyId, 'companyId', errors)
  validateRequiredId(form.agentId, 'agentId', errors)
  validateOptionalIntegerRange(form.bedrooms, 'bedrooms', 0, 100, errors)
  validateOptionalIntegerRange(form.bathrooms, 'bathrooms', 0, 50, errors)
  validateOptionalIntegerRange(form.floors, 'floors', 0, 200, errors)
  validateOptionalIntegerRange(form.yearBuilt, 'yearBuilt', 1800, new Date().getFullYear(), errors)
  validateOptionalNumberRange(form.latitude, 'latitude', -90, 90, errors)
  validateOptionalNumberRange(form.longitude, 'longitude', -180, 180, errors)

  return errors
}

function validateRequiredPositiveNumber(
  value: string,
  field: 'price' | 'area',
  errors: FormErrors,
) {
  const parsed = Number(value)

  if (!value || Number.isNaN(parsed) || parsed <= 0) {
    errors[field] = `${field === 'price' ? 'Price' : 'Area'} must be greater than zero.`
  }
}

function validateRequiredId(
  value: string,
  field: 'propertyTypeId' | 'propertyStatusId' | 'companyId' | 'agentId',
  errors: FormErrors,
) {
  if (!value || Number(value) <= 0) {
    errors[field] = 'Please select a value.'
  }
}

function validateOptionalIntegerRange(
  value: string,
  field: 'bedrooms' | 'bathrooms' | 'floors' | 'yearBuilt',
  min: number,
  max: number,
  errors: FormErrors,
) {
  if (!value) {
    return
  }

  const parsed = Number(value)

  if (!Number.isInteger(parsed) || parsed < min || parsed > max) {
    errors[field] = `Value must be between ${min} and ${max}.`
  }
}

function validateOptionalNumberRange(
  value: string,
  field: 'latitude' | 'longitude',
  min: number,
  max: number,
  errors: FormErrors,
) {
  if (!value) {
    return
  }

  const parsed = Number(value)

  if (Number.isNaN(parsed) || parsed < min || parsed > max) {
    errors[field] = `Value must be between ${min} and ${max}.`
  }
}

function buildPayload(form: PropertyFormState): CreatePropertyPayload {
  return {
    title: form.title.trim(),
    description: form.description.trim() || null,
    price: Number(form.price),
    area: Number(form.area),
    bedrooms: toOptionalNumber(form.bedrooms),
    bathrooms: toOptionalNumber(form.bathrooms),
    floors: toOptionalNumber(form.floors),
    yearBuilt: toOptionalNumber(form.yearBuilt),
    propertyTypeId: Number(form.propertyTypeId),
    propertyStatusId: Number(form.propertyStatusId),
    companyId: Number(form.companyId),
    agentId: Number(form.agentId),
    address: form.address.trim(),
    city: form.city.trim(),
    latitude: toOptionalNumber(form.latitude),
    longitude: toOptionalNumber(form.longitude),
  }
}

function toOptionalNumber(value: string) {
  return value ? Number(value) : null
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
