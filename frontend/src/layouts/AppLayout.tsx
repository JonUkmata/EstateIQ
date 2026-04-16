import { Outlet } from 'react-router-dom'
import Navbar from '../components/Navbar'
import Sidebar from '../components/Sidebar'

export default function AppLayout() {
  return (
    <div className="app-frame">
      <Navbar />

      <div className="layout-shell">
        <Sidebar />

        <main className="content-shell">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
