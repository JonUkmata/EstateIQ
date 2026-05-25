import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import EmptyState from '../components/EmptyState'
import ErrorState from '../components/ErrorState'
import LoadingState from '../components/LoadingState'
import { Permissions } from '../constants/auth'
import { useAuth } from '../context/AuthContext'
import type {
  AdminDashboard,
  AgentDashboard,
  CompanyAdminDashboard,
  Dashboard,
  DashboardProperty,
  UserDashboard,
} from '../services/api'
import { getMyDashboard } from '../services/api'

export default function DashboardPage() {
  const { user } = useAuth()
  const [dashboard, setDashboard] = useState<Dashboard | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => { document.title = 'Dashboard | EstateIQ' }, [])

  useEffect(() => {
    const controller = new AbortController()

    async function load() {
      try {
        const data = await getMyDashboard(controller.signal)
        setDashboard(data)
        setLoading(false)
      } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') return
        setError(err instanceof Error ? err.message : 'Failed to load dashboard.')
        setLoading(false)
      }
    }

    load()
    return () => controller.abort()
  }, [])

  return (
    <div className="content-stack">
      <div className="section-heading">
        <div>
          <p className="eyebrow">Dashboard</p>
          <h1>Hello, {user?.firstName ?? 'there'}</h1>
        </div>
        {user?.roles?.[0] && (
          <span className="response-badge response-badge-success">{user.roles[0]}</span>
        )}
      </div>

      {loading && <LoadingState message="Loading dashboard..." />}
      {error && <ErrorState message={error} />}
      {!loading && !error && !dashboard && (
        <EmptyState message="No dashboard data available." />
      )}

      {dashboard?.role === 'Admin' && <AdminView data={dashboard} />}
      {dashboard?.role === 'CompanyAdmin' && <CompanyAdminView data={dashboard} />}
      {dashboard?.role === 'Agent' && <AgentView data={dashboard} />}
      {dashboard?.role === 'User' && <UserView data={dashboard} />}
    </div>
  )
}

// ── Admin ─────────────────────────────────────────────────────────────────────

function AdminView({ data }: { data: AdminDashboard }) {
  return (
    <>
      <div className="dashboard-metrics">
        <MetricCard value={data.totalProperties} label="Total Properties" />
        <MetricCard value={data.forSaleProperties} label="For Sale" />
        <MetricCard value={data.forRentProperties} label="For Rent" />
        <MetricCard value={data.soldProperties} label="Sold" />
        <MetricCard value={data.totalUsers} label="Users" />
        <MetricCard value={data.totalCompanies} label="Companies" />
        <MetricCard value={data.totalAgents} label="Agents" />
      </div>

      <div className="dashboard-shortcuts">
        <ShortcutCard to="/admin/users" label="Manage Users" desc="View, activate, deactivate users" />
        <ShortcutCard to="/properties" label="All Properties" desc="Browse the full property list" />
        <ShortcutCard to="/map" label="Map Search" desc="Explore properties on the map" />
      </div>

      <RecentPropertiesPanel properties={data.recentProperties} />
    </>
  )
}

// ── CompanyAdmin ──────────────────────────────────────────────────────────────

function CompanyAdminView({ data }: { data: CompanyAdminDashboard }) {
  return (
    <>
      <div className="response-card" style={{ marginBottom: 0 }}>
        <p className="dashboard-section-title">{data.companyName}</p>
        <div className="dashboard-metrics">
          <MetricCard value={data.companyProperties} label="Properties" />
          <MetricCard value={data.forSaleProperties} label="For Sale" />
          <MetricCard value={data.forRentProperties} label="For Rent" />
          <MetricCard value={data.soldProperties} label="Sold" />
          <MetricCard value={data.companyAgents} label="Agents" />
        </div>
      </div>

      <div className="dashboard-shortcuts">
        <ShortcutCard to="/company/agents" label="Company Agents" desc="View and manage your agents" />
        <ShortcutCard to="/properties" label="All Properties" desc="Browse the full property list" />
        <ShortcutCard to="/map" label="Map Search" desc="Explore properties on the map" />
      </div>

      <RecentPropertiesPanel properties={data.recentCompanyProperties} />
    </>
  )
}

// ── Agent ─────────────────────────────────────────────────────────────────────

function AgentView({ data }: { data: AgentDashboard }) {
  const { hasPermission } = useAuth()

  return (
    <>
      <div className="dashboard-metrics">
        <MetricCard value={data.myProperties} label="My Properties" />
        <MetricCard value={data.myForSaleProperties} label="For Sale" />
        <MetricCard value={data.myForRentProperties} label="For Rent" />
        <MetricCard value={data.mySoldProperties} label="Sold" />
        <MetricCard value={data.myRentedProperties} label="Rented" />
      </div>

      <div className="dashboard-shortcuts">
        <ShortcutCard to="/my-properties" label="My Properties" desc="Manage your assigned listings" />
        <ShortcutCard to="/properties" label="Browse All" desc="View all available properties" />
        <ShortcutCard to="/map" label="Map Search" desc="Explore properties on the map" />
        {hasPermission(Permissions.CreateProperty) && (
          <ShortcutCard to="/properties/new" label="Add Property" desc="Create a new property listing" />
        )}
      </div>

      <RecentPropertiesPanel properties={data.recentMyProperties} title="My Recent Properties" />
    </>
  )
}

// ── User ──────────────────────────────────────────────────────────────────────

function UserView({ data }: { data: UserDashboard }) {
  return (
    <>
      <div className="dashboard-metrics">
        <MetricCard value={data.availableProperties} label="Available Properties" />
      </div>

      <div className="dashboard-shortcuts">
        <ShortcutCard to="/properties" label="Browse Properties" desc="Explore properties for sale and rent" />
        <ShortcutCard to="/map" label="Map Search" desc="Find properties near you on the map" />
      </div>

      {data.popularCities.length > 0 && (
        <div className="response-card">
          <p className="dashboard-section-title">Popular Cities</p>
          <div className="popular-cities">
            {data.popularCities.map((city) => (
              <span key={city} className="city-tag">{city}</span>
            ))}
          </div>
        </div>
      )}

      <RecentPropertiesPanel properties={data.latestProperties} title="Latest Listings" />
    </>
  )
}

// ── Shared components ─────────────────────────────────────────────────────────

function MetricCard({ value, label }: { value: number; label: string }) {
  return (
    <div className="metric-card">
      <span className="metric-card-value">{value.toLocaleString()}</span>
      <span className="metric-card-label">{label}</span>
    </div>
  )
}

function ShortcutCard({ to, label, desc }: { to: string; label: string; desc: string }) {
  return (
    <Link to={to} className="shortcut-card">
      <span className="shortcut-card-label">{label}</span>
      <span className="shortcut-card-desc">{desc}</span>
    </Link>
  )
}

function RecentPropertiesPanel({
  properties,
  title = 'Recent Properties',
}: {
  properties: DashboardProperty[]
  title?: string
}) {
  return (
    <div className="response-card">
      <p className="dashboard-section-title">{title}</p>
      {properties.length === 0 ? (
        <p className="dashboard-empty">No properties to display.</p>
      ) : (
        <div className="properties-table-wrap">
          <table className="properties-table">
            <thead>
              <tr>
                <th>Title</th>
                <th>City</th>
                <th>Price</th>
                <th>Status</th>
                <th>Added</th>
              </tr>
            </thead>
            <tbody>
              {properties.map((p) => (
                <tr key={p.id}>
                  <td>
                    <Link className="table-title-link" to={`/properties/${p.id}`}>
                      {p.title}
                    </Link>
                  </td>
                  <td>{p.city}</td>
                  <td>€{p.price.toLocaleString()}</td>
                  <td><span className="status-pill">{p.status}</span></td>
                  <td>{new Date(p.createdAt).toLocaleDateString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
