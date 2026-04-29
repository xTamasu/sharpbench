// AppShell.tsx — layout for authenticated pages: header with logout and
// an <Outlet /> for the routed page content.
import { Link, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export default function AppShell() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login', { replace: true });
  };

  return (
    <div className="min-h-full flex flex-col">
      <header className="bg-white border-b border-slate-200">
        <div className="max-w-5xl mx-auto px-4 py-3 flex items-center justify-between">
          <Link to="/tasks" className="font-semibold text-lg">Task Manager</Link>
          <div className="flex items-center gap-3 text-sm">
            <span className="text-slate-600">{user?.displayName}</span>
            <button
              type="button"
              onClick={handleLogout}
              className="px-3 py-1 rounded border border-slate-300 hover:bg-slate-100"
            >
              Log out
            </button>
          </div>
        </div>
      </header>
      <main className="flex-1 max-w-5xl mx-auto w-full px-4 py-6">
        <Outlet />
      </main>
    </div>
  );
}
