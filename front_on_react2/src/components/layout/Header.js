import React from 'react';
import { Navbar, Nav, Button, Container } from 'react-bootstrap';
import { useAuth } from '../../context/AuthContext';
import { Link, useNavigate } from 'react-router-dom';
import { Calendar, User, LogOut, Users } from 'lucide-react';

const Header = () => {
  const { user, logout, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  return (
    <Navbar bg="dark" variant="dark" expand="lg">
      <Container>
        <Navbar.Brand as={Link} to="/">
          <i style={{color:"white", fontSize:"1.8rem"}} className="bi bi-calendar3"></i>
        </Navbar.Brand>
        
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Navbar.Collapse id="basic-navbar-nav">
          <Nav className="me-auto">
            <Nav.Link as={Link} to="/">WorkShifts</Nav.Link>
            {
            //<Nav.Link as={Link} to="/public">Описание</Nav.Link>
            }
            {isAuthenticated && (
              <>
                <Nav.Link as={Link} to="/dashboard">Учет времени</Nav.Link>
                <Nav.Link as={Link} to="/users">Пользователи сайта</Nav.Link>
                {
                //<Nav.Link as={Link} to="/profile">Профиль</Nav.Link>
                }
              </>
            )}
          </Nav>
          
          <Nav>
            {/*    isAuthenticated ? (
              <>
                <Navbar.Text className="me-3">
                  <i style={{color:"white", fontSize:"1.6rem"}} className="bi bi-person-circle"></i>
                   &nbsp;&nbsp;
                   {user?.Login} |
                   &nbsp;
                   {user?.Role === "Admin" && "Администратор"}
                </Navbar.Text>
                <Button variant="outline-light" onClick={handleLogout}>
                  Выйти
                </Button>
              </>
            ) : (
              <Button 
                variant="outline-light" 
                as={Link} 
                to="/login"
              >
                Войти
              </Button>
            )    */ }
            {isAuthenticated ? (
                          <div className="d-flex align-items-center bg-light rounded px-3 py-2">
                            <User size={20} className="text-muted me-2" />
                            <div className="text-end">
                              <div className="small fw-medium">{user?.Login}</div>
                              <div className="small text-muted">{user?.Role === "Admin" && "Администратор"}</div>
                            </div>
                            <Button 
                              onClick={handleLogout}
                              variant="link" 
                              className="text-muted ms-2 p-1" 
                              
                            >
                              <LogOut size={18} />
                            </Button>
                          </div>
                          ) : (
                <Button 
                variant="outline-light" 
                as={Link} 
                to="/login"
              >
                Войти
              </Button>
              ) }
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
};

export default Header;