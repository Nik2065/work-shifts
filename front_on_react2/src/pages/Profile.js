import React from 'react';
import { Container, Card, Alert } from 'react-bootstrap';
import { useAuth } from '../context/AuthContext';

const Profile = () => {
  const { user } = useAuth();

  return (
    <Container className="mt-4">
      <h1>Профиль пользователя</h1>
      
      <Card>
        <Card.Header>
          <h4>Личная информация</h4>
        </Card.Header>
        <Card.Body>
          <Alert variant="info">
            Это защищенная страница профиля. Только авторизованные пользователи могут ее просматривать.
          </Alert>
          
          <div className="mb-3">
            <strong>ID пользователя:</strong> {user?.id}
          </div>
          <div className="mb-3">
            <strong>Имя пользователя:</strong> {user?.username}
          </div>
          <div className="mb-3">
            <strong>Email:</strong> {user?.email}
          </div>
          <div className="mb-3">
            <strong>Роль:</strong> {user?.role}
          </div>
        </Card.Body>
      </Card>
    </Container>
  );
};

export default Profile;