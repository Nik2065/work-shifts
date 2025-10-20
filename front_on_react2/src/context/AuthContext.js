import React, { useState, useEffect, createContext, useContext } from 'react';
import { jwtDecode } from 'jwt-decode';
import {authService} from '../services/authService';

const AuthContext = createContext();

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export const AuthProvider = ({ children }) => {
  const [token, setToken] = useState(localStorage.getItem('token'));
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    initializeAuth();
  }, []);

  const initializeAuth = () => {
    const savedToken = localStorage.getItem('token');
    if (savedToken) {
      try {
        const decoded = jwtDecode(savedToken);
        
        // Проверяем не истек ли токен
        if (decoded.exp * 1000 > Date.now()) {
          setToken(savedToken);
          setUser(decoded);
        } else {
          logout();
        }
      } catch (error) {
        console.error('Invalid token:', error);
        logout();
      }
    }
    setLoading(false);
  };

  const login = async (credentials) => {

    //console.log("credentials", credentials );
    
    try {
      // Имитация API запроса
      //const response = await mockLogin(credentials);
      const response = await authService.login(credentials);
      
      localStorage.setItem('token', response.token);
      const decoded = jwtDecode(response.token);
      
      setToken(response.token);
      console.log("decoded", decoded);
      setUser(decoded);
      
      return { success: true };
    } catch (error) {
      return { success: false, error: error.message };
    }
  };

  const logout = () => {
    localStorage.removeItem('token');
    setToken(null);
    setUser(null);
  };

  const value = {
    token,
    user,
    login,
    logout,
    loading,
    isAuthenticated: !!token,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};


const loginFromApi = (credentials) => {
    return authService.login(credentials);
}

// Имитация сервиса авторизации
const mockLogin = (credentials) => {
  return new Promise((resolve, reject) => {
    setTimeout(() => {
      if (credentials.username === 'admin' && credentials.password === 'password') {
        // Генерируем mock JWT токен
        const mockUser = {
          id: 1,
          username: credentials.username,
          email: 'admin@example.com',
          role: 'admin'
        };
        
        // Создаем правильную структуру JWT
        const header = {
          alg: 'HS256',
          typ: 'JWT'
        };
        const payload = {
          ...mockUser,
          exp: Math.floor(Date.now() / 1000) + (60 * 60), // 1 час
          iat: Math.floor(Date.now() / 1000)
        };

        // Кодируем header и payload в base64
        const encodedHeader = btoa(JSON.stringify(header))
          .replace(/=/g, '')
          .replace(/\+/g, '-')
          .replace(/\//g, '_');
        
        const encodedPayload = btoa(JSON.stringify(payload))
          .replace(/=/g, '')
          .replace(/\+/g, '-')
          .replace(/\//g, '_');
        
        // Создаем mock подпись
        const signature = 'mock_signature_12345';
        const encodedSignature = btoa(signature)
          .replace(/=/g, '')
          .replace(/\+/g, '-')
          .replace(/\//g, '_');
        
        // Собираем полный токен
        const token = `${encodedHeader}.${encodedPayload}.${encodedSignature}`;


        console.log("token=", token);
        
        resolve({ token });
      } else {
        reject(new Error('Неверное имя пользователя или пароль'));
      }
    }, 1000);
  });
};