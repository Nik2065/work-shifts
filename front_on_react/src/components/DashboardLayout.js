import React, { useContext } from 'react';
import { Outlet,  } from 'react-router-dom';
import { Container, Navbar, Nav, Button } from 'react-bootstrap';
import { Calendar, User, LogOut, Users } from 'lucide-react';
import { AuthContext } from '../App';
import { LinkContainer } from 'react-router-bootstrap';



const DashboardLayout = () => {
  const { user, logout } = useContext(AuthContext);
  
  return (
    <div className="min-vh-100 bg-light">
      <Navbar bg="white" expand="lg" className="border-bottom shadow-sm">
        <Container fluid>
          
          <LinkContainer to="/">
          <Navbar.Brand className="d-flex align-items-center">
            <div className="bg-primary rounded p-2 me-3">
              <Calendar className="text-white" size={24} />
            </div>
            <div>
              <h5 className="mb-0 fw-bold">Учет рабочего времени</h5>
              <small className="text-muted">Отслеживание времени работы сотрудников по датам</small>
            </div>
          </Navbar.Brand>
          </LinkContainer>
          
          <Navbar.Toggle />
          <Navbar.Collapse className="justify-content-end">
            <Nav className="align-items-center">
              <Button 
                variant="outline-secondary" 
                className="me-3"
                href="/employees"
              >
                <Users size={16} className="me-2" />
                Список сотрудников
              </Button>
              
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
        <Outlet />
      </Container>
    </div>
  );
};

export default DashboardLayout;