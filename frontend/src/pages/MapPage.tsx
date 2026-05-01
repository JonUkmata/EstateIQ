import { type MutableRefObject, useEffect, useMemo, useRef, useState } from 'react'
import L from 'leaflet'
import { MapContainer, Marker, Popup, TileLayer, useMap } from 'react-leaflet'
import 'leaflet/dist/leaflet.css'
import {
  getProperties,
  getPropertyStatuses,
  getPropertyTypes,
  type Property,
  type PropertyStatus,
  type PropertyType,
} from '../services/api'
import LoadingSpinner from '../components/LoadingSpinner'

const propertyPinIcon = L.divIcon({
  className: 'property-pin-icon',
  html: '<span class="property-pin" aria-hidden="true"></span>',
  iconSize: [30, 42],
  iconAnchor: [15, 42],
  popupAnchor: [0, -38],
})

const selectedPropertyPinIcon = L.divIcon({
  className: 'property-pin-icon property-pin-icon-selected',
  html: '<span class="property-pin property-pin-selected" aria-hidden="true"></span>',
  iconSize: [38, 52],
  iconAnchor: [19, 52],
  popupAnchor: [0, -48],
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
  const [selectedPropertyId, setSelectedPropertyId] = useState<number | null>(null)
  const markerRefs = useRef<Record<number, L.Marker | null>>({})

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

  const selectedProperty = useMemo(
    () => markerProperties.find((property) => property.id === selectedPropertyId) ?? null,
    [markerProperties, selectedPropertyId],
  )

  useEffect(() => {
    if (selectedPropertyId && !selectedProperty) {
      setSelectedPropertyId(null)
    }
  }, [selectedProperty, selectedPropertyId])

  function updateFilter(field: keyof MapFilterState, value: string) {
    setFilters((current) => ({
      ...current,
      [field]: value,
    }))
  }

  function resetFilters() {
    setFilters(initialMapFilters)
  }

  function selectProperty(property: Property) {
    if (typeof property.latitude !== 'number' || typeof property.longitude !== 'number') {
      return
    }

    setSelectedPropertyId(property.id)
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

      <section className="map-sync-layout">
        <aside className="map-property-list" aria-label="Map property list">
          {loadState === 'loading' && (
            <p className="map-list-state state-with-spinner">
              <LoadingSpinner label="Loading map properties" />
              <span>Loading properties...</span>
            </p>
          )}

          {loadState === 'success' && markerProperties.length === 0 && (
            <p className="map-list-state">No properties with valid map coordinates match these filters.</p>
          )}

          {loadState === 'success' &&
            markerProperties.map((property) => (
              <button
                key={property.id}
                type="button"
                className={`map-property-item ${
                  selectedPropertyId === property.id ? 'map-property-item-active' : ''
                }`}
                onClick={() => selectProperty(property)}
                aria-pressed={selectedPropertyId === property.id}
              >
                <span>{property.title}</span>
                <strong>{currencyFormatter.format(property.price)}</strong>
                <small>{property.city}</small>
              </button>
            ))}
        </aside>

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
            <MapSelectionController selectedProperty={selectedProperty} markerRefs={markerRefs} />
            {markerProperties.map((property) => (
              <Marker
                key={property.id}
                ref={(marker) => {
                  markerRefs.current[property.id] = marker
                }}
                icon={selectedPropertyId === property.id ? selectedPropertyPinIcon : propertyPinIcon}
                position={[property.latitude as number, property.longitude as number]}
                eventHandlers={{
                  click: () => setSelectedPropertyId(property.id),
                }}
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
    </section>
  )
}

function MapSelectionController({
  selectedProperty,
  markerRefs,
}: {
  selectedProperty: Property | null
  markerRefs: MutableRefObject<Record<number, L.Marker | null>>
}) {
  const map = useMap()

  useEffect(() => {
    if (
      !selectedProperty ||
      typeof selectedProperty.latitude !== 'number' ||
      typeof selectedProperty.longitude !== 'number'
    ) {
      return
    }

    const position: [number, number] = [selectedProperty.latitude, selectedProperty.longitude]
    map.flyTo(position, Math.max(map.getZoom(), 15), {
      animate: true,
      duration: 0.75,
    })

    window.setTimeout(() => {
      markerRefs.current[selectedProperty.id]?.openPopup()
    }, 300)
  }, [map, markerRefs, selectedProperty])

  return null
}

function getUniqueCities(properties: Property[], existingCities: string[] = []) {
  return Array.from(
    new Set(
      [...existingCities, ...properties.map((property) => property.city.trim())].filter(Boolean),
    ),
  ).sort((first, second) => first.localeCompare(second))
}
