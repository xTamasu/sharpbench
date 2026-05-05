import { Navigate } from 'react-router-dom'
import { isAuthenticated } from '../hooks/useAuth'

/**
 * Wrapper component that redirects to /login if the user is not authenticated.
 */
interface ProtectedRouteProps {
  children: React.ReactNode
}

export default function ProtectedRoute({ children }: ProtectedRouteProps) {
  if (!isAuthenticated()) {
    return <Navigate to="/login" replace />
  }

  return <>{children}</>
}
