import { buildFileUrl } from '../../services/api'
import type { PropertyImage } from '../../types/files'

type PropertyImageGalleryProps = {
  images: PropertyImage[]
  isLoading?: boolean
}

const imageSizeFormatter = new Intl.NumberFormat('en-US', {
  maximumFractionDigits: 1,
  minimumFractionDigits: 0,
})

export default function PropertyImageGallery({ images, isLoading = false }: PropertyImageGalleryProps) {
  return (
    <section className="property-images-panel">
      <div className="form-panel-header">
        <div>
          <span className="panel-label">Gallery</span>
          <h2>Property Images</h2>
        </div>
        <span className="response-badge response-badge-success">
          {isLoading ? 'Loading' : `${images.length}/10 images`}
        </span>
      </div>

      {images.length === 0 ? (
        <div className="image-empty-state">
          <p>No images have been uploaded for this property yet.</p>
        </div>
      ) : (
        <div className="property-image-grid">
          {images.map((image) => (
            <figure className="property-image-card" key={image.id}>
              <img src={buildFileUrl(image.url)} alt={image.fileName} loading="lazy" />
              <figcaption>
                <span>{image.fileName}</span>
                <small>{imageSizeFormatter.format(image.fileSize / 1024)} KB</small>
              </figcaption>
            </figure>
          ))}
        </div>
      )}
    </section>
  )
}
