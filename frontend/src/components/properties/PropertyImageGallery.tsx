import { useEffect, useMemo, useState } from 'react'
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
  const [activeImageIndex, setActiveImageIndex] = useState<number | null>(null)
  const activeImage = activeImageIndex === null ? null : images[activeImageIndex]
  const hasMultipleImages = images.length > 1

  const activeImageUrl = useMemo(() => {
    return activeImage ? buildFileUrl(activeImage.url) : ''
  }, [activeImage])

  useEffect(() => {
    if (activeImageIndex === null) {
      return
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') {
        setActiveImageIndex(null)
      }

      if (event.key === 'ArrowLeft') {
        showPreviousImage()
      }

      if (event.key === 'ArrowRight') {
        showNextImage()
      }
    }

    window.addEventListener('keydown', handleKeyDown)
    return () => window.removeEventListener('keydown', handleKeyDown)
  }, [activeImageIndex, images.length])

  function showPreviousImage() {
    setActiveImageIndex((current) => {
      if (current === null) {
        return current
      }

      return current === 0 ? images.length - 1 : current - 1
    })
  }

  function showNextImage() {
    setActiveImageIndex((current) => {
      if (current === null) {
        return current
      }

      return current === images.length - 1 ? 0 : current + 1
    })
  }

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
          {images.map((image, index) => (
            <figure className="property-image-card" key={image.id}>
              <button
                className="property-image-button"
                type="button"
                onClick={() => setActiveImageIndex(index)}
              >
                <img src={buildFileUrl(image.url)} alt={image.fileName} loading="lazy" />
              </button>
              <figcaption>
                <span>{image.fileName}</span>
                <small>{imageSizeFormatter.format(image.fileSize / 1024)} KB</small>
              </figcaption>
            </figure>
          ))}
        </div>
      )}

      {activeImage ? (
        <div className="image-lightbox-backdrop" role="presentation" onClick={() => setActiveImageIndex(null)}>
          <div
            className="image-lightbox"
            role="dialog"
            aria-modal="true"
            aria-label={`Image ${activeImageIndex! + 1} of ${images.length}: ${activeImage.fileName}`}
            onClick={(event) => event.stopPropagation()}
          >
            <div className="image-lightbox-header">
              <div>
                <span className="panel-label">Image {activeImageIndex! + 1} of {images.length}</span>
                <h2>{activeImage.fileName}</h2>
              </div>
              <button type="button" onClick={() => setActiveImageIndex(null)}>
                Close
              </button>
            </div>

            <div className="image-lightbox-stage">
              {hasMultipleImages ? (
                <button className="image-lightbox-nav image-lightbox-nav-prev" type="button" onClick={showPreviousImage}>
                  Previous
                </button>
              ) : null}
              <img src={activeImageUrl} alt={activeImage.fileName} />
              {hasMultipleImages ? (
                <button className="image-lightbox-nav image-lightbox-nav-next" type="button" onClick={showNextImage}>
                  Next
                </button>
              ) : null}
            </div>
          </div>
        </div>
      ) : null}
    </section>
  )
}
