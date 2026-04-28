const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/$/, '')

function buildApiUrl(path: string) {
  return `${apiBaseUrl}${path}`
}

export type Property = {
  id: number
  title: string
  description?: string | null
  price: number
  area: number
  address: string
  city: string
  bedrooms?: number | null
  bathrooms?: number | null
  floors?: number | null
  yearBuilt?: number | null
  propertyStatus?: {
    id: number
    name: string
    colorCode?: string | null
  } | null
}

export type PropertyType = {
  id: number
  name: string
  description?: string | null
}

export type PropertyStatus = {
  id: number
  name: string
  description?: string | null
  colorCode?: string | null
}

export type Company = {
  id: number
  name: string
  city?: string | null
  isActive: boolean
}

export type Agent = {
  id: number
  firstName: string
  lastName: string
  email: string
  phone?: string | null
  mobile?: string | null
  isActive: boolean
}

export type CreatePropertyPayload = {
  title: string
  description?: string | null
  price: number
  area: number
  bedrooms?: number | null
  bathrooms?: number | null
  floors?: number | null
  yearBuilt?: number | null
  propertyTypeId: number
  propertyStatusId: number
  companyId: number
  agentId: number
  address: string
  city: string
  latitude?: number | null
  longitude?: number | null
}

export type PagedResult<T> = {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

type PropertyQuery = {
  search?: string
  city?: string
  propertyTypeId?: string | number
  propertyStatusId?: string | number
  minPrice?: string | number
  maxPrice?: string | number
  page?: string | number
  pageSize?: string | number
}

export async function getApiTestMessage(signal?: AbortSignal) {
  const response = await fetch(buildApiUrl('/api/test'), {
    headers: {
      Accept: 'text/plain',
    },
    signal,
  })

  if (!response.ok) {
    throw new Error(`Request failed with status ${response.status}`)
  }

  return response.text()
}

export async function getProperties(signal?: AbortSignal, query?: PropertyQuery) {
  const parameters = new URLSearchParams()
  const search = query?.search?.trim()
  const city = query?.city?.toString().trim()
  const propertyTypeId = query?.propertyTypeId?.toString().trim()
  const propertyStatusId = query?.propertyStatusId?.toString().trim()
  const minPrice = query?.minPrice?.toString().trim()
  const maxPrice = query?.maxPrice?.toString().trim()
  const page = query?.page?.toString().trim()
  const pageSize = query?.pageSize?.toString().trim()

  if (search) {
    parameters.set('search', search)
  }

  if (city) {
    parameters.set('city', city)
  }

  if (propertyTypeId) {
    parameters.set('propertyTypeId', propertyTypeId)
  }

  if (propertyStatusId) {
    parameters.set('propertyStatusId', propertyStatusId)
  }

  if (minPrice) {
    parameters.set('minPrice', minPrice)
  }

  if (maxPrice) {
    parameters.set('maxPrice', maxPrice)
  }

  if (page) {
    parameters.set('page', page)
  }

  if (pageSize) {
    parameters.set('pageSize', pageSize)
  }

  const queryString = parameters.toString()
  const response = await fetch(buildApiUrl(`/api/properties${queryString ? `?${queryString}` : ''}`), {
    headers: {
      Accept: 'application/json',
    },
    signal,
  })

  if (!response.ok) {
    throw new Error(`Request failed with status ${response.status}`)
  }

  const result = (await response.json()) as Property[] | PagedResult<Property>

  return Array.isArray(result)
    ? {
        items: result,
        totalCount: result.length,
        page: 1,
        pageSize: result.length,
        totalPages: result.length > 0 ? 1 : 0,
      }
    : result
}

export async function getPropertyTypes(signal?: AbortSignal) {
  const response = await fetch(buildApiUrl('/api/propertytypes'), {
    headers: {
      Accept: 'application/json',
    },
    signal,
  })

  if (!response.ok) {
    throw new Error(`Request failed with status ${response.status}`)
  }

  return response.json() as Promise<PropertyType[]>
}

export async function getPropertyStatuses(signal?: AbortSignal) {
  const response = await fetch(buildApiUrl('/api/propertystatuses'), {
    headers: {
      Accept: 'application/json',
    },
    signal,
  })

  if (!response.ok) {
    throw new Error(`Request failed with status ${response.status}`)
  }

  return response.json() as Promise<PropertyStatus[]>
}

export async function getCompanies(signal?: AbortSignal) {
  const response = await fetch(buildApiUrl('/api/companies'), {
    headers: {
      Accept: 'application/json',
    },
    signal,
  })

  if (!response.ok) {
    throw new Error(`Request failed with status ${response.status}`)
  }

  return response.json() as Promise<Company[]>
}

export async function getAgents(signal?: AbortSignal, companyId?: number) {
  const query = companyId ? `?companyId=${companyId}` : ''
  const response = await fetch(buildApiUrl(`/api/agents${query}`), {
    headers: {
      Accept: 'application/json',
    },
    signal,
  })

  if (!response.ok) {
    throw new Error(`Request failed with status ${response.status}`)
  }

  return response.json() as Promise<Agent[]>
}

export async function createProperty(payload: CreatePropertyPayload) {
  const response = await fetch(buildApiUrl('/api/properties'), {
    method: 'POST',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

  if (!response.ok) {
    const contentType = response.headers.get('content-type') ?? ''
    const details = contentType.includes('application/json')
      ? JSON.stringify(await response.json())
      : await response.text()

    throw new Error(details || `Request failed with status ${response.status}`)
  }

  return response.json() as Promise<Property>
}
