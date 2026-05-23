import { BrowserRouter, Route, Routes } from 'react-router-dom'
import ProtectedRoute from '../components/ProtectedRoute'
import { Permissions, Roles, authenticatedRoles } from '../constants/auth'
import AppLayout from '../layouts/AppLayout'
import AdminUsersPage from '../pages/AdminUsersPage'
import CompanyAgentsPage from '../pages/CompanyAgentsPage'
import DashboardPage from '../pages/DashboardPage'
import EditPropertyPage from '../pages/EditPropertyPage'
import HomePage from '../pages/HomePage'
import LoginPage from '../pages/LoginPage'
import MapPage from '../pages/MapPage'
import MyPropertiesPage from '../pages/MyPropertiesPage'
import PropertyDetailsPage from '../pages/PropertyDetailsPage'
import PropertiesPage from '../pages/PropertiesPage'
import RegisterPage from '../pages/RegisterPage'
import RootEntryPage from '../pages/RootEntryPage'
import VerifyEmailPage from '../pages/VerifyEmailPage'

export default function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<RootEntryPage />} />
        <Route element={<AppLayout />}>
          <Route path="dev/health" element={<HomePage />} />
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
              <ProtectedRoute roles={authenticatedRoles}>
                <DashboardPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="my-properties"
            element={
              <ProtectedRoute
                roles={[Roles.Agent]}
                permissions={[Permissions.CreateProperty]}
                fallbackPath="/dashboard"
              >
                <MyPropertiesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="company-agents"
            element={
              <ProtectedRoute
                roles={[Roles.CompanyAdmin]}
                permissions={[Permissions.ManageAgents]}
                fallbackPath="/dashboard"
              >
                <CompanyAgentsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="admin-users"
            element={
              <ProtectedRoute
                roles={[Roles.Admin]}
                permissions={[Permissions.ManageUsers]}
                fallbackPath="/dashboard"
              >
                <AdminUsersPage />
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
