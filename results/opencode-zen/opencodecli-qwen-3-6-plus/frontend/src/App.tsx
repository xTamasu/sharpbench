// App.tsx — root component with React Router setup and protected routes.

import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import LoginPage from '@/pages/LoginPage';
import RegisterPage from '@/pages/RegisterPage';
import TaskListPage from '@/pages/TaskListPage';
import TaskDetailPage from '@/pages/TaskDetailPage';
import ProtectedRoute from '@/components/ProtectedRoute';

const queryClient = new QueryClient();

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
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
          <Route path="*" element={<Navigate to="/tasks" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
