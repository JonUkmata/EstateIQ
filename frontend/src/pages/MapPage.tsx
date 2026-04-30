import { MapContainer, TileLayer } from 'react-leaflet'
import 'leaflet/dist/leaflet.css'

const tiranaCenter: [number, number] = [41.3275, 19.8187]

export default function MapPage() {
  return (
    <section className="content-stack">
      <div className="section-heading">
        <div>
          <span className="eyebrow">Map</span>
          <h1>Property Map</h1>
        </div>
        <span className="response-badge response-badge-success">Ready</span>
      </div>

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
        </MapContainer>
      </section>
    </section>
  )
}
