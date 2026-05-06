import { BrowserRouter, Route, Routes } from 'react-router-dom'
import ProtectedRoute from '../components/ProtectedRoute'
import { Permissions, staffRoles } from '../constants/auth'
import AppLayout from '../layouts/AppLayout'
import DashboardPage from '../pages/DashboardPage'
import EditPropertyPage from '../pages/EditPropertyPage'
import HomePage from '../pages/HomePage'
import LoginPage from '../pages/LoginPage'
import MapPage from '../pages/MapPage'
import PropertyDetailsPage from '../pages/PropertyDetailsPage'
import PropertiesPage from '../pages/PropertiesPage'
import RegisterPage from '../pages/RegisterPage'
import VerifyEmailPage from '../pages/VerifyEmailPage'

export default function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<AppLayout />}>
          <Route index element={<HomePage />} />
          <Route path="properties" element={<PropertiesPage />} />
          <Route path="properties/:id" element={<PropertyDetailsPage />} />
          <Route
            path="properties/:id/edit"
            element={
              <ProtectedRoute permissions={[Permissions.EditProperty]} fallbackPath="/properties">
                <EditPropertyPage />
              </ProtectedRoute>
            }
          />
          <Route path="map" element={<MapPage />} />
          <Route
            path="dashboard"
            element={
              <ProtectedRoute roles={staffRoles}>
                <DashboardPage />
              </ProtectedRoute>
            }
          />
        </Route>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/verify-email" element={<VerifyEmailPage />} />
      </Routes>
    </BrowserRouter>
  )
}
