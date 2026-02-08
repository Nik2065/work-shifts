import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/auth/ProtectedRoute';
import LoginForm from './components/auth/LoginForm';
import Layout from './components/layout/Layout.js'


// Bootstrap CSS
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap-icons/font/bootstrap-icons.css';

// Pages
import PublicPage from './pages/PublicPage';
import Home from './pages/Home';
import PageNotFound from './pages/PageNotFound.js';
import { DashboardPage } from './pages/DashboardPage';
//import DashboardLayout from './pages/DashboardLayout.jsx';
import { SiteUsersPage } from './pages/SiteUsersPage.jsx';
import { ReportForEmployeePage } from './pages/ReportForEmployeePage.jsx';
import { ReportForEmployesListPage } from './pages/ReportForEmployesListPage.jsx';
import { EmployeesPage } from './pages/EmployeesPage.jsx';
import { PayoutReportPage } from './pages/PayoutReportPage.jsx';


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
            

            <Route 
              path="/dashboard" 
              element={
                <ProtectedRoute>
                  <DashboardPage />
                </ProtectedRoute>
              }
            />

            <Route 
              path="/employees" 
              element={
                <ProtectedRoute>
                  <EmployeesPage />
                </ProtectedRoute>
              }
            />

              {/*
            <Route 
              path="/profile" 
              element={
                <ProtectedRoute>
                  <ProfilePage />
                </ProtectedRoute>
              } 
            />
            */}

            <Route 
              path="/users" 
              element={
                <ProtectedRoute>
                  <SiteUsersPage />
                </ProtectedRoute>
              } 
            />

            <Route 
              path="/reportForEmployee" 
              element={
                <ProtectedRoute>
                  <ReportForEmployeePage />
                </ProtectedRoute>
              } 
            />

            <Route 
              path="/reportForEmployesList"
              element={
                <ProtectedRoute>
                  <ReportForEmployesListPage />
                </ProtectedRoute>
              }
            />

            <Route 
              path="/reportForPayout"
              element={
                <ProtectedRoute>
                  <PayoutReportPage />
                </ProtectedRoute>
              }
            />

            {/* 404 fallback */}
            <Route path="*" element={<PageNotFound />} />
          </Routes>
        </Layout>
      </Router>
    </AuthProvider>
  );
}

export default App;