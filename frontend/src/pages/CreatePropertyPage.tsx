import { type FormEvent, useEffect, useMemo, useState } from 'react'
import L from 'leaflet'
import { MapContainer, Marker, TileLayer, useMapEvents } from 'react-leaflet'
import 'leaflet/dist/leaflet.css'
import { Link, useNavigate } from 'react-router-dom'
import {
  createProperty,
  generatePropertyPrice,
  getAgents,
  getCompanies,
  getPropertyStatuses,
  getPropertyTypes,
  type Agent,
  type Company,
  type CreatePropertyPayload,
  type GeneratePropertyPriceResponse,
  type PropertyStatus,
  type PropertyType,
} from '../services/api'
import LoadingSpinner from '../components/LoadingSpinner'

type LoadState = 'loading' | 'success' | 'error'
type SubmitState = 'idle' | 'submitting' | 'success' | 'error'
type PriceState = 'idle' | 'loading' | 'success' | 'error'
type AreaUnit = 'm2' | 'sqft'

type PropertyFormState = {
  title: string
  description: string
  price: string
  livingArea: string
  livingAreaUnit: AreaUnit
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
  zipcode: string
  latitude: string
  longitude: string
  lotArea: string
  lotAreaUnit: AreaUnit
  condition: string
  grade: string
  hasBasement: boolean
  basementArea: string
  basementAreaUnit: AreaUnit
  waterfront: boolean
  viewQuality: string
  renovated: boolean
  yearRenovated: string
  nearbyLivingArea: string
  nearbyLivingAreaUnit: AreaUnit
  nearbyLotArea: string
  nearbyLotAreaUnit: AreaUnit
}

type FormErrors = Partial<Record<keyof PropertyFormState, string>>

const initialFormState: PropertyFormState = {
  title: '',
  description: '',
  price: '',
  livingArea: '',
  livingAreaUnit: 'sqft',
  bedrooms: '',
  bathrooms: '',
  floors: '',
  yearBuilt: '',
  propertyTypeId: '',
  propertyStatusId: '',
  companyId: '',
  agentId: '',
  address: '',
  city: 'Seattle',
  zipcode: '98101',
  latitude: '47.606200',
  longitude: '-122.332100',
  lotArea: '',
  lotAreaUnit: 'sqft',
  condition: '3',
  grade: '7',
  hasBasement: false,
  basementArea: '',
  basementAreaUnit: 'sqft',
  waterfront: false,
  viewQuality: '0',
  renovated: false,
  yearRenovated: '',
  nearbyLivingArea: '',
  nearbyLivingAreaUnit: 'sqft',
  nearbyLotArea: '',
  nearbyLotAreaUnit: 'sqft',
}

const mapPinIcon = L.divIcon({
  className: 'property-pin-icon property-picker-pin-icon',
  html: '<span class="property-pin property-picker-pin" aria-hidden="true"></span>',
  iconSize: [30, 42],
  iconAnchor: [15, 42],
})

const seattleCenter: [number, number] = [47.6062, -122.3321]
const currencyFormatter = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 })

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
  const [priceState, setPriceState] = useState<PriceState>('idle')
  const [generatedPrice, setGeneratedPrice] = useState<GeneratePropertyPriceResponse | null>(null)
  const [priceMessage, setPriceMessage] = useState('')
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
        if (error instanceof DOMException && error.name === 'AbortError') return
        setLoadState('error')
        setErrorMessage(error instanceof Error ? error.message : 'Failed to load property form.')
      }
    }

    void loadCreateData()
    return () => controller.abort()
  }, [])

  const missingPriceFields = useMemo(() => getMissingPriceFields(form), [form])
  const canGeneratePrice = missingPriceFields.length === 0 && priceState !== 'loading'
  const pickerPosition = getPickerPosition(form)

  useEffect(() => {
    const latitude = Number(form.latitude)
    const longitude = Number(form.longitude)

    if (
      !Number.isFinite(latitude) ||
      !Number.isFinite(longitude) ||
      latitude < -90 ||
      latitude > 90 ||
      longitude < -180 ||
      longitude > 180
    ) {
      return
    }

    const timeoutId = window.setTimeout(() => {
      void fillAddressFromCoordinates(latitude, longitude)
    }, 650)

    return () => window.clearTimeout(timeoutId)
  }, [form.latitude, form.longitude])

  function updateFormField(field: keyof PropertyFormState, value: string | boolean) {
    setForm((current) => ({
      ...current,
      [field]: value,
      ...(field === 'companyId' ? { agentId: '' } : {}),
      ...(field === 'hasBasement' && value === false ? { basementArea: '' } : {}),
      ...(field === 'renovated' && value === false ? { yearRenovated: '' } : {}),
    }))
    setFormErrors((current) => {
      const next = { ...current }
      delete next[field]
      if (field === 'companyId') delete next.agentId
      return next
    })
    if (field !== 'price') {
      setGeneratedPrice(null)
      setPriceState('idle')
      setPriceMessage('')
    }
  }

  async function handleCompanyChange(value: string) {
    updateFormField('companyId', value)
    setAgents(value ? await getAgents(undefined, Number(value)) : await getAgents())
  }

  async function handleLocationPicked(latitude: number, longitude: number) {
    setForm((current) => ({
      ...current,
      latitude: latitude.toFixed(6),
      longitude: longitude.toFixed(6),
    }))
    setFormErrors((current) => {
      const next = { ...current }
      delete next.latitude
      delete next.longitude
      return next
    })

    await fillAddressFromCoordinates(latitude, longitude)
  }

  async function fillAddressFromCoordinates(latitude: number, longitude: number) {
    try {
      const result = await reverseGeocode(latitude, longitude)
      setForm((current) => {
        if (current.latitude !== latitude.toFixed(6) && Number(current.latitude) !== latitude) {
          return current
        }

        if (current.longitude !== longitude.toFixed(6) && Number(current.longitude) !== longitude) {
          return current
        }

        return {
          ...current,
          address: result.address || current.address,
          city: result.city || current.city,
          zipcode: result.zipcode || current.zipcode,
        }
      })
    } catch {
      setPriceMessage('Location lookup was not available, so you can fill address details manually.')
    }
  }

  async function handleGeneratePrice() {
    const validation = validatePriceFields(form)
    setFormErrors((current) => ({ ...current, ...validation }))

    if (Object.keys(validation).length > 0) {
      setPriceState('error')
      setPriceMessage('Add the missing property details before generating a price.')
      return
    }

    try {
      setPriceState('loading')
      setPriceMessage('')
      const result = await generatePropertyPrice(buildPricePayload(form))
      setGeneratedPrice(result)
      setPriceState('success')
    } catch (error) {
      setPriceState('error')
      setPriceMessage(error instanceof Error ? error.message : 'Failed to generate price.')
    }
  }

  function applyGeneratedPrice() {
    if (!generatedPrice) return
    updateFormField('price', generatedPrice.suggestedPrice.toFixed(2))
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
        <Link className="top-nav-link" to="/properties">Back to properties</Link>
      </div>

      <section className="form-panel">
        <div className="form-panel-header">
          <div>
            <span className="panel-label">Create</span>
            <h2>New Property</h2>
          </div>
          {submitMessage && (
            <span className={`form-message ${submitState === 'success' ? 'form-message-success' : 'form-message-error'}`}>
              {submitMessage}
            </span>
          )}
        </div>

        <form className="property-form property-create-form" onSubmit={handleSubmit} noValidate>
          <fieldset className="form-section">
            <legend>Listing Info</legend>
            <label className="field">
              <span>Title</span>
              <input value={form.title} onChange={(event) => updateFormField('title', event.target.value)} maxLength={200} placeholder="Modern family home" />
              {formErrors.title && <small>{formErrors.title}</small>}
            </label>
            <label className="field">
              <span>Property Type</span>
              <select value={form.propertyTypeId} onChange={(event) => updateFormField('propertyTypeId', event.target.value)}>
                <option value="">Select type</option>
                {propertyTypes.map((type) => <option key={type.id} value={type.id}>{type.name}</option>)}
              </select>
              {formErrors.propertyTypeId && <small>{formErrors.propertyTypeId}</small>}
            </label>
            <label className="field">
              <span>Status</span>
              <select value={form.propertyStatusId} onChange={(event) => updateFormField('propertyStatusId', event.target.value)}>
                <option value="">Select status</option>
                {propertyStatuses.map((status) => <option key={status.id} value={status.id}>{status.name}</option>)}
              </select>
              {formErrors.propertyStatusId && <small>{formErrors.propertyStatusId}</small>}
            </label>
            <label className="field">
              <span>Company</span>
              <select value={form.companyId} onChange={(event) => void handleCompanyChange(event.target.value)}>
                <option value="">Select company</option>
                {companies.map((company) => <option key={company.id} value={company.id}>{company.name}</option>)}
              </select>
              {formErrors.companyId && <small>{formErrors.companyId}</small>}
            </label>
            <label className="field">
              <span>Agent</span>
              <select value={form.agentId} onChange={(event) => updateFormField('agentId', event.target.value)} disabled={!form.companyId}>
                <option value="">{form.companyId ? 'Select agent' : 'Select company first'}</option>
                {agents.map((agent) => <option key={agent.id} value={agent.id}>{agent.firstName} {agent.lastName}</option>)}
              </select>
              {formErrors.agentId && <small>{formErrors.agentId}</small>}
            </label>
            <label className="field field-wide">
              <span>Description</span>
              <textarea value={form.description} onChange={(event) => updateFormField('description', event.target.value)} maxLength={5000} rows={4} placeholder="Short property description" />
            </label>
            <div className="image-upload-placeholder field-wide">
              <strong>Images</strong>
              <span>Upload photos after the property is created.</span>
            </div>
          </fieldset>

          <fieldset className="form-section">
            <legend>Location</legend>
            <div className="location-picker field-wide">
              <MapContainer center={pickerPosition ?? seattleCenter} zoom={pickerPosition ? 14 : 11} scrollWheelZoom className="create-property-map">
                <TileLayer attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors' url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
                <LocationPickerMarker position={pickerPosition} onPick={handleLocationPicked} />
              </MapContainer>
            </div>
            <label className="field">
              <span>City</span>
              <input value={form.city} onChange={(event) => updateFormField('city', event.target.value)} maxLength={100} placeholder="Seattle" />
              {formErrors.city && <small>{formErrors.city}</small>}
            </label>
            <label className="field">
              <span>Zipcode</span>
              <input value={form.zipcode} onChange={(event) => updateFormField('zipcode', event.target.value)} inputMode="numeric" placeholder="98101" />
              {formErrors.zipcode && <small>{formErrors.zipcode}</small>}
            </label>
            <label className="field field-wide">
              <span>Address</span>
              <input value={form.address} onChange={(event) => updateFormField('address', event.target.value)} maxLength={300} placeholder="Street address" />
              {formErrors.address && <small>{formErrors.address}</small>}
            </label>
            <label className="field">
              <span>Latitude</span>
              <input value={form.latitude} onChange={(event) => updateFormField('latitude', event.target.value)} min="-90" max="90" step="0.000001" type="number" placeholder="47.6062" />
              {formErrors.latitude && <small>{formErrors.latitude}</small>}
            </label>
            <label className="field">
              <span>Longitude</span>
              <input value={form.longitude} onChange={(event) => updateFormField('longitude', event.target.value)} min="-180" max="180" step="0.000001" type="number" placeholder="-122.3321" />
              {formErrors.longitude && <small>{formErrors.longitude}</small>}
            </label>
          </fieldset>

          <fieldset className="form-section">
            <legend>Property Details</legend>
            <div className="field-group field-group-area">
              <label className="field">
                <span>Living Area</span>
                <input value={form.livingArea} onChange={(event) => updateFormField('livingArea', event.target.value)} min="0.01" step="0.01" type="number" placeholder="1800" />
                {formErrors.livingArea && <small>{formErrors.livingArea}</small>}
              </label>
              <label className="field">
                <span>Unit</span>
                <select value={form.livingAreaUnit} onChange={(event) => updateFormField('livingAreaUnit', event.target.value as AreaUnit)}>
                  <option value="sqft">sqft</option>
                  <option value="m2">m2</option>
                </select>
              </label>
            </div>
            <label className="field">
              <span>Bedrooms</span>
              <input value={form.bedrooms} onChange={(event) => updateFormField('bedrooms', event.target.value)} min="1" max="10" step="1" type="number" />
              {formErrors.bedrooms && <small>{formErrors.bedrooms}</small>}
            </label>
            <label className="field">
              <span>Bathrooms</span>
              <input value={form.bathrooms} onChange={(event) => updateFormField('bathrooms', event.target.value)} min="0.5" max="8" step="0.5" type="number" />
              {formErrors.bathrooms && <small>{formErrors.bathrooms}</small>}
            </label>
            <label className="field">
              <span>Floors</span>
              <select value={form.floors} onChange={(event) => updateFormField('floors', event.target.value)}>
                <option value="">Select floors</option>
                {['1', '1.5', '2', '2.5', '3', '3.5'].map((floor) => <option key={floor} value={floor}>{floor}</option>)}
              </select>
              {formErrors.floors && <small>{formErrors.floors}</small>}
            </label>
            <label className="field">
              <span>Year Built</span>
              <input value={form.yearBuilt} onChange={(event) => updateFormField('yearBuilt', event.target.value)} min="1800" max={new Date().getFullYear() + 2} step="1" type="number" />
              {formErrors.yearBuilt && <small>{formErrors.yearBuilt}</small>}
            </label>
          </fieldset>

          <details className="form-section optional-details">
            <summary>
              <span>Optional Accuracy Details</span>
              <small>Optional details. Filling these can make the prediction more accurate.</small>
            </summary>
            <div className="optional-details-grid">
              <div className="field-group field-group-area">
                <label className="field">
                  <span>Lot Area</span>
                  <input value={form.lotArea} onChange={(event) => updateFormField('lotArea', event.target.value)} min="0.01" step="0.01" type="number" />
                </label>
                <label className="field">
                  <span>Unit</span>
                  <select value={form.lotAreaUnit} onChange={(event) => updateFormField('lotAreaUnit', event.target.value as AreaUnit)}>
                    <option value="sqft">sqft</option>
                    <option value="m2">m2</option>
                  </select>
                </label>
              </div>
              <label className="field">
                <span>Condition</span>
                <select value={form.condition} onChange={(event) => updateFormField('condition', event.target.value)}>
                  <option value="1">Poor</option>
                  <option value="2">Fair</option>
                  <option value="3">Average</option>
                  <option value="4">Good</option>
                  <option value="5">Excellent</option>
                </select>
              </label>
              <label className="field">
                <span>Quality Grade</span>
                <select value={form.grade} onChange={(event) => updateFormField('grade', event.target.value)}>
                  {Array.from({ length: 13 }, (_, index) => index + 1).map((grade) => <option key={grade} value={grade}>{grade}</option>)}
                </select>
              </label>
              <div className="field-group field-group-three">
                <label className="toggle-field">
                  <input type="checkbox" checked={form.hasBasement} onChange={(event) => updateFormField('hasBasement', event.target.checked)} />
                  <span>Has Basement</span>
                </label>
                <label className="field">
                  <span>Basement Area</span>
                  <input value={form.basementArea} onChange={(event) => updateFormField('basementArea', event.target.value)} min="0.01" step="0.01" type="number" disabled={!form.hasBasement} />
                  {formErrors.basementArea && <small>{formErrors.basementArea}</small>}
                </label>
                <label className="field">
                  <span>Unit</span>
                  <select value={form.basementAreaUnit} onChange={(event) => updateFormField('basementAreaUnit', event.target.value as AreaUnit)} disabled={!form.hasBasement}>
                    <option value="sqft">sqft</option>
                    <option value="m2">m2</option>
                  </select>
                </label>
              </div>
              <div className="field-group field-group-area">
                <label className="toggle-field">
                  <input type="checkbox" checked={form.waterfront} onChange={(event) => updateFormField('waterfront', event.target.checked)} />
                  <span>Waterfront</span>
                </label>
                <label className="field">
                  <span>View Quality</span>
                  <select value={form.viewQuality} onChange={(event) => updateFormField('viewQuality', event.target.value)}>
                    <option value="0">None</option>
                    <option value="1">Fair</option>
                    <option value="2">Average</option>
                    <option value="3">Good</option>
                    <option value="4">Excellent</option>
                  </select>
                </label>
              </div>
              <div className="field-group field-group-area">
                <label className="toggle-field">
                  <input type="checkbox" checked={form.renovated} onChange={(event) => updateFormField('renovated', event.target.checked)} />
                  <span>Renovated</span>
                </label>
                <label className="field">
                  <span>Year Renovated</span>
                  <input value={form.yearRenovated} onChange={(event) => updateFormField('yearRenovated', event.target.value)} min="1800" max={new Date().getFullYear()} step="1" type="number" disabled={!form.renovated} />
                  {formErrors.yearRenovated && <small>{formErrors.yearRenovated}</small>}
                </label>
              </div>
              <div className="field-group field-group-area">
                <label className="field">
                  <span>Nearby Living Area</span>
                  <input value={form.nearbyLivingArea} onChange={(event) => updateFormField('nearbyLivingArea', event.target.value)} min="0.01" step="0.01" type="number" />
                </label>
                <label className="field">
                  <span>Unit</span>
                  <select value={form.nearbyLivingAreaUnit} onChange={(event) => updateFormField('nearbyLivingAreaUnit', event.target.value as AreaUnit)}>
                    <option value="sqft">sqft</option>
                    <option value="m2">m2</option>
                  </select>
                </label>
              </div>
              <div className="field-group field-group-area">
                <label className="field">
                  <span>Nearby Lot Area</span>
                  <input value={form.nearbyLotArea} onChange={(event) => updateFormField('nearbyLotArea', event.target.value)} min="0.01" step="0.01" type="number" />
                </label>
                <label className="field">
                  <span>Unit</span>
                  <select value={form.nearbyLotAreaUnit} onChange={(event) => updateFormField('nearbyLotAreaUnit', event.target.value as AreaUnit)}>
                    <option value="sqft">sqft</option>
                    <option value="m2">m2</option>
                  </select>
                </label>
              </div>
            </div>
          </details>

          <section className={`generate-price-card generate-price-card-${priceState}`}>
            <div className="generate-price-header">
              <div className="generate-price-icon" aria-hidden="true">$</div>
              <div>
                <h3>Generate Price</h3>
                <p>Use property details and location to estimate a suggested market price.</p>
              </div>
              <span className="generate-price-status">
                {priceState === 'success' ? 'Generated price' : missingPriceFields.length > 0 ? 'Missing details' : 'Ready to generate'}
              </span>
            </div>
            <div className="generated-price-display">
              {priceState === 'loading' ? (
                <span className="state-with-spinner"><LoadingSpinner label="Generating price" /> Generating...</span>
              ) : generatedPrice ? (
                <strong>{generatedPrice.formattedPrice || currencyFormatter.format(generatedPrice.suggestedPrice)}</strong>
              ) : (
                <strong>Price estimate</strong>
              )}
              {missingPriceFields.length > 0 && <small>Needed: {missingPriceFields.join(', ')}</small>}
              {priceMessage && <small className={priceState === 'error' ? 'generate-price-error' : ''}>{priceMessage}</small>}
              {generatedPrice?.mlWarnings.map((warning) => <small key={warning} className="generate-price-warning">{warning}</small>)}
            </div>
            <div className="generate-price-actions">
              <button type="button" onClick={() => void handleGeneratePrice()} disabled={!canGeneratePrice}>
                {priceState === 'loading' ? 'Generating...' : 'Generate Price'}
              </button>
              <button type="button" className="secondary-button" onClick={applyGeneratedPrice} disabled={!generatedPrice}>
                Apply to Price
              </button>
            </div>
            <p className="generate-price-note">This is an estimate. Agents can adjust the final listing price.</p>
          </section>

          <fieldset className="form-section final-price-section">
            <legend>Final Price</legend>
            <label className="field">
              <span>Price</span>
              <input value={form.price} onChange={(event) => updateFormField('price', event.target.value)} min="0.01" step="0.01" type="number" placeholder="549000" />
              {formErrors.price && <small>{formErrors.price}</small>}
            </label>
          </fieldset>

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

function LocationPickerMarker({
  position,
  onPick,
}: {
  position: [number, number] | null
  onPick: (latitude: number, longitude: number) => void
}) {
  useMapEvents({
    click(event) {
      onPick(event.latlng.lat, event.latlng.lng)
    },
  })

  return position ? <Marker icon={mapPinIcon} position={position} /> : null
}

function validateForm(form: PropertyFormState) {
  const errors: FormErrors = {}

  if (!form.title.trim()) errors.title = 'Title is required.'
  if (!form.city.trim()) errors.city = 'City is required.'
  if (!form.address.trim()) errors.address = 'Address is required.'
  validateRequiredPositiveNumber(form.price, 'price', errors)
  validateRequiredPositiveNumber(form.livingArea, 'livingArea', errors)
  validateRequiredId(form.propertyTypeId, 'propertyTypeId', errors)
  validateRequiredId(form.propertyStatusId, 'propertyStatusId', errors)
  validateRequiredId(form.companyId, 'companyId', errors)
  validateRequiredId(form.agentId, 'agentId', errors)
  validatePriceFields(form, errors)

  return errors
}

function validatePriceFields(form: PropertyFormState, existingErrors: FormErrors = {}) {
  const errors = existingErrors
  const currentYear = new Date().getFullYear()

  validateRequiredPositiveNumber(form.livingArea, 'livingArea', errors)
  validateIntegerRange(form.bedrooms, 'bedrooms', 1, 10, errors)
  validateNumberRange(form.bathrooms, 'bathrooms', 0.5, 8, errors)
  validateNumberRange(form.floors, 'floors', 1, 3.5, errors)
  validateIntegerRange(form.yearBuilt, 'yearBuilt', 1800, currentYear + 2, errors)
  validateNumberRange(form.latitude, 'latitude', -90, 90, errors)
  validateNumberRange(form.longitude, 'longitude', -180, 180, errors)
  validateIntegerRange(form.zipcode, 'zipcode', 1, 99999, errors)

  if (form.hasBasement && form.basementArea) {
    validateRequiredPositiveNumber(form.basementArea, 'basementArea', errors)
    if (!errors.basementArea && convertToSqft(Number(form.basementArea), form.basementAreaUnit) > convertToSqft(Number(form.livingArea), form.livingAreaUnit)) {
      errors.basementArea = 'Basement Area cannot be greater than Living Area.'
    }
  }

  if (form.renovated) {
    validateIntegerRange(form.yearRenovated, 'yearRenovated', Number(form.yearBuilt || 1800), currentYear, errors)
  }

  return errors
}

function validateRequiredPositiveNumber(value: string, field: 'price' | 'livingArea' | 'basementArea', errors: FormErrors) {
  const parsed = Number(value)
  if (!value || Number.isNaN(parsed) || parsed <= 0) {
    errors[field] = `${field === 'price' ? 'Price' : field === 'livingArea' ? 'Living Area' : 'Basement Area'} must be greater than zero.`
  }
}

function validateRequiredId(value: string, field: 'propertyTypeId' | 'propertyStatusId' | 'companyId' | 'agentId', errors: FormErrors) {
  if (!value || Number(value) <= 0) errors[field] = 'Please select a value.'
}

function validateIntegerRange(value: string, field: keyof PropertyFormState, min: number, max: number, errors: FormErrors) {
  const parsed = Number(value)
  if (!value || !Number.isInteger(parsed) || parsed < min || parsed > max) {
    errors[field] = `Value must be between ${min} and ${max}.`
  }
}

function validateNumberRange(value: string, field: keyof PropertyFormState, min: number, max: number, errors: FormErrors) {
  const parsed = Number(value)
  if (!value || Number.isNaN(parsed) || parsed < min || parsed > max) {
    errors[field] = `Value must be between ${min} and ${max}.`
  }
}

function buildPayload(form: PropertyFormState): CreatePropertyPayload {
  return {
    title: form.title.trim(),
    description: form.description.trim() || null,
    price: Number(form.price),
    area: convertToSquareMeters(Number(form.livingArea), form.livingAreaUnit),
    bedrooms: toOptionalNumber(form.bedrooms),
    bathrooms: toOptionalRoundedNumber(form.bathrooms),
    floors: toOptionalRoundedNumber(form.floors),
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

function buildPricePayload(form: PropertyFormState) {
  return {
    livingArea: Number(form.livingArea),
    livingAreaUnit: form.livingAreaUnit,
    bedrooms: Number(form.bedrooms),
    bathrooms: Number(form.bathrooms),
    floors: Number(form.floors),
    yearBuilt: Number(form.yearBuilt),
    latitude: Number(form.latitude),
    longitude: Number(form.longitude),
    zipcode: Number(form.zipcode),
    lotArea: toOptionalNumber(form.lotArea),
    lotAreaUnit: form.lotArea ? form.lotAreaUnit : null,
    condition: toOptionalNumber(form.condition),
    grade: toOptionalNumber(form.grade),
    hasBasement: form.hasBasement,
    basementArea: form.hasBasement ? toOptionalNumber(form.basementArea) : null,
    basementAreaUnit: form.hasBasement && form.basementArea ? form.basementAreaUnit : null,
    waterfront: form.waterfront,
    viewQuality: toOptionalNumber(form.viewQuality),
    renovated: form.renovated,
    yearRenovated: form.renovated ? toOptionalNumber(form.yearRenovated) : null,
    nearbyLivingArea: toOptionalNumber(form.nearbyLivingArea),
    nearbyLivingAreaUnit: form.nearbyLivingArea ? form.nearbyLivingAreaUnit : null,
    nearbyLotArea: toOptionalNumber(form.nearbyLotArea),
    nearbyLotAreaUnit: form.nearbyLotArea ? form.nearbyLotAreaUnit : null,
  }
}

function getMissingPriceFields(form: PropertyFormState) {
  return [
    ['Living Area', form.livingArea],
    ['Bedrooms', form.bedrooms],
    ['Bathrooms', form.bathrooms],
    ['Floors', form.floors],
    ['Year Built', form.yearBuilt],
    ['Latitude', form.latitude],
    ['Longitude', form.longitude],
    ['Zipcode', form.zipcode],
  ]
    .filter(([, value]) => !value)
    .map(([label]) => label)
}

function getPickerPosition(form: PropertyFormState): [number, number] | null {
  const latitude = Number(form.latitude)
  const longitude = Number(form.longitude)
  return Number.isFinite(latitude) && Number.isFinite(longitude) ? [latitude, longitude] : null
}

function toOptionalNumber(value: string) {
  return value ? Number(value) : null
}

function toOptionalRoundedNumber(value: string) {
  return value ? Math.round(Number(value)) : null
}

function convertToSquareMeters(value: number, unit: AreaUnit) {
  return unit === 'sqft' ? Math.round((value / 10.7639) * 100) / 100 : value
}

function convertToSqft(value: number, unit: AreaUnit) {
  return unit === 'm2' ? Math.round(value * 10.7639) : Math.round(value)
}

async function reverseGeocode(latitude: number, longitude: number) {
  const response = await fetch(`https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${latitude}&lon=${longitude}`)
  if (!response.ok) throw new Error('Reverse geocoding failed.')
  const payload = await response.json() as {
    display_name?: string
    address?: {
      house_number?: string
      road?: string
      city?: string
      town?: string
      village?: string
      postcode?: string
    }
  }
  const address = payload.address
  const street = [address?.house_number, address?.road].filter(Boolean).join(' ')

  return {
    address: street || payload.display_name || '',
    city: address?.city || address?.town || address?.village || '',
    zipcode: address?.postcode || '',
  }
}
