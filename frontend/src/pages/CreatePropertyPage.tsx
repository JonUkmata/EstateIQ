import { type FormEvent, useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import {
  createProperty,
  getAgents,
  getCompanies,
  getPropertyStatuses,
  getPropertyTypes,
  type Agent,
  type Company,
  type CreatePropertyPayload,
  type PropertyStatus,
  type PropertyType,
} from '../services/api'
import LoadingSpinner from '../components/LoadingSpinner'

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

export default function CreatePropertyPage() {
  const navigate = useNavigate()
  const [form, setForm] = useState<PropertyFormState>(initialFormState)
  const [formErrors, setFormErrors] = useState<FormErrors>({})
  const [propertyTypes, setPropertyTypes] = useState<PropertyType[]>([])
  const [propertyStatuses, setPropertyStatuses] = useState<PropertyStatus[]>([])
  const [companies, setCompanies] = useState<Company[]>([])
  const [agents, setAgents] = useState<Agent[]>([])
  const [loadState, setLoadState] = useState<LoadState>('loading')
  const [submitState, setSubmitState] = useState<SubmitState>('idle')
  const [errorMessage, setErrorMessage] = useState('')
  const [submitMessage, setSubmitMessage] = useState('')

  useEffect(() => { document.title = 'New Property | EstateIQ' }, [])

  useEffect(() => {
    const controller = new AbortController()

    async function loadCreateData() {
      try {
        setLoadState('loading')
        setErrorMessage('')

        const [typesResult, statusesResult, companiesResult, agentsResult] = await Promise.all([
          getPropertyTypes(controller.signal),
          getPropertyStatuses(controller.signal),
          getCompanies(controller.signal),
          getAgents(controller.signal),
        ])

        setPropertyTypes(typesResult)
        setPropertyStatuses(statusesResult)
        setCompanies(companiesResult)
        setAgents(agentsResult)
        setLoadState('success')
      } catch (error) {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return
        }

        setLoadState('error')
        setErrorMessage(error instanceof Error ? error.message : 'Failed to load property form.')
      }
    }

    void loadCreateData()
    return () => controller.abort()
  }, [])

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
      setSubmitState('success')
      setSubmitMessage('Property created successfully.')
      navigate(`/properties/${createdProperty.id}`)
    } catch (error) {
      setSubmitState('error')
      setSubmitMessage(error instanceof Error ? error.message : 'Failed to create property.')
    }
  }

  if (loadState === 'loading') {
    return (
      <section className="content-stack">
        <div className="section-heading">
          <h1>Create Property</h1>
          <span className="response-badge response-badge-loading state-with-spinner">
            <LoadingSpinner label="Loading property form" />
            <span>Loading</span>
          </span>
        </div>
      </section>
    )
  }

  if (loadState === 'error') {
    return (
      <section className="content-stack">
        <div className="section-heading">
          <h1>Create Property</h1>
          <span className="response-badge response-badge-error">Error</span>
        </div>
        <section className="data-panel">
          <div className="table-state table-state-error">
            <p>{errorMessage}</p>
          </div>
        </section>
      </section>
    )
  }

  return (
    <section className="content-stack">
      <div className="section-heading">
        <div>
          <span className="eyebrow">Properties</span>
          <h1>Create Property</h1>
        </div>
        <Link className="top-nav-link" to="/properties">
          Back to properties
        </Link>
      </div>

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
            <select value={form.companyId} onChange={(event) => void handleCompanyChange(event.target.value)}>
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
            <button type="submit" disabled={submitState === 'submitting'}>
              {submitState === 'submitting' ? 'Creating...' : 'Create Property'}
            </button>
          </div>
        </form>
      </section>
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
