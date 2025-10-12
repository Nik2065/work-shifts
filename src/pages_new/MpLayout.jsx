import React from 'react';
import { 
  Container, 
  Row, 
  Col, 
  Nav, 
  Navbar, 
  Card, 
  Button, 
  Badge,
  Dropdown
} from 'react-bootstrap';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap-icons/font/bootstrap-icons.css';

import '../dashboard.css';
import {MpFooter} from './MpFooter.jsx';


export function MpLayout ({ children }) {
  return (
    <div className="d-flex flex-column min-vh-100">
      {/* Верхняя навигационная панель */}
      <Navbar bg="white" expand="lg" className="border-bottom shadow-sm">
        <Container fluid>
          <Navbar.Brand href="#" className="d-flex align-items-center">
            <i className="bi bi-grid-3x3-gap-fill text-primary me-2"></i>
            <span className="fw-bold">Учет рабочих смен</span>
          </Navbar.Brand>
          
          <Navbar.Toggle aria-controls="navbar-nav" />
          
          <Navbar.Collapse id="navbar-nav" className="justify-content-end">
            <Nav className="align-items-center">
             
              <Nav.Link href="#" className="position-relative mx-3" >
                <i  className="bi bi-envelope fs-2"></i>
                <Badge bg="success" className="position-absolute top-40 start-250 translate-middle p-1">
                  5
                </Badge>
              </Nav.Link>
              
              <Dropdown align="end">
                <Dropdown.Toggle variant="light" id="dropdown-user" className="d-flex align-items-center border-0">
                  <div className="me-2 text-end d-none d-sm-block">
                    <div className="fw-semibold">Анна Петрова</div>
                    <small className="text-muted">Администратор</small>
                  </div>
                  <div className="bg-primary text-white rounded-circle d-flex align-items-center justify-content-center" 
                       style={{width: '40px', height: '40px'}}>
                    <i className="bi bi-person-fill"></i>
                  </div>
                </Dropdown.Toggle>

                <Dropdown.Menu>
                  <Dropdown.Item href="#">
                    <i className="bi bi-person me-2"></i>Профиль
                  </Dropdown.Item>
                  <Dropdown.Item href="#">
                    <i className="bi bi-gear me-2"></i>Настройки
                  </Dropdown.Item>
                  <Dropdown.Divider />
                  <Dropdown.Item href="#">
                    <i className="bi bi-box-arrow-right me-2"></i>Выйти
                  </Dropdown.Item>
                </Dropdown.Menu>
              </Dropdown>
            </Nav>
          </Navbar.Collapse>
        </Container>
      </Navbar>

      {/* Основной контент с меню */}
      <Container fluid className="flex-grow-1">
        <Row className="h-100">
          {/* Левое фиксированное меню */}
          <Col xs={3} lg={2} className="bg-dark text-white p-0">
            <div className="d-flex flex-column h-100">
              {/* Заголовок меню */}
              <div className="p-3 border-bottom border-secondary">
                <h6 className="mb-0 text-uppercase text-muted">Основное меню</h6>
              </div>

              {/* Навигация */}
              <Nav className="flex-column p-3 flex-grow-1">
                <Nav.Link href="/main2" className="text-white mb-2 nav-item active">
                  <div className="d-flex align-items-center">
                    <i className="bi bi-speedometer2 me-3"></i>
                    <span>Дашборд</span>
                  </div>
                </Nav.Link>
                
                <Nav.Link href="/siteusers" className="text-white mb-2 nav-item">
                  <div className="d-flex align-items-center">
                    <i className="bi bi-people me-3"></i>
                    <span>Пользователи сайта</span>
                  </div>
                </Nav.Link>
                
                <Nav.Link href="/employees" className="text-white mb-2 nav-item">
                  <div className="d-flex align-items-center">
                    <i className="bi bi-folder me-3"></i>
                    <span>Сотрудники</span>
                  </div>
                </Nav.Link>
                
                <Nav.Link href="#" className="text-white mb-2 nav-item">
                  <div className="d-flex align-items-center">
                    <i className="bi bi-bar-chart me-3"></i>
                    <span>Аналитика</span>
                  </div>
                </Nav.Link>
                
                <Nav.Link href="#" className="text-white mb-2 nav-item">
                  <div className="d-flex align-items-center">
                    <i className="bi bi-calendar-event me-3"></i>
                    <span>Календарь</span>
                  </div>
                </Nav.Link>
                
                <Nav.Link href="#" className="text-white mb-2 nav-item">
                  <div className="d-flex align-items-center">
                    <i className="bi bi-chat-dots me-3"></i>
                    <span>Сообщения</span>
                    <Badge bg="primary" className="ms-auto">5</Badge>
                  </div>
                </Nav.Link>
                
                <Nav.Link href="#" className="text-white mb-2 nav-item">
                  <div className="d-flex align-items-center">
                    <i className="bi bi-file-earmark-text me-3"></i>
                    <span>Документы</span>
                  </div>
                </Nav.Link>
              </Nav>

              {/* Дополнительное меню */}
              <div className="p-3 border-top border-secondary">
                <h6 className="mb-2 text-uppercase text-muted">Система</h6>
                <Nav className="flex-column">
                  <Nav.Link href="#" className="text-white mb-2 nav-item">
                    <div className="d-flex align-items-center">
                      <i className="bi bi-gear me-3"></i>
                      <span>Настройки</span>
                    </div>
                  </Nav.Link>
                  <Nav.Link href="#" className="text-white nav-item">
                    <div className="d-flex align-items-center">
                      <i className="bi bi-question-circle me-3"></i>
                      <span>Помощь</span>
                    </div>
                  </Nav.Link>
                </Nav>
              </div>
            </div>
          </Col>

          {/* Основной контент */}
          <Col xs={9} lg={10} className="bg-light p-4">

            
              {children}
            


          </Col>
        </Row>
      </Container>

      {/* Нижняя панель */}
      <MpFooter />
    </div>
  );
};
