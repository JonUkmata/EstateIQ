import { useState } from 'react'
import type { ChangeEvent, FormEvent } from 'react'
import { uploadPropertyImages } from '../../services/api'

type PropertyImageUploadProps = {
  propertyId: number
  currentImageCount: number
  onUploaded: () => Promise<void> | void
}

const acceptedExtensions = ['.jpg', '.jpeg', '.png', '.webp']
const acceptedContentTypes = ['image/jpeg', 'image/png', 'image/webp']
const maxFileSizeBytes = 5 * 1024 * 1024
const maxPropertyImages = 10

export default function PropertyImageUpload({
  propertyId,
  currentImageCount,
  onUploaded,
}: PropertyImageUploadProps) {
  const [selectedFiles, setSelectedFiles] = useState<File[]>([])
  const [message, setMessage] = useState<{ text: string; type: 'success' | 'error' } | null>(null)
  const [isUploading, setIsUploading] = useState(false)

  function handleFileChange(event: ChangeEvent<HTMLInputElement>) {
    const files = Array.from(event.target.files ?? [])
    setSelectedFiles(files)
    setMessage(validateFiles(files, currentImageCount))
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const validationMessage = validateFiles(selectedFiles, currentImageCount)
    setMessage(validationMessage)

    if (validationMessage?.type === 'error') {
      return
    }

    setIsUploading(true)
    try {
      await uploadPropertyImages(propertyId, selectedFiles)
      setSelectedFiles([])
      setMessage({ text: 'Images uploaded successfully.', type: 'success' })
      await onUploaded()
    } catch (error) {
      setMessage({
        text: error instanceof Error ? error.message : 'Failed to upload property images.',
        type: 'error',
      })
    } finally {
      setIsUploading(false)
    }
  }

  return (
    <section className="property-images-panel">
      <div className="form-panel-header">
        <div>
          <span className="panel-label">Upload</span>
          <h2>Add Images</h2>
        </div>
        {message ? <span className={`form-message form-message-${message.type}`}>{message.text}</span> : null}
      </div>

      <form className="image-upload-form" onSubmit={handleSubmit}>
        <label className="field field-wide">
          <span>Images</span>
          <input
            accept=".jpg,.jpeg,.png,.webp"
            multiple
            onChange={handleFileChange}
            type="file"
          />
          <small>JPG, PNG or WEBP. Maximum 5MB per image. Maximum 10 images per property.</small>
        </label>

        {selectedFiles.length > 0 ? (
          <ul className="selected-image-list">
            {selectedFiles.map((file) => (
              <li key={`${file.name}-${file.lastModified}`}>
                <span>{file.name}</span>
                <small>{Math.round(file.size / 1024)} KB</small>
              </li>
            ))}
          </ul>
        ) : null}

        <div className="form-actions">
          <button
            type="submit"
            disabled={isUploading || selectedFiles.length === 0 || currentImageCount >= maxPropertyImages}
          >
            {isUploading ? 'Uploading...' : 'Upload images'}
          </button>
        </div>
      </form>
    </section>
  )
}

function validateFiles(files: File[], currentImageCount: number) {
  if (files.length === 0) {
    return null
  }

  if (currentImageCount + files.length > maxPropertyImages) {
    return {
      text: `A property can have at most ${maxPropertyImages} images.`,
      type: 'error' as const,
    }
  }

  const invalidFile = files.find((file) => {
    const extension = file.name.slice(file.name.lastIndexOf('.')).toLowerCase()
    return !acceptedExtensions.includes(extension) || !acceptedContentTypes.includes(file.type)
  })

  if (invalidFile) {
    return {
      text: `${invalidFile.name} is not a supported image type.`,
      type: 'error' as const,
    }
  }

  const oversizedFile = files.find((file) => file.size > maxFileSizeBytes)
  if (oversizedFile) {
    return {
      text: `${oversizedFile.name} is larger than 5MB.`,
      type: 'error' as const,
    }
  }

  return {
    text: `${files.length} ${files.length === 1 ? 'image' : 'images'} ready to upload.`,
    type: 'success' as const,
  }
}
