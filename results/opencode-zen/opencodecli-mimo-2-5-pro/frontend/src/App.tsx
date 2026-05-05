// Main application component with routing and protected route logic.

import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from './hooks/useAuth';
import Navbar from './components/Navbar';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import TaskListPage from './pages/TaskListPage';
import TaskDetailPage from './pages/TaskDetailPage';

function App() {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <div className="flex items-center justify-center min-h-screen">Loading...</div>;
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <main className="container mx-auto px-4 py-8">
        <Routes>
          <Route
            path="/login"
            element={isAuthenticated ? <Navigate to="/tasks" /> : <LoginPage />}
          />
          <Route
            path="/register"
            element={isAuthenticated ? <Navigate to="/tasks" /> : <RegisterPage />}
          />
          <Route
            path="/tasks"
            element={isAuthenticated ? <TaskListPage /> : <Navigate to="/login" />}
          />
          <Route
            path="/tasks/:id"
            element={isAuthenticated ? <TaskDetailPage /> : <Navigate to="/login" />}
          />
          <Route path="/" element={<Navigate to="/tasks" />} />
          <Route path="*" element={<Navigate to="/tasks" />} />
        </Routes>
      </main>
    </div>
  );
}

export default App;
