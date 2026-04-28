import { Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import TaskListPage from './pages/TaskListPage';
import TaskDetailPage from './pages/TaskDetailPage';

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const token = localStorage.getItem('token');
  if (!token) {
    return <Navigate to="/login" replace />;
  }
  return <>{children}</>;
}

function App() {
  return (
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
  );
}

export default App;
