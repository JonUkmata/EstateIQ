import { NavLink } from 'react-router-dom'

export default function Navbar() {
  return (
    <header className="navbar">
      <div className="brand-block">
        <span className="brand-kicker">EstateIQ Platform</span>
        <NavLink className="brand-link" to="/">
          EstateIQ
        </NavLink>
      </div>

      <nav className="top-nav" aria-label="Top navigation">
        <NavLink className="top-nav-link" to="/">
          Home
        </NavLink>
        <NavLink className="top-nav-link" to="/login">
          Login
        </NavLink>
      </nav>
    </header>
  )
}
