import React, { useContext, useState } from "react";
import {  useNavigate } from "react-router-dom";
import { Container, Row, Col, Card, Form, Button, Alert } from "react-bootstrap";
import { AuthContext } from "../App";
import { Calendar,  Mail } from 'lucide-react';
//import {Login} from '../services/auth-api';

export function LoginPage() {

  const navigate = useNavigate();
  const { login } = useContext(AuthContext);

  const [form, setForm] = useState({ email: '', password: '' });
  const [error, setError] = useState('');
  
    const handleSubmit = async (e) => {
    e.preventDefault();
    const {email, password } = form;
    const success = await login({ email, password });
    if (!success) {
      alert('Login failed!');
    }
    else {
      //переходим на внутрении страницы
      //navigate('/main')
      navigate('/')
    }
  };

  /*const { login } = useContext(AuthContext);
  const [form, setForm] = useState({ email: '', password: '' });
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    const result = await login(form.email, form.password);
    if (!result.success) {
      setError(result.error);
    }
    else {
      //переходим на внутрении страницы
    }
  };*/

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
                    {
                    //<Lock size={18} className="position-absolute top-50 start-0 translate-middle-y ms-3 text-muted" />
                    }
                    
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
  )
}




// Страница логина
export function LoginPage3() {
  //const { login } = useContext(AuthContext);
  const [form, setForm] = useState({ email: '', password: '' });
  const [error, setError] = useState('');

  /*
  const handleSubmit = async (e) => {
    e.preventDefault();
    const result = await login(form.email, form.password);
    if (!result.success) {
      setError(result.error);
    }
  };*/

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

              <Form onSubmit={()=>{}}>
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

