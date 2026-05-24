import { type FormEvent, useEffect, useMemo, useState } from 'react'
import LoadingSpinner from '../components/LoadingSpinner'
import { Permissions } from '../constants/auth'
import { useAuth } from '../context/AuthContext'
import {
  createCompanyAdmin,
  getCompanies,
  getUsers,
  updateUserStatus,
  type AdminUser,
  type Company,
  type CreateCompanyAdminPayload,
  type PagedResult,
} from '../services/api'

type LoadState = 'loading' | 'success' | 'error'
type SubmitState = 'idle' | 'submitting' | 'success' | 'error'
type FormErrors = Partial<Record<keyof CreateCompanyAdminPayload, string>>

const pageSize = 10
const roleOptions = ['', 'Admin', 'CompanyAdmin', 'Agent', 'User']
const initialForm: CreateCompanyAdminPayload = {
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  companyId: 0,
}

const dateFormatter = new Intl.DateTimeFormat('en-US', {
  year: 'numeric',
  month: 'short',
  day: 'numeric',
})

export default function AdminUsersPage() {
  const { user: currentUser, hasPermission } = useAuth()
  const canManageUsers = hasPermission(Permissions.ManageUsers)

  const [users, setUsers] = useState<AdminUser[]>([])
  const [companies, setCompanies] = useState<Company[]>([])
  const [loadState, setLoadState] = useState<LoadState>('loading')
  const [companyLoadState, setCompanyLoadState] = useState<LoadState>('loading')
  const [submitState, setSubmitState] = useState<SubmitState>('idle')
  const [errorMessage, setErrorMessage] = useState('')
  const [companyErrorMessage, setCompanyErrorMessage] = useState('')
  const [submitMessage, setSubmitMessage] = useState('')
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState('')
  const [role, setRole] = useState('')
  const [page, setPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [totalPages, setTotalPages] = useState(0)
  const [form, setForm] = useState<CreateCompanyAdminPayload>(initialForm)
  const [formErrors, setFormErrors] = useState<FormErrors>({})
  const [reloadKey, setReloadKey] = useState(0)
  const [actionMessage, setActionMessage] = useState<{ text: string; type: 'success' | 'error' } | null>(null)
  const [pendingDeactivate, setPendingDeactivate] = useState<AdminUser | null>(null)
  const [updatingUserId, setUpdatingUserId] = useState<string | null>(null)

  useEffect(() => {
    const controller = new AbortController()

    async function loadUsers() {
      try {
        setLoadState('loading')
        setErrorMessage('')

        const result = await getUsers(controller.signal, {
          search,
          role,
          page,
          pageSize,
        })
        applyResult(result)
        setLoadState('success')
      } catch (error) {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return
        }

        setErrorMessage(error instanceof Error ? error.message : 'Failed to load users.')
        setLoadState('error')
      }
    }

    void loadUsers()

    return () => controller.abort()
  }, [page, reloadKey, role, search])

  useEffect(() => {
    const controller = new AbortController()

    async function loadCompanies() {
      try {
        setCompanyLoadState('loading')
        setCompanyErrorMessage('')

        const result = await getCompanies(controller.signal)
        setCompanies(result.filter((company) => company.isActive))
        setCompanyLoadState('success')
      } catch (error) {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return
        }

        setCompanyErrorMessage(error instanceof Error ? error.message : 'Failed to load companies.')
        setCompanyLoadState('error')
      }
    }

    void loadCompanies()

    return () => controller.abort()
  }, [])

  const userCountLabel = useMemo(() => {
    if (loadState === 'loading') {
      return 'Loading'
    }

    return `${totalCount} ${totalCount === 1 ? 'user' : 'users'}`
  }, [loadState, totalCount])

  function applyResult(result: PagedResult<AdminUser>) {
    setUsers(result.items)
    setTotalCount(result.totalCount)
    setTotalPages(result.totalPages)
  }

  function handleSearchSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSearch(searchInput.trim())
    setPage(1)
  }

  function handleRoleChange(nextRole: string) {
    setRole(nextRole)
    setPage(1)
  }

  function updateFormField(field: keyof CreateCompanyAdminPayload, value: string) {
    setForm((current) => ({
      ...current,
      [field]: field === 'companyId' ? Number(value) : value,
    }))
    setFormErrors((current) => ({
      ...current,
      [field]: undefined,
    }))
  }

  async function handleCreateCompanyAdmin(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const validation = validateForm(form)
    setFormErrors(validation)
    setSubmitMessage('')

    if (Object.keys(validation).length > 0) {
      setSubmitState('error')
      setSubmitMessage('Please fix the highlighted fields.')
      return
    }

    try {
      setSubmitState('submitting')
      await createCompanyAdmin({
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        email: form.email.trim(),
        password: form.password,
        companyId: form.companyId,
      })
      setForm(initialForm)
      setFormErrors({})
      setSearch('')
      setSearchInput('')
      setRole('CompanyAdmin')
      setPage(1)
      setReloadKey((current) => current + 1)
      setSubmitState('success')
      setSubmitMessage('CompanyAdmin created successfully.')
    } catch (error) {
      setSubmitState('error')
      setSubmitMessage(error instanceof Error ? error.message : 'Failed to create CompanyAdmin.')
    }
  }

  async function handleStatusUpdate(targetUser: AdminUser, newStatus: boolean) {
    setUpdatingUserId(targetUser.id)
    setActionMessage(null)

    try {
      const response = await updateUserStatus(targetUser.id, { isActive: newStatus })
      setUsers((current) =>
        current.map((u) => (u.id === response.id ? { ...u, isActive: response.isActive } : u)),
      )
      setActionMessage({
        text: `${targetUser.firstName} ${targetUser.lastName} has been ${response.isActive ? 'activated' : 'deactivated'}.`,
        type: 'success',
      })
    } catch (error) {
      setActionMessage({
        text: error instanceof Error ? error.message : 'Failed to update user status.',
        type: 'error',
      })
    } finally {
      setUpdatingUserId(null)
      setPendingDeactivate(null)
    }
  }

  function handleToggleStatus(user: AdminUser) {
    setActionMessage(null)
    if (user.isActive) {
      setPendingDeactivate(user)
    } else {
      void handleStatusUpdate(user, true)
    }
  }

  return (
    <section className="content-stack">
      <div className="section-heading">
        <div>
          <span className="eyebrow">Admin</span>
          <h1>Users</h1>
        </div>
        <span className={`response-badge response-badge-${loadState}`}>
          {loadState === 'error' ? 'Error' : userCountLabel}
        </span>
      </div>

      <form className="search-panel admin-users-filter" onSubmit={handleSearchSubmit}>
        <label className="search-field">
          <span>Search</span>
          <input
            value={searchInput}
            onChange={(event) => setSearchInput(event.target.value)}
            placeholder="Name or email"
            type="search"
          />
        </label>

        <label className="filter-field">
          <span>Role</span>
          <select value={role} onChange={(event) => handleRoleChange(event.target.value)}>
            {roleOptions.map((roleOption) => (
              <option key={roleOption || 'all'} value={roleOption}>
                {roleOption || 'All roles'}
              </option>
            ))}
          </select>
        </label>

        <div className="filter-actions">
          <button type="submit">Apply</button>
        </div>
      </form>

      <section className="data-panel" aria-live="polite">
        {actionMessage && (
          <div className={`table-message table-message-${actionMessage.type}`}>
            <span>{actionMessage.text}</span>
          </div>
        )}

        {loadState === 'loading' ? (
          <div className="table-state">
            <p className="state-with-spinner">
              <LoadingSpinner label="Loading users" />
              <span>Loading users...</span>
            </p>
          </div>
        ) : null}

        {loadState === 'error' ? (
          <div className="table-state table-state-error">
            <p>{errorMessage}</p>
          </div>
        ) : null}

        {loadState === 'success' && users.length === 0 ? (
          <div className="table-state">
            <p>No users match the current filters.</p>
          </div>
        ) : null}

        {loadState === 'success' && users.length > 0 ? (
          <>
            <div className="properties-table-wrap">
              <table className="properties-table admin-users-table">
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Email</th>
                    <th>Role</th>
                    <th>Status</th>
                    <th>Verification</th>
                    <th>Company</th>
                    <th>Created</th>
                    {canManageUsers && <th>Actions</th>}
                  </tr>
                </thead>
                <tbody>
                  {users.map((user) => (
                    <tr key={user.id}>
                      <td data-label="Name">{formatName(user)}</td>
                      <td data-label="Email">{user.email}</td>
                      <td data-label="Role">{formatRoles(user.roles)}</td>
                      <td data-label="Status">
                        <span
                          className={`user-status-badge user-status-badge-${user.isActive ? 'active' : 'inactive'}`}
                        >
                          {user.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </td>
                      <td data-label="Email confirmed">
                        {user.isEmailConfirmed ? 'Verified' : 'Unverified'}
                      </td>
                      <td data-label="Company">{formatCompanies(user)}</td>
                      <td data-label="Created">{formatDate(user.createdAt)}</td>
                      {canManageUsers && (
                        <td data-label="Actions">
                          <div className="table-actions">
                            <button
                              type="button"
                              className={user.isActive ? 'table-action-danger' : 'table-action-activate'}
                              disabled={updatingUserId === user.id || user.id === currentUser?.id}
                              onClick={() => handleToggleStatus(user)}
                              title={
                                user.id === currentUser?.id
                                  ? 'You cannot change your own status'
                                  : undefined
                              }
                            >
                              {updatingUserId === user.id
                                ? user.isActive
                                  ? 'Deactivating...'
                                  : 'Activating...'
                                : user.isActive
                                  ? 'Deactivate'
                                  : 'Activate'}
                            </button>
                          </div>
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="pagination-bar">
              <span className="pagination-summary">
                Page {page} of {Math.max(totalPages, 1)}
              </span>
              <div className="pagination-controls">
                <button
                  type="button"
                  onClick={() => setPage((current) => current - 1)}
                  disabled={page <= 1}
                >
                  Previous
                </button>
                <button
                  type="button"
                  onClick={() => setPage((current) => current + 1)}
                  disabled={page >= totalPages}
                >
                  Next
                </button>
              </div>
            </div>
          </>
        ) : null}
      </section>

      <section className="form-panel">
        <div className="form-panel-header">
          <div>
            <span className="panel-label">Create</span>
            <h2>Add CompanyAdmin</h2>
          </div>
          {submitMessage ? (
            <span
              className={`form-message ${
                submitState === 'success' ? 'form-message-success' : 'form-message-error'
              }`}
            >
              {submitMessage}
            </span>
          ) : null}
        </div>

        {companyLoadState === 'error' ? (
          <div className="table-state table-state-error">
            <p>{companyErrorMessage}</p>
          </div>
        ) : null}

        <form className="property-form" onSubmit={handleCreateCompanyAdmin} noValidate>
          <label className="field">
            <span>First name</span>
            <input
              value={form.firstName}
              onChange={(event) => updateFormField('firstName', event.target.value)}
              autoComplete="given-name"
            />
            {formErrors.firstName ? <small>{formErrors.firstName}</small> : null}
          </label>

          <label className="field">
            <span>Last name</span>
            <input
              value={form.lastName}
              onChange={(event) => updateFormField('lastName', event.target.value)}
              autoComplete="family-name"
            />
            {formErrors.lastName ? <small>{formErrors.lastName}</small> : null}
          </label>

          <label className="field">
            <span>Email</span>
            <input
              value={form.email}
              onChange={(event) => updateFormField('email', event.target.value)}
              autoComplete="email"
              inputMode="email"
              type="email"
            />
            {formErrors.email ? <small>{formErrors.email}</small> : null}
          </label>

          <label className="field">
            <span>Temporary password</span>
            <input
              value={form.password}
              onChange={(event) => updateFormField('password', event.target.value)}
              autoComplete="new-password"
              type="password"
            />
            {formErrors.password ? <small>{formErrors.password}</small> : null}
          </label>

          <label className="field field-wide">
            <span>Company</span>
            <select
              value={form.companyId || ''}
              onChange={(event) => updateFormField('companyId', event.target.value)}
              disabled={companyLoadState !== 'success' || companies.length === 0}
            >
              <option value="">
                {companyLoadState === 'loading'
                  ? 'Loading companies'
                  : companies.length === 0
                    ? 'No active companies'
                    : 'Select company'}
              </option>
              {companies.map((company) => (
                <option key={company.id} value={company.id}>
                  {company.name}
                </option>
              ))}
            </select>
            {formErrors.companyId ? <small>{formErrors.companyId}</small> : null}
          </label>

          <div className="form-actions">
            <button
              type="submit"
              disabled={submitState === 'submitting' || companyLoadState !== 'success' || companies.length === 0}
            >
              {submitState === 'submitting' ? 'Creating...' : 'Create CompanyAdmin'}
            </button>
          </div>
        </form>
      </section>

      {pendingDeactivate && canManageUsers && (
        <div className="dialog-backdrop" role="presentation">
          <div
            className="confirm-dialog"
            role="dialog"
            aria-modal="true"
            aria-labelledby="deactivate-user-title"
          >
            <div>
              <span className="panel-label">Confirm</span>
              <h2 id="deactivate-user-title">Deactivate User</h2>
            </div>
            <p>
              Are you sure you want to deactivate{' '}
              <strong>
                {pendingDeactivate.firstName} {pendingDeactivate.lastName}
              </strong>
              ? Their active sessions will be revoked.
            </p>
            <div className="confirm-dialog-actions">
              <button
                type="button"
                className="dialog-button-secondary"
                onClick={() => setPendingDeactivate(null)}
                disabled={updatingUserId === pendingDeactivate.id}
              >
                Cancel
              </button>
              <button
                type="button"
                className="dialog-button-danger"
                onClick={() => void handleStatusUpdate(pendingDeactivate, false)}
                disabled={updatingUserId === pendingDeactivate.id}
              >
                {updatingUserId === pendingDeactivate.id ? 'Deactivating...' : 'Deactivate'}
              </button>
            </div>
          </div>
        </div>
      )}
    </section>
  )
}

function formatName(user: AdminUser) {
  return `${user.firstName} ${user.lastName}`.trim() || 'Unnamed user'
}

function formatRoles(roles?: string[]) {
  return roles && roles.length > 0 ? roles.join(', ') : 'No role'
}

function formatCompanies(user: AdminUser) {
  if (!user.companies || user.companies.length === 0) {
    return 'Not assigned'
  }

  return user.companies.map((company) => `${company.name} (${company.relationshipType})`).join(', ')
}

function formatDate(value?: string) {
  if (!value) {
    return 'Not available'
  }

  const parsed = new Date(value)
  if (Number.isNaN(parsed.getTime())) {
    return 'Not available'
  }

  return dateFormatter.format(parsed)
}

function validateForm(form: CreateCompanyAdminPayload) {
  const errors: FormErrors = {}
  const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

  if (!form.firstName.trim()) {
    errors.firstName = 'First name is required.'
  }

  if (!form.lastName.trim()) {
    errors.lastName = 'Last name is required.'
  }

  if (!form.email.trim()) {
    errors.email = 'Email is required.'
  } else if (!emailPattern.test(form.email.trim())) {
    errors.email = 'Enter a valid email address.'
  }

  if (!form.password) {
    errors.password = 'Temporary password is required.'
  } else if (form.password.length < 8) {
    errors.password = 'Temporary password must be at least 8 characters.'
  } else if (!/[A-Z]/.test(form.password)) {
    errors.password = 'Temporary password must include an uppercase letter.'
  } else if (!/[a-z]/.test(form.password)) {
    errors.password = 'Temporary password must include a lowercase letter.'
  } else if (!/\d/.test(form.password)) {
    errors.password = 'Temporary password must include a number.'
  } else if (!/[^A-Za-z0-9]/.test(form.password)) {
    errors.password = 'Temporary password must include a symbol.'
  }

  if (!form.companyId) {
    errors.companyId = 'Select a company.'
  }

  return errors
}
