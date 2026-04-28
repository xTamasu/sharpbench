// Authentication context and hook for managing login state across the app
import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';

interface AuthContextType {
  token: string | null;
  userId: string | null;
  displayName: string | null;
  isAuthenticated: boolean;
  login: (token: string, userId: string, displayName: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(localStorage.getItem('token'));
  const [userId, setUserId] = useState<string | null>(localStorage.getItem('userId'));
  const [displayName, setDisplayName] = useState<string | null>(localStorage.getItem('displayName'));

  useEffect(() => {
    if (token) {
      localStorage.setItem('token', token);
    } else {
      localStorage.removeItem('token');
    }
  }, [token]);

  useEffect(() => {
    if (userId) {
      localStorage.setItem('userId', userId);
    } else {
      localStorage.removeItem('userId');
    }
  }, [userId]);

  useEffect(() => {
    if (displayName) {
      localStorage.setItem('displayName', displayName);
    } else {
      localStorage.removeItem('displayName');
    }
  }, [displayName]);

  const loginFn = (newToken: string, newUserId: string, newDisplayName: string) => {
    setToken(newToken);
    setUserId(newUserId);
    setDisplayName(newDisplayName);
  };

  const logout = () => {
    setToken(null);
    setUserId(null);
    setDisplayName(null);
  };

  return (
    <AuthContext.Provider value={{ token, userId, displayName, isAuthenticated: !!token, login: loginFn, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}