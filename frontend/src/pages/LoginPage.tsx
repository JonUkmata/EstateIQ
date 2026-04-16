import { NavLink } from 'react-router-dom'

export default function LoginPage() {
  return (
    <main className="login-shell">
      <section className="login-card">
        <span className="eyebrow">Login</span>
        <h1>Login route placeholder.</h1>
        <p className="lead">
          This page is reserved for the authentication form and related login flows.
        </p>

        <div className="login-actions">
          <NavLink className="cta-link" to="/">
            Go back home
          </NavLink>
        </div>
      </section>
    </main>
  )
}
