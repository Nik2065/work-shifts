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


export function MpSiteUsersPage () {
  return (
    <MpLayout>

            {/* Заголовок страницы */}
            <div className="d-flex justify-content-between align-items-center mb-4">
              <div>
                <h2 className="mb-1">Пользователи сайта</h2>
                <p className="text-muted mb-0"></p>
              </div>
              <Button variant="primary" className="d-flex align-items-center">
                <i className="bi bi-plus-circle me-2"></i>
                Добавить пользователя
              </Button>
            </div>



                <Card className="border-0 shadow-sm h-100">
                  <Card.Header className="bg-white border-0">
                    <h5 className="mb-0">&nbsp;</h5>
                  </Card.Header>
                  <Card.Body>
                    <div className="table-responsive">
                      <table className="table table-hover">
                        <thead>
                          <tr>
                            <th>Фамилия Имя Отчество</th>
                            <th>Роль</th>
                            <th>Статус</th>
                            <th>Дата добавления</th>
                            <th>Действия</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr>
                            <td>Иванов Александр Сергеевич</td>
                            <td>Администратор</td>
                            <td>
                              <Badge bg="success">Активный</Badge>
                            </td>
                            <td>15.01.2024</td>
                            <td>
                              <Button variant="outline-primary" size="sm">
                                <i className="bi bi-pencil"></i>
                              </Button>
                            </td>
                          </tr>
                          <tr>
                            <td>Петров Дмитрий Игоревич</td>
                            <td>Администратор</td>
                            <td>
                              <Badge bg="success">Активный</Badge>
                            </td>
                            <td>15.05.2025</td>
                            <td>
                              <Button variant="outline-primary" size="sm">
                                <i className="bi bi-pencil"></i>
                              </Button>
                            </td>
                          </tr>
                          <tr>
                            <td>Лебедева Светлана Викторовна</td>
                            <td>Бухгалтер</td>
                            <td>
                              <Badge bg="success">Активный</Badge>
                            </td>
                            <td>15.05.2025</td>
                            <td>
                              <Button variant="outline-primary" size="sm">
                                <i className="bi bi-pencil"></i>
                              </Button>
                            </td>
                          </tr>

                          <tr>
                            <td>Морозова Юлия Александровна</td>
                            <td>Сотрудник отдела кадров</td>
                            <td>
                              <Badge bg="success">Активный</Badge>
                            </td>
                            <td>15.05.2025</td>
                            <td>
                              <Button variant="outline-primary" size="sm">
                                <i className="bi bi-pencil"></i>
                              </Button>
                            </td>
                          </tr>
                          <tr>
                            <td>Волкова Ирина Олеговна</td>
                            <td>Сотрудник отдела кадров</td>
                            <td>
                              <Badge bg="danger">Уволен</Badge>
                            </td>
                            <td>15.08.2025</td>
                            <td>
                              <Button variant="outline-primary" size="sm">
                                <i className="bi bi-pencil"></i>
                              </Button>
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
                  </Card.Body>
                </Card>



    </MpLayout>


  );
};
