import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import Layout from './components/layout/Layout';
import ProtectedRoute from './components/auth/ProtectedRoute';
import LoginForm from './components/auth/LoginForm';

// Pages
import Home from './pages/Home';
import PublicPage from './pages/PublicPage';
import Dashboard from './components/dashboard/Dashboard';
import Profile from './pages/Profile';



// Bootstrap CSS
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap-icons/font/bootstrap-icons.css';
import { DashboardPage } from './pages/DashboardPage';
import DashboardLayout from './pages/DashboardLayout.jsx';


function App() {
  return (
    <AuthProvider>
      <Router>
        <Layout>
          <Routes>
            {/* Public routes */}
            <Route path="/" element={<Home />} />
            <Route path="/public" element={<PublicPage />} />
            <Route path="/login" element={<LoginForm />} />
            
            {/* Protected routes */}
            <Route 
              path="/dashboard" 
              element={
                <ProtectedRoute>
                  <Dashboard />
                </ProtectedRoute>
              }
            />

            <Route 
              path="/dashboard2" 
              element={
                <ProtectedRoute>
                  <DashboardPage />
                </ProtectedRoute>
              }
            />

            <Route 
              path="/dashboard3" 
              element={
                <ProtectedRoute>
                  <DashboardLayout />
                </ProtectedRoute>
              }
            />


            <Route 
              path="/profile" 
              element={
                <ProtectedRoute>
                  <Profile />
                </ProtectedRoute>
              } 
            />
            
            {/* 404 fallback */}
            <Route path="*" element={<Home />} />
          </Routes>
        </Layout>
      </Router>
    </AuthProvider>
  );
}

export default App;