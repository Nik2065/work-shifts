import React, { useState, createContext, useContext } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import DashboardPageOld from './pages/DashboardPageOld.jsx';
import DashboardPage from './pages/DashboardPage.jsx';
import EmployeesPage from './pages/EmployeesPage.jsx';
import {LoginPage} from './pages/LoginPage.jsx';
import DashboardLayout from './components/DashboardLayout.js';
import { MainPage } from './pages/MainPage.jsx';
import { MainPage2 } from './pages_new/MainPage2.jsx';
import { MpSiteUsersPage } from './pages_new/MpSiteUsersPage.jsx';
import {MpEmploeesPage} from './pages_new/MpEmploeesPage.jsx';


// Контекст аутентификации
export const AuthContext = createContext();

const App = () => {
  const [auth, setAuth] = useState({
    token: localStorage.getItem('token'),
    user: JSON.parse(localStorage.getItem('user') || 'null')
  });

  const login = async (email, password) => {
    // Mock аутентификация
    if (email === 'admin@company.com' && password === 'password123') {
      const token = 'mock-jwt-token-' + Date.now();
      localStorage.setItem('token', token);
      localStorage.setItem('user', JSON.stringify({
        name: 'Администратор',
        email: email,
        role: 'admin'
      }));
      setAuth({
        token: token,
        user: { name: 'Администратор', email: email, role: 'admin' }
      });
      return { success: true };
    }
    return { success: false, error: 'Неверный email или пароль' };
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setAuth({ token: null, user: null });
  };

  return (
    <AuthContext.Provider value={{ ...auth, login, logout }}>
      <Router>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/main" element={<MainPage />} />
          <Route path="/main2" element={<MainPage2 />} />
          <Route path="/siteusers" element={<MpSiteUsersPage />} />
          <Route path="/employees" element={<MpEmploeesPage />} />

          <Route path="/" element={
            <ProtectedRoute>
              <DashboardLayout />
            </ProtectedRoute>
            }>
            
            <Route index element={<DashboardPage />} />
            <Route path="employees2" element={<EmployeesPage />} />
          </Route>
           
        </Routes>
      </Router>
    </AuthContext.Provider>
  );
};

// Защищенный маршрут
const ProtectedRoute = ({ children }) => {
  const { token } = useContext(AuthContext);
  return token ? children : <Navigate to="/login" replace />;
};

export default App;