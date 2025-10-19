import React from 'react';
import { Container, Jumbotron, Button } from 'react-bootstrap';
import { Link } from 'react-router-dom';

const Home = () => {
  return (
    <Container className="mt-4">
      <div>
        <h1>Добро пожаловать!</h1>
        <p>
          Это демонстрационное приложение с JWT авторизацией и защитой маршрутов.
        </p>
        <p>
          <Button as={Link} to="/public" variant="primary" className="me-2">
            Публичная страница
          </Button>
          <Button as={Link} to="/dashboard" variant="success">
            Защищенный дашборд
          </Button>
        </p>
      </div>
    </Container>
  );
};

export default Home;