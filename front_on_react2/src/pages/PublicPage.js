import React from 'react';
import { Container, Alert } from 'react-bootstrap';

const PublicPage = () => {
  return (
    <Container className="mt-4">
      <h1>Публичная страница</h1>
      <Alert variant="info">
        Эта страница доступна всем пользователям без авторизации.
      </Alert>
      <p>
        Здесь может быть публичный контент, информация о компании, 
        контакты и другая общедоступная информация.
      </p>
    </Container>
  );
};

export default PublicPage;