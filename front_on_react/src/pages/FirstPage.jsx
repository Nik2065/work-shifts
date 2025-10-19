import React from 'react';
import { Container, Row, Col, Card, Form, Button, Alert } from "react-bootstrap";
import { Calendar,  Mail } from 'lucide-react';
import {useNavigate } from 'react-router-dom';

export function FirstPage() {

    const navigate = useNavigate();


    return (
    <Container fluid className="min-vh-100 bg-light d-flex align-items-center justify-content-center">
      <Row className="w-100 justify-content-center">
         <Col xs={12} sm={8} md={6} lg={6}>
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



                <Button onClick={() => navigate('/login')} variant="primary"  className="w-100 py-2">
                  Войти в систему
                </Button>
                

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