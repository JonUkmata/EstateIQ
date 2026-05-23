import { NavLink } from 'react-router-dom'

export default function AuthLandingPage() {
  return (
    <main className="auth-landing-shell">
      <section className="auth-landing">
        <div className="auth-landing-copy">
          <span className="eyebrow">EstateIQ</span>
          <h1>Real estate intelligence for buyers, agents, and operators.</h1>
          <p className="lead">
            Sign in to manage your workspace or create an account to browse property tools with a
            verified profile.
          </p>

          <div className="auth-landing-actions" aria-label="Authentication options">
            <NavLink className="cta-link auth-landing-primary" to="/login">
              Login
            </NavLink>
            <NavLink className="cta-link cta-link-secondary" to="/register">
              Register
            </NavLink>
          </div>
        </div>

        <div className="auth-landing-panel" aria-label="Platform summary">
          <div>
            <span>Role-based access</span>
            <strong>Admin, company, agent, and user flows</strong>
          </div>
          <div>
            <span>Property workspace</span>
            <strong>Listings, maps, details, and management tools</strong>
          </div>
          <div>
            <span>Secure entry</span>
            <strong>Verified accounts with protected dashboard routes</strong>
          </div>
        </div>
      </section>
    </main>
  )
}
