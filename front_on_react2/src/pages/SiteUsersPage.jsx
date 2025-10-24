import React, {useState, useEffect} from "react";
import { 
  Container, Row, Col, Nav, 
  Navbar, Card, Button, Badge,
  Dropdown, Form
} from 'react-bootstrap';

import {GetSiteUsersList } from '../services/apiService';


export function SiteUsersPage() {



    const [siteUsersList, setSiteUsersList] = useState([]);

    useEffect(() => {
        GetSiteUsersList()
        .then((response) => response.json())
        .then((data) => {
            console.log(data);
            if(data.isSuccess){
                setSiteUsersList(data.users);
                }
            })
        .catch((error) => {
                console.error('Ошибка:', error);
            });
        }    
       , []);
            
      

    return (
        <Container expand="lg">
        <br/>
        {/* Заголовок страницы */}
            <div className="d-flex justify-content-between align-items-center mb-4">
              <div>
                <h2 className="mb-1">Пользователи сайта</h2>
                <p className="text-muted mb-0"></p>
              </div>
              <Button onClick={()=> {setEmployeeId(null); setShowEmpModal(true);}} variant="primary" className="d-flex align-items-center">
                <i className="bi bi-plus-circle me-2"></i>
                Добавить пользователя
              </Button>
            </div>
            <Card className=" h-100">
                <Card.Header className="bg-white border-0">
                    <Row>
                      <Col sm={4}> 
                      <Form.Control type='text' placeholder='Поиск по ФИО' />
                      </Col>
                      <Col sm={1}>
                        <Button sm={1} variant="outline-secondary" className="d-flex align-items-center">Поиск</Button>
                      </Col>
                    </Row>
                </Card.Header>
                <Card.Body>
                    <div className="table-responsive">
                        {
                    siteUsersList && siteUsersList.length > 0 ? 
                      <table className="table table-bordered table-hover">
                        <thead>
                            <tr>
                            <th width="10%">ID</th>
                            <th width="20%">Логин</th>
                            <th width="20%">Объект</th>
                            <th width="20%" >Роль</th>
                            <th width="20%" >Дата добавления</th>
                          </tr>
                          </thead>
                          <tbody>
                           {
                           siteUsersList.map((user, index) =>  (
                           <tr key={user.id}>
                            <td>{user.id}</td>
                            <td>{user.login}</td>
                            <td>-</td>
                            <td>{user.roleName}</td>
                            <td>{user.created}</td>
                           </tr>))
                           
                           }
                          </tbody>
                    </table>
                          :
                          <div className="text-center">
                          <h4>Пользователи не найдены</h4>
                          </div>
                        }
                    </div>
                </Card.Body>
                        
            </Card>
        </Container>
    );
}