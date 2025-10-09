import React, { useState, createContext, useContext, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { 
  Container, 
  Row, 
  Col, 
  Card, 
  Button, 
  Form, 
  Navbar, 
  Nav,
  Table,
  Badge,
  Modal,
  Alert
} from 'react-bootstrap';
import { 
  Calendar, 
  User, 
  Clock, 
  Play, 
  Pause, 
  Plus, 
  Edit3, 
  Trash2, 
  LogOut, 
  Users, 
  Building2, 
  CalendarDays, 
  CheckCircle, 
  XCircle, 
  Lock, 
  Mail,
  ChevronLeft,
  ChevronRight
} from 'lucide-react';

// Контекст для аутентификации
const AuthContext = createContext();

import { DashboardPage } from './Pages/DashboardPage';
import { EmployeesPage } from './Pages/EmployeesPage';


// Mock JWT токен и API
const mockAuthAPI = {
  login: async (email, password) => {
    if (email === 'admin@company.com' && password === 'password123') {
      const token = 'mock-jwt-token-' + Date.now();
      localStorage.setItem('token', token);
      localStorage.setItem('user', JSON.stringify({
        name: 'Администратор',
        email: email,
        role: 'admin'
      }));
      return { success: true, token };
    }
    return { success: false, error: 'Неверный email или пароль' };
  },
  
  validateToken: (token) => {
    return token && token.startsWith('mock-jwt-token-');
  }
};

// Главный компонент приложения
const TimeTrackingApp = () => {
  const [auth, setAuth] = useState({
    token: localStorage.getItem('token'),
    user: JSON.parse(localStorage.getItem('user') || 'null')
  });

  const login = async (email, password) => {
    const result = await mockAuthAPI.login(email, password);
    if (result.success) {
      setAuth({
        token: result.token,
        user: JSON.parse(localStorage.getItem('user'))
      });
      return { success: true };
    }
    return { success: false, error: result.error };
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
          <Route path="/" element={
            <ProtectedRoute>
              <DashboardLayout />
            </ProtectedRoute>
          }>
            <Route index element={<DashboardPage />} />
            <Route path="employees" element={<EmployeesPage />} />
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

// Страница логина
const LoginPage = () => {
  const { login } = useContext(AuthContext);
  const [form, setForm] = useState({ email: '', password: '' });
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    const result = await login(form.email, form.password);
    if (!result.success) {
      setError(result.error);
    }
  };

  return (
    <Container fluid className="min-vh-100 bg-light d-flex align-items-center justify-content-center">
      <Row className="w-100 justify-content-center">
        <Col xs={12} sm={8} md={6} lg={4}>
          <Card className="border-0 shadow">
            <Card.Body className="p-4">
              <div className="text-center mb-4">
                <div className="bg-primary rounded-circle d-inline-flex align-items-center justify-content-center mb-3" 
                     style={{ width: '64px', height: '64px' }}>
                  <Calendar size={32} className="text-white" />
                </div>
                <h4 className="fw-bold">Учет Рабочего Времени</h4>
                <p className="text-muted">Войдите в систему для продолжения</p>
              </div>

              <Form onSubmit={handleSubmit}>
                <Form.Group className="mb-3">
                  <Form.Label>Email</Form.Label>
                  <div className="position-relative">
                    <Mail size={18} className="position-absolute top-50 start-0 translate-middle-y ms-3 text-muted" />
                    <Form.Control
                      type="email"
                      value={form.email}
                      onChange={(e) => setForm({ ...form, email: e.target.value })}
                      placeholder="Введите ваш email"
                      className="ps-5"
                      required
                    />
                  </div>
                </Form.Group>

                <Form.Group className="mb-4">
                  <Form.Label>Пароль</Form.Label>
                  <div className="position-relative">
                    <Lock size={18} className="position-absolute top-50 start-0 translate-middle-y ms-3 text-muted" />
                    <Form.Control
                      type="password"
                      value={form.password}
                      onChange={(e) => setForm({ ...form, password: e.target.value })}
                      placeholder="Введите ваш пароль"
                      className="ps-5"
                      required
                    />
                  </div>
                </Form.Group>

                {error && (
                  <Alert variant="danger" className="small">
                    {error}
                  </Alert>
                )}

                <Button variant="primary" type="submit" className="w-100 py-2">
                  Войти в систему
                </Button>
              </Form>

              <div className="text-center mt-4">
                <small className="text-muted">Демо учетные данные:</small>
                <div>
                  <small className="fw-medium">Email: admin@company.com</small>
                </div>
                <div>
                  <small className="fw-medium">Пароль: password123</small>
                </div>
              </div>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

// Layout для авторизованных страниц
const DashboardLayout = () => {
  const { user, logout } = useContext(AuthContext);
  
  return (
    <div className="min-vh-100 bg-light">
      <Navbar bg="white" expand="lg" className="border-bottom shadow-sm">
        <Container fluid>
          <Navbar.Brand className="d-flex align-items-center">
            <div className="bg-primary rounded p-2 me-3">
              <Calendar className="text-white" size={24} />
            </div>
            <div>
              <h5 className="mb-0 fw-bold">Учет Рабочего Времени</h5>
              <small className="text-muted">Отслеживание времени работы сотрудников по датам</small>
            </div>
          </Navbar.Brand>
          
          <Navbar.Toggle />
          <Navbar.Collapse className="justify-content-end">
            <Nav className="align-items-center">
              <Nav.Link as={Button} variant="secondary" className="me-3" href="/employees">
                <Users size={16} className="me-2" />
                Список сотрудников
              </Nav.Link>
              
              <div className="d-flex align-items-center bg-light rounded px-3 py-2">
                <User size={20} className="text-muted me-2" />
                <div className="text-end">
                  <div className="small fw-medium">{user?.name}</div>
                  <div className="small text-muted">{user?.email}</div>
                </div>
                <Button 
                  variant="link" 
                  className="text-muted ms-2 p-1" 
                  onClick={logout}
                >
                  <LogOut size={18} />
                </Button>
              </div>
            </Nav>
          </Navbar.Collapse>
        </Container>
      </Navbar>

      <Container fluid className="py-4">
        <Routes>
          <Route index element={<DashboardPage />} />
          <Route path="employees" element={<EmployeesPage />} />
        </Routes>
      </Container>
    </div>
  );
};





export default TimeTrackingApp;