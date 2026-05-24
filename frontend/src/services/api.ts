import type {
  LoginRequest,
  LoginResponse,
  LogoutResponse,
  RegisterRequest,
  RegisterResponse,
  VerifyEmailRequest,
  VerifyEmailResponse,
} from '../types/auth'
import type { PropertyImage, UploadedPropertyImage } from '../types/files'

const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/$/, '')
let accessToken: string | null = null

export function setApiAccessToken(token: string | null) {
  accessToken = token
}

function buildApiUrl(path: string) {
  return `${apiBaseUrl}${path}`
}

export function buildFileUrl(path: string) {
  if (/^https?:\/\//i.test(path)) {
    return path
  }

  return buildApiUrl(path.startsWith('/') ? path : `/${path}`)
}

async function parseErrorDetails(response: Response) {
  const contentType = response.headers.get('content-type') ?? ''
  if (contentType.includes('application/json')) {
    const payload = (await response.json()) as Record<string, unknown>
    const validationErrors = payload.errors
    if (validationErrors && typeof validationErrors === 'object' && !Array.isArray(validationErrors)) {
      const messages = Object.values(validationErrors)
        .flatMap((value) => (Array.isArray(value) ? value : []))
        .filter((value): value is string => typeof value === 'string')

      if (messages.length > 0) {
        return messages.join(' ')
      }
    }

    const message =
      typeof payload.message === 'string'
        ? payload.message
        : typeof payload.detail === 'string'
          ? payload.detail
        : typeof payload.title === 'string'
          ? payload.title
          : ''
    return message || JSON.stringify(payload)
  }

  return await response.text()
}

function buildHttpErrorMessage(endpoint: string, response: Response, details?: string) {
  if (response.status === 404) {
    return details || `We could not find the requested resource at ${endpoint}.`
  }

  if (response.status >= 500) {
    return details || `The server failed while processing ${endpoint}. Please try again.`
  }

  if (details) {
    return details
  }

  return `Request to ${endpoint} failed with status ${response.status}.`
}

function buildNetworkErrorMessage(endpoint: string, error: unknown) {
  if (error instanceof Error) {
    if (error.name === 'AbortError') {
      return error.message
    }

    return `Could not reach ${endpoint}. ${error.message}`
  }

  return `Could not reach ${endpoint}.`
}

async function fetchJson<T>(path: string, options: RequestInit = {}) {
  const endpoint = path
  const headers = new Headers(options.headers)
  if (!headers.has('Accept')) {
    headers.set('Accept', 'application/json')
  }
  if (accessToken && !headers.has('Authorization')) {
    headers.set('Authorization', `Bearer ${accessToken}`)
  }

  let response: Response
  try {
    response = await fetch(buildApiUrl(path), {
      ...options,
      headers,
    })
  } catch (error) {
    throw new Error(buildNetworkErrorMessage(endpoint, error))
  }

  if (!response.ok) {
    const details = await parseErrorDetails(response)
    throw new Error(buildHttpErrorMessage(endpoint, response, details))
  }

  return response.json() as Promise<T>
}

async function fetchText(path: string, options: RequestInit = {}) {
  const endpoint = path
  const headers = new Headers(options.headers)
  if (!headers.has('Accept')) {
    headers.set('Accept', 'text/plain')
  }
  if (accessToken && !headers.has('Authorization')) {
    headers.set('Authorization', `Bearer ${accessToken}`)
  }

  let response: Response
  try {
    response = await fetch(buildApiUrl(path), {
      ...options,
      headers,
    })
  } catch (error) {
    throw new Error(buildNetworkErrorMessage(endpoint, error))
  }

  if (!response.ok) {
    const details = await parseErrorDetails(response)
    throw new Error(buildHttpErrorMessage(endpoint, response, details))
  }

  return response.text()
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
  latitude?: number | null
  longitude?: number | null
  coverImageUrl?: string | null
  propertyType?: {
    id: number
    name: string
  } | null
  propertyStatus?: {
    id: number
    name: string
    colorCode?: string | null
  } | null
}

export type PropertyDetails = Property & {
  propertyType: {
    id: number
    name: string
    description?: string | null
  }
  propertyStatus: {
    id: number
    name: string
    description?: string | null
    colorCode?: string | null
  }
  company: {
    id: number
    name: string
    email?: string | null
    phone?: string | null
    city?: string | null
    isActive: boolean
  }
  agent: {
    id: number
    firstName: string
    lastName: string
    email: string
    phone?: string | null
    mobile?: string | null
    isActive: boolean
  }
  images: PropertyImage[]
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
  createdAt?: string
}

export type CreateAgentPayload = {
  firstName: string
  lastName: string
  email: string
  password: string
  phone?: string | null
}

export type CreateCompanyAdminPayload = {
  firstName: string
  lastName: string
  email: string
  password: string
  companyId: number
}

export type CreateAgentResponse = {
  userId: string
  agentId: number
  email: string
  companyId: number
}

export type CreateCompanyAdminResponse = {
  id: string
  email: string
  role: string
  companyId: number
}

export type AdminUser = {
  id: string
  firstName: string
  lastName: string
  email: string
  isActive: boolean
  isEmailConfirmed: boolean
  createdAt?: string
  roles?: string[]
  companies?: {
    id: number
    name: string
    relationshipType: string
  }[]
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

export type UpdatePropertyPayload = CreatePropertyPayload & {
  id: number
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

type UserQuery = {
  search?: string
  role?: string
  page?: string | number
  pageSize?: string | number
}

export async function getApiTestMessage(signal?: AbortSignal) {
  return fetchText('/api/test', { signal })
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
  const result = await fetchJson<Property[] | PagedResult<Property>>(
    `/api/properties${queryString ? `?${queryString}` : ''}`,
    { signal },
  )

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

export async function getPropertyById(id: number, signal?: AbortSignal) {
  return fetchJson<PropertyDetails>(`/api/properties/${id}`, { signal })
}

export async function getPropertyTypes(signal?: AbortSignal) {
  return fetchJson<PropertyType[]>('/api/propertytypes', { signal })
}

export async function getPropertyStatuses(signal?: AbortSignal) {
  return fetchJson<PropertyStatus[]>('/api/propertystatuses', { signal })
}

export async function getCompanies(signal?: AbortSignal) {
  return fetchJson<Company[]>('/api/companies', { signal })
}

export async function getAgents(signal?: AbortSignal, companyId?: number) {
  const query = companyId ? `?companyId=${companyId}` : ''
  return fetchJson<Agent[]>(`/api/agents${query}`, { signal })
}

export async function getMyCompanyAgents(signal?: AbortSignal) {
  return fetchJson<Agent[]>('/api/agents/my-company', { signal })
}

export async function createAgent(payload: CreateAgentPayload) {
  return fetchJson<CreateAgentResponse>('/api/users/agents', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })
}

export async function createCompanyAdmin(payload: CreateCompanyAdminPayload) {
  return fetchJson<CreateCompanyAdminResponse>('/api/users/company-admins', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })
}

export async function getUsers(signal?: AbortSignal, query?: UserQuery) {
  const parameters = new URLSearchParams()
  const search = query?.search?.trim()
  const role = query?.role?.trim()
  const page = query?.page?.toString().trim()
  const pageSize = query?.pageSize?.toString().trim()

  if (search) {
    parameters.set('search', search)
  }

  if (role) {
    parameters.set('role', role)
  }

  if (page) {
    parameters.set('page', page)
  }

  if (pageSize) {
    parameters.set('pageSize', pageSize)
  }

  const queryString = parameters.toString()
  return fetchJson<PagedResult<AdminUser>>(`/api/users${queryString ? `?${queryString}` : ''}`, {
    signal,
  })
}

export async function createProperty(payload: CreatePropertyPayload) {
  return fetchJson<Property>('/api/properties', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })
}

export async function deleteProperty(id: number) {
  await fetchText(`/api/properties/${id}`, {
    method: 'DELETE',
  })
}

export async function updateProperty(id: number, payload: UpdatePropertyPayload) {
  return fetchJson<PropertyDetails>(`/api/properties/${id}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })
}

export async function getPropertyImages(propertyId: number, signal?: AbortSignal) {
  return fetchJson<PropertyImage[]>(`/api/properties/${propertyId}/images`, { signal })
}

export async function uploadPropertyImages(propertyId: number, files: File[]) {
  const formData = new FormData()
  files.forEach((file) => formData.append('files', file))

  return fetchJson<UploadedPropertyImage[]>(`/api/properties/${propertyId}/images`, {
    method: 'POST',
    body: formData,
  })
}

export type UserListItem = {
  id: string
  firstName: string
  lastName: string
  email: string
  isActive: boolean
  isEmailConfirmed: boolean
  roles: string[]
}

type UserListQuery = {
  search?: string
  page?: string | number
  pageSize?: string | number
}

export type UpdateUserStatusPayload = {
  isActive: boolean
}

export type UpdateUserStatusResponse = {
  id: string
  isActive: boolean
}

export async function getUsers(signal?: AbortSignal, query?: UserListQuery) {
  const parameters = new URLSearchParams()
  const search = query?.search?.trim()
  const page = query?.page?.toString().trim()
  const pageSize = query?.pageSize?.toString().trim()

  if (search) parameters.set('search', search)
  if (page) parameters.set('page', page)
  if (pageSize) parameters.set('pageSize', pageSize)

  const queryString = parameters.toString()
  return fetchJson<PagedResult<UserListItem>>(
    `/api/users${queryString ? `?${queryString}` : ''}`,
    { signal },
  )
}

export async function updateUserStatus(id: string, payload: UpdateUserStatusPayload) {
  return fetchJson<UpdateUserStatusResponse>(`/api/users/${id}/status`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
}

export async function registerUser(payload: RegisterRequest) {
  return fetchJson<RegisterResponse>('/api/auth/register', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })
}

export async function verifyEmail(payload: VerifyEmailRequest) {
  return fetchJson<VerifyEmailResponse>('/api/auth/verify-email', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })
}

export async function loginUser(payload: LoginRequest) {
  return fetchJson<LoginResponse>('/api/auth/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })
}

export async function logoutUser(refreshToken?: string) {
  return fetchJson<LogoutResponse>('/api/auth/logout', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ refreshToken: refreshToken ?? null }),
  })
}
