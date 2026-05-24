import { type FormEvent, useEffect, useMemo, useState } from 'react'
import LoadingSpinner from '../components/LoadingSpinner'
import { getUsers, type AdminUser, type PagedResult } from '../services/api'

type LoadState = 'loading' | 'success' | 'error'

const pageSize = 10
const roleOptions = ['', 'Admin', 'CompanyAdmin', 'Agent', 'User']

const dateFormatter = new Intl.DateTimeFormat('en-US', {
  year: 'numeric',
  month: 'short',
  day: 'numeric',
})

export default function AdminUsersPage() {
  const [users, setUsers] = useState<AdminUser[]>([])
  const [loadState, setLoadState] = useState<LoadState>('loading')
  const [errorMessage, setErrorMessage] = useState('')
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState('')
  const [role, setRole] = useState('')
  const [page, setPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [totalPages, setTotalPages] = useState(0)

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
  }, [page, role, search])

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
                    <th>Verification</th>
                    <th>Role</th>
                    <th>Status</th>
                    <th>Email</th>
                    <th>Company</th>
                    <th>Created</th>
                  </tr>
                </thead>
                <tbody>
                  {users.map((user) => (
                    <tr key={user.id}>
                      <td data-label="Name">{formatName(user)}</td>
                      <td data-label="Email">{user.email}</td>
                      <td data-label="Role">{formatRoles(user.roles)}</td>
                      <td data-label="Status">
                        <span className="status-pill">{user.isActive ? 'Active' : 'Inactive'}</span>
                      </td>
                      <td data-label="Email confirmed">
                        {user.isEmailConfirmed ? 'Verified' : 'Unverified'}
                      </td>
                      <td data-label="Company">{formatCompanies(user)}</td>
                      <td data-label="Created">{formatDate(user.createdAt)}</td>
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
                <button type="button" onClick={() => setPage((current) => current - 1)} disabled={page <= 1}>
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
    </section>
  )
}

function formatName(user: AdminUser) {
  return `${user.firstName} ${user.lastName}`.trim() || 'Unnamed user'
}

function formatRoles(roles: string[]) {
  return roles.length > 0 ? roles.join(', ') : 'No role'
}

function formatCompanies(user: AdminUser) {
  if (user.companies.length === 0) {
    return 'Not assigned'
  }

  return user.companies.map((company) => `${company.name} (${company.relationshipType})`).join(', ')
}

function formatDate(value: string) {
  const parsed = new Date(value)
  if (Number.isNaN(parsed.getTime())) {
    return 'Not available'
  }

  return dateFormatter.format(parsed)
}
