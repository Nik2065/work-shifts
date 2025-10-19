import React from 'react';
import { Container, Card, Row, Col, Alert } from 'react-bootstrap';
import { useAuth } from '../../context/AuthContext';

const Dashboard = () => {
  const { user } = useAuth();

  return (
    <Container className="mt-4">
      <h1>Панель управления</h1>
      
      <Alert variant="success" className="mb-4">
        Добро пожаловать в защищенную зону приложения!
      </Alert>

      <Row>
        <Col md={6}>
          <Card className="mb-3">
            <Card.Header>Информация о пользователе</Card.Header>
            <Card.Body>
              <p><strong>ID:</strong> {user?.id}</p>
              <p><strong>Имя пользователя:</strong> {user?.username}</p>
              <p><strong>Email:</strong> {user?.email}</p>
              <p><strong>Роль:</strong> {user?.role}</p>
            </Card.Body>
          </Card>
        </Col>
        
        <Col md={6}>
          <Card className="mb-3">
            <Card.Header>Статистика</Card.Header>
            <Card.Body>
              <p>Здесь может быть ваша статистика и другая важная информация...</p>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default Dashboard;