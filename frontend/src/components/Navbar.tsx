import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function Navbar() {
  const navigate = useNavigate()
  const { isAuthenticated, logout, user } = useAuth()

  async function handleLogout() {
    await logout()
    navigate('/login')
  }

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
        {isAuthenticated ? (
          <>
            <span className="top-nav-user">
              {user?.firstName} {user?.lastName}
            </span>
            <button className="top-nav-button" type="button" onClick={() => void handleLogout()}>
              Logout
            </button>
          </>
        ) : (
          <>
            <NavLink className="top-nav-link" to="/register">
              Register
            </NavLink>
            <NavLink className="top-nav-link" to="/login">
              Login
            </NavLink>
          </>
        )}
      </nav>
    </header>
  )
}
