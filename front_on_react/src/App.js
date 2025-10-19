import React, { useState, createContext, useContext, useEffect } from 'react';
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
import { FirstPage } from './pages/FirstPage.jsx';
//import {Login} from './services/auth-api.js';

// Контекст аутентификации
export const AuthContext = createContext();

export const useAuth = () => {
  return useContext(AuthContext);
};

const App = () => {
  
  const [token, setToken] = useState(null);
  const [user, setUser] = useState(null);
  //const [auth, setAuth] = useState(null);
  const [loading, setLoading] = useState(true);

  const [auth, setAuth] = useState({
    token: localStorage.getItem('token'),
    user: JSON.parse(localStorage.getItem('user') || 'null')
  });


  // Проверяем наличие токена при загрузке приложения
  useEffect(() => {
    const storedToken = localStorage.getItem('token');
    //console.log(storedToken);

    if (storedToken) {
      setToken(storedToken);
      // Декодируем токен чтобы получить данные пользователя (без проверки подписи)
      const userData = JSON.parse(atob(storedToken.split('.')[1]));
      setUser(userData);
    }
    setLoading(false);
  }, []);

 

  const logout = () => {
    // Удаляем токен и данные пользователя
    localStorage.removeItem('token');
    setToken(null);
    setUser(null);
  };

  const value = {
    token,
    user,
    login,
    logout,
    isAuthenticated: !!token,
  };




  const login = async (email, password) => {
    // Mock аутентификация
    if (email === 'admin@company.com' && password === 'password123') {
      //const token = 'mock-jwt-token-' + Date.now();
      const token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbkBjb21wYW55LmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6ImFkbWluIiwianRpIjoiYTk3NWM1YjItN2UwNS00MjcyLWI3YjUtMjM3YjZjNzE5NDMyIiwiZXhwIjoxNzYwODc3NjAxLCJpc3MiOiJteWFwaS5jb20iLCJhdWQiOiJteWFwaS51c2VycyJ9.iRQw0r4_s8r-owSakai9iwl0QKJbd9Q1WP0zaZTg5Oc";

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

  return (
    //<AuthContext.Provider value={""} >
    <AuthContext.Provider value={{ ...auth, login, logout }}>
      <Router>
        <Routes>
          <Route path="/" element={<FirstPage />} />
        
          <Route path="/login" element={<LoginPage />} />
          {
          //<Route path="/main" element={<MainPage />} />
          //<Route path="/main2" element={<MainPage2 />} />
          }
          <Route path="/siteusers" element={<MpSiteUsersPage />} />
          <Route path="/employees" element={<MpEmploeesPage />} />


        <Route path="/main" element={
        <ProtectedRoute>
              <DashboardLayout>
              </DashboardLayout>
        </ProtectedRoute>
                }>
              
        </Route>

          <Route path="/" element={
            <ProtectedRoute>
              <DashboardLayout>
              </DashboardLayout>

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