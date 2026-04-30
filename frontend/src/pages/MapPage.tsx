import { useEffect, useMemo, useState } from 'react'
import L from 'leaflet'
import { MapContainer, Marker, Popup, TileLayer } from 'react-leaflet'
import 'leaflet/dist/leaflet.css'
import {
  getProperties,
  getPropertyStatuses,
  getPropertyTypes,
  type Property,
  type PropertyStatus,
  type PropertyType,
} from '../services/api'

const propertyPinIcon = L.divIcon({
  className: 'property-pin-icon',
  html: '<span class="property-pin" aria-hidden="true"></span>',
  iconSize: [30, 42],
  iconAnchor: [15, 42],
  popupAnchor: [0, -38],
})

const tiranaCenter: [number, number] = [41.3275, 19.8187]
const mapPageSize = 100

type MapFilterState = {
  city: string
  propertyTypeId: string
  propertyStatusId: string
}

const initialMapFilters: MapFilterState = {
  city: '',
  propertyTypeId: '',
  propertyStatusId: '',
}

const currencyFormatter = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  maximumFractionDigits: 0,
})

export default function MapPage() {
  const [properties, setProperties] = useState<Property[]>([])
  const [propertyTypes, setPropertyTypes] = useState<PropertyType[]>([])
  const [propertyStatuses, setPropertyStatuses] = useState<PropertyStatus[]>([])
  const [cityOptions, setCityOptions] = useState<string[]>([])
  const [filters, setFilters] = useState<MapFilterState>(initialMapFilters)
  const [loadState, setLoadState] = useState<'loading' | 'success' | 'error'>('loading')
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    const controller = new AbortController()

    async function loadLookups() {
      try {
        const [typesResult, statusesResult, propertiesResult] = await Promise.all([
          getPropertyTypes(controller.signal),
          getPropertyStatuses(controller.signal),
          getProperties(controller.signal, { pageSize: mapPageSize }),
        ])

        setPropertyTypes(typesResult)
        setPropertyStatuses(statusesResult)
        setCityOptions(getUniqueCities(propertiesResult.items))
      } catch (error) {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return
        }

        setErrorMessage(error instanceof Error ? error.message : 'Failed to load map lookups.')
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
          city: filters.city,
          propertyTypeId: filters.propertyTypeId,
          propertyStatusId: filters.propertyStatusId,
          page: 1,
          pageSize: mapPageSize,
        })

        setProperties(propertiesResult.items)
        setCityOptions((current) => getUniqueCities(propertiesResult.items, current))
        setLoadState('success')
      } catch (error) {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return
        }

        setErrorMessage(error instanceof Error ? error.message : 'Failed to load property markers.')
        setLoadState('error')
      }
    }

    void loadProperties()

    return () => controller.abort()
  }, [filters])

  const markerProperties = useMemo(
    () =>
      properties.filter(
        (property) =>
          typeof property.latitude === 'number' &&
          typeof property.longitude === 'number' &&
          property.latitude >= -90 &&
          property.latitude <= 90 &&
          property.longitude >= -180 &&
          property.longitude <= 180,
      ),
    [properties],
  )

  function updateFilter(field: keyof MapFilterState, value: string) {
    setFilters((current) => ({
      ...current,
      [field]: value,
    }))
  }

  function resetFilters() {
    setFilters(initialMapFilters)
  }

  return (
    <section className="content-stack">
      <div className="section-heading">
        <div>
          <span className="eyebrow">Map</span>
          <h1>Property Map</h1>
        </div>
        <span className={`response-badge response-badge-${loadState}`}>
          {loadState === 'error' ? 'Error' : `${markerProperties.length} markers`}
        </span>
      </div>

      <section className="map-filter-panel">
        <label className="filter-field">
          <span>City</span>
          <select value={filters.city} onChange={(event) => updateFilter('city', event.target.value)}>
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
            onChange={(event) => updateFilter('propertyTypeId', event.target.value)}
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
            onChange={(event) => updateFilter('propertyStatusId', event.target.value)}
          >
            <option value="">All statuses</option>
            {propertyStatuses.map((status) => (
              <option key={status.id} value={status.id}>
                {status.name}
              </option>
            ))}
          </select>
        </label>

        <div className="filter-actions">
          <button type="button" onClick={resetFilters}>
            Clear
          </button>
        </div>
      </section>

      {loadState === 'error' && (
        <div className="table-state table-state-error">
          <p>{errorMessage}</p>
        </div>
      )}

      <section className="map-panel">
        <MapContainer
          center={tiranaCenter}
          zoom={12}
          scrollWheelZoom
          className="property-map"
          aria-label="Property locations map"
        >
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          {markerProperties.map((property) => (
            <Marker
              key={property.id}
              icon={propertyPinIcon}
              position={[property.latitude as number, property.longitude as number]}
            >
              <Popup>
                <div className="map-popup">
                  <strong>{property.title}</strong>
                  <span>{currencyFormatter.format(property.price)}</span>
                  <span>{property.city}</span>
                </div>
              </Popup>
            </Marker>
          ))}
        </MapContainer>
      </section>
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
