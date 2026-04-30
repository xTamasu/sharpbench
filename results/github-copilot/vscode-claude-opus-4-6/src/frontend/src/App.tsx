// App component: defines routes and wraps protected routes with auth guard.
import { Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import TaskListPage from './pages/TaskListPage';
import TaskDetailPage from './pages/TaskDetailPage';
import ProtectedRoute from './components/ProtectedRoute';

function App() {
  return (
    <div className="min-h-screen bg-gray-50">
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route
          path="/tasks"
          element={
            <ProtectedRoute>
              <TaskListPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/tasks/:id"
          element={
            <ProtectedRoute>
              <TaskDetailPage />
            </ProtectedRoute>
          }
        />
        <Route path="/" element={<Navigate to="/tasks" replace />} />
      </Routes>
    </div>
  );
}

export default App;
