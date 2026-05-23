import { Navigate } from 'react-router-dom'
import AuthLandingPage from './AuthLandingPage'
import { useAuth } from '../context/AuthContext'
import { getAuthenticatedHomePath } from '../utils/authRedirects'

export default function RootEntryPage() {
  const { isAuthenticated, user } = useAuth()

  if (!isAuthenticated) {
    return <AuthLandingPage />
  }

  return <Navigate to={getAuthenticatedHomePath(user)} replace />
}
