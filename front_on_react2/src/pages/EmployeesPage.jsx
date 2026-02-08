import React ,{ useState, useEffect} from 'react';
import { 
  Container, Row, Col, Toast, Table, ToastContainer ,
   Card, Button, 
  Form, Spinner
} from 'react-bootstrap';






export function EmployeesPage () {

    return (
        <Container expand="lg">
                    <br/>
                {/* Заголовок страницы */}
                    <div className="d-flex justify-content-between align-items-center mb-4">
                      <div>
                        <h2 className="mb-1">Список сотрудников</h2>
                        <p className="text-muted mb-0"></p>
                      </div>
                      { /*
                      <Button onClick={()=> {setEmployeeId(null); setShowEmpModal(true);}} variant="primary" className="d-flex align-items-center">
                        <i className="bi bi-plus-circle me-2"></i>
                        Добавить сотрудника
                      </Button>
                      */ }
                    </div>
        </Container>
    )

}