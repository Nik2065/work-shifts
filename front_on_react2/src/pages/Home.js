import React from 'react';
import { Container,  Button } from 'react-bootstrap';
import { Link } from 'react-router-dom';


const Home = () => {


  return (
    <Container className="mt-4">
      <div>
        <h1>WorkShifts</h1>
        <>Приложение для учета смен</><br/><br/>
          <Button as={Link} to="/dashboard" variant="primary">Войти</Button>
      </div>
    </Container>
  );
};

export default Home;