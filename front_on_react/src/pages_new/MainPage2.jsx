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
import { MpLayout } from './MpLayout';


export function MainPage2 () {
  return (
    <MpLayout>

            {/* Заголовок страницы */}
            <div className="d-flex justify-content-between align-items-center mb-4">
              <div>
                <h2 className="mb-1">Дашборд</h2>
                <p className="text-muted mb-0">Обзор вашей деятельности и статистика</p>
              </div>
              <Button variant="primary" className="d-flex align-items-center">
                <i className="bi bi-plus-circle me-2"></i>
                Создать проект
              </Button>
            </div>




            {/* Статистические карточки */}
            <Row className="mb-4">
              <Col md={6} lg={3} className="mb-3">
                <Card className="border-0 shadow-sm">
                  <Card.Body>
                    <div className="d-flex justify-content-between align-items-center">
                      <div>
                        <h6 className="card-title text-muted mb-2">Пользователи</h6>
                        <h3 className="mb-0">1,248</h3>
                        <small className="text-success">
                          <i className="bi bi-arrow-up-short"></i> 12.5%
                        </small>
                      </div>
                      <div className="bg-primary bg-opacity-10 p-3 rounded">
                        <i className="bi bi-people-fill text-primary fs-4"></i>
                      </div>
                    </div>
                  </Card.Body>
                </Card>
              </Col>
              
              <Col md={6} lg={3} className="mb-3">
                <Card className="border-0 shadow-sm">
                  <Card.Body>
                    <div className="d-flex justify-content-between align-items-center">
                      <div>
                        <h6 className="card-title text-muted mb-2">Проекты</h6>
                        <h3 className="mb-0">56</h3>
                        <small className="text-success">
                          <i className="bi bi-arrow-up-short"></i> 5.2%
                        </small>
                      </div>
                      <div className="bg-success bg-opacity-10 p-3 rounded">
                        <i className="bi bi-folder-fill text-success fs-4"></i>
                      </div>
                    </div>
                  </Card.Body>
                </Card>
              </Col>
              
              <Col md={6} lg={3} className="mb-3">
                <Card className="border-0 shadow-sm">
                  <Card.Body>
                    <div className="d-flex justify-content-between align-items-center">
                      <div>
                        <h6 className="card-title text-muted mb-2">Задачи</h6>
                        <h3 className="mb-0">128</h3>
                        <small className="text-warning">
                          <i className="bi bi-arrow-down-short"></i> 2.1%
                        </small>
                      </div>
                      <div className="bg-warning bg-opacity-10 p-3 rounded">
                        <i className="bi bi-list-task text-warning fs-4"></i>
                      </div>
                    </div>
                  </Card.Body>
                </Card>
              </Col>
              
              <Col md={6} lg={3} className="mb-3">
                <Card className="border-0 shadow-sm">
                  <Card.Body>
                    <div className="d-flex justify-content-between align-items-center">
                      <div>
                        <h6 className="card-title text-muted mb-2">Доход</h6>
                        <h3 className="mb-0">$24.8K</h3>
                        <small className="text-success">
                          <i className="bi bi-arrow-up-short"></i> 18.3%
                        </small>
                      </div>
                      <div className="bg-info bg-opacity-10 p-3 rounded">
                        <i className="bi bi-currency-dollar text-info fs-4"></i>
                      </div>
                    </div>
                  </Card.Body>
                </Card>
              </Col>
            </Row>

            {/* Основной контент - таблица и графики */}
            <Row>
              <Col lg={8} className="mb-4">
                <Card className="border-0 shadow-sm h-100">
                  <Card.Header className="bg-white border-0">
                    <h5 className="mb-0">Последние проекты</h5>
                  </Card.Header>
                  <Card.Body>
                    <div className="table-responsive">
                      <table className="table table-hover">
                        <thead>
                          <tr>
                            <th>Название проекта</th>
                            <th>Статус</th>
                            <th>Прогресс</th>
                            <th>Срок</th>
                            <th>Действия</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr>
                            <td>Дизайн веб-сайта</td>
                            <td>
                              <Badge bg="success">Активный</Badge>
                            </td>
                            <td>
                              <div className="progress" style={{height: '6px'}}>
                                <div className="progress-bar bg-success" style={{width: '75%'}}></div>
                              </div>
                            </td>
                            <td>15.01.2024</td>
                            <td>
                              <Button variant="outline-primary" size="sm">
                                <i className="bi bi-eye"></i>
                              </Button>
                            </td>
                          </tr>
                          <tr>
                            <td>Мобильное приложение</td>
                            <td>
                              <Badge bg="warning">В процессе</Badge>
                            </td>
                            <td>
                              <div className="progress" style={{height: '6px'}}>
                                <div className="progress-bar bg-warning" style={{width: '45%'}}></div>
                              </div>
                            </td>
                            <td>20.02.2024</td>
                            <td>
                              <Button variant="outline-primary" size="sm">
                                <i className="bi bi-eye"></i>
                              </Button>
                            </td>
                          </tr>
                          <tr>
                            <td>База данных</td>
                            <td>
                              <Badge bg="secondary">Запланирован</Badge>
                            </td>
                            <td>
                              <div className="progress" style={{height: '6px'}}>
                                <div className="progress-bar bg-secondary" style={{width: '10%'}}></div>
                              </div>
                            </td>
                            <td>05.03.2024</td>
                            <td>
                              <Button variant="outline-primary" size="sm">
                                <i className="bi bi-eye"></i>
                              </Button>
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
                  </Card.Body>
                </Card>
              </Col>
              
              <Col lg={4} className="mb-4">
                <Card className="border-0 shadow-sm h-100">
                  <Card.Header className="bg-white border-0">
                    <h5 className="mb-0">Ближайшие события</h5>
                  </Card.Header>
                  <Card.Body>
                    <div className="d-flex mb-3">
                      <div className="bg-primary text-white rounded text-center p-2 me-3" style={{width: '50px'}}>
                        <div className="fw-bold">15</div>
                        <small>ЯНВ</small>
                      </div>
                      <div>
                        <h6 className="mb-1">Встреча с клиентом</h6>
                        <small className="text-muted">10:00 - 11:30</small>
                      </div>
                    </div>
                    
                    <div className="d-flex mb-3">
                      <div className="bg-success text-white rounded text-center p-2 me-3" style={{width: '50px'}}>
                        <div className="fw-bold">18</div>
                        <small>ЯНВ</small>
                      </div>
                      <div>
                        <h6 className="mb-1">Презентация проекта</h6>
                        <small className="text-muted">14:00 - 15:00</small>
                      </div>
                    </div>
                    
                    <div className="d-flex">
                      <div className="bg-warning text-white rounded text-center p-2 me-3" style={{width: '50px'}}>
                        <div className="fw-bold">22</div>
                        <small>ЯНВ</small>
                      </div>
                      <div>
                        <h6 className="mb-1">Дедлайн по проекту</h6>
                        <small className="text-muted">Весь день</small>
                      </div>
                    </div>
                  </Card.Body>
                </Card>
              </Col>
            </Row>


    </MpLayout>


  );
};
