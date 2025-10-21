import React from 'react';
import Header from './Header';
import { Container } from 'react-bootstrap';

const Layout = ({ children }) => {
  return (
    <div className='d-flex flex-column min-vh-100'>
      <Header />
      <main className='flex-grow-1'>
        {children}
      </main>
      <footer className="bg-dark text-white py-4">
        <Container expand="lg" className="text-center">
        <p>© 2024 WorkShifts. All rights reserved.</p>
        </Container>
      </footer>
    </div>
  );
};

export default Layout;