// ProtectedRoute.tsx
// Wrapper that redirects unauthenticated visitors to /login, preserving
// the originally requested URL in location.state so we can return after login.
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from './AuthContext';
import type { ReactNode } from 'react';

export default function ProtectedRoute({ children }: { children?: ReactNode }) {
  const { isAuthenticated } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }
  return <>{children ?? <Outlet />}</>;
}
