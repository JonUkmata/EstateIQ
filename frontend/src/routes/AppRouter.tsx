import { BrowserRouter, Route, Routes } from 'react-router-dom'
import AppLayout from '../layouts/AppLayout'
import DashboardPage from '../pages/DashboardPage'
import EditPropertyPage from '../pages/EditPropertyPage'
import HomePage from '../pages/HomePage'
import LoginPage from '../pages/LoginPage'
import MapPage from '../pages/MapPage'
import PropertyDetailsPage from '../pages/PropertyDetailsPage'
import PropertiesPage from '../pages/PropertiesPage'

export default function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<AppLayout />}>
          <Route index element={<HomePage />} />
          <Route path="properties" element={<PropertiesPage />} />
          <Route path="properties/:id" element={<PropertyDetailsPage />} />
          <Route path="properties/:id/edit" element={<EditPropertyPage />} />
          <Route path="map" element={<MapPage />} />
          <Route path="dashboard" element={<DashboardPage />} />
        </Route>
        <Route path="/login" element={<LoginPage />} />
      </Routes>
    </BrowserRouter>
  )
}
