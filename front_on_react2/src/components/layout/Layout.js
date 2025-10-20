import React from 'react';
import Header from './Header';
import { Container } from 'react-bootstrap';

const Layout = ({ children }) => {
  return (
    <>
      <Header />
      <main className='min-vh-100 '>
        {children}
      </main>
      <footer className="bg-dark text-white py-4">
        <Container expand="lg" className="py-4 text-center">
        <p>© 2024 WorkShifts. All rights reserved.</p>
        </Container>
      </footer>
    </>
  );
};

export default Layout;