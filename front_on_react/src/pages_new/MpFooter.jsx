import React from 'react';
import { 
  Container, 
  Row, 
  Col, 
  Nav, 

} from 'react-bootstrap';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap-icons/font/bootstrap-icons.css';

export function MpFooter (){
    
    
    return (
      <footer className="bg-dark text-white py-3 mt-auto">
        <Container fluid>
          <Row className="align-items-center">
            <Col md={6}>
              <div className="d-flex align-items-center">
                <i className="bi bi-grid-3x3-gap-fill text-primary me-2"></i>
                <span className="fw-semibold">AdminPanel</span>
                <small className="text-muted ms-3">&copy; 2024 Все права защищены</small>
              </div>
            </Col>
            <Col md={6} className="text-md-end">
              <Nav className="justify-content-end">
                <Nav.Link href="#" className="text-white-50">Политика конфиденциальности</Nav.Link>
                <Nav.Link href="#" className="text-white-50">Условия использования</Nav.Link>
                <Nav.Link href="#" className="text-white-50">Поддержка</Nav.Link>
              </Nav>
            </Col>
          </Row>
        </Container>
      </footer>
    )


}