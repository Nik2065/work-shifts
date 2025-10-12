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


export function MpEmploeesPage () {
  return (
    <MpLayout>

            {/* Заголовок страницы */}
            <div className="d-flex justify-content-between align-items-center mb-4">
              <div>
                <h2 className="mb-1">Сотрудники</h2>
                <p className="text-muted mb-0"></p>
              </div>
              <Button variant="primary" className="d-flex align-items-center">
                <i className="bi bi-plus-circle me-2"></i>
                Добавить запись
              </Button>
            </div>



                <Card className="border-0 shadow-sm h-100">
                  <Card.Header className="bg-white border-0">
                    <h5 className="mb-0">&nbsp;</h5>
                  </Card.Header>
                  <Card.Body>
                    <div className="table-responsive">
                      <table className="table table-bordered table-hover">
                        <thead>
                          <tr>
                            <th width="15%">Фамилия Имя Отчество</th>
                            <th width="10%">Статус</th>
                            <th></th>
                            <th>12-окт</th>
                            <th>13-окт</th>
                            <th>14-окт</th>
                            <th>15-окт</th>
                            <th width="5%">Действия</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr>
                            <td>Фёдоров Константин Николаевич</td>
                            <td><Badge  bg="success"><span style={{fontSize:"1.0rem"}}>Активный</span></Badge></td>
                            <td>
                              Запланировано (часов)<br/>
                              Факт (часов)<br/>
                              Штраф (руб.)<br/>
                              Премия (руб.)<br/>
                            </td>
                            <td>
                              12<br/>
                              10<br/>
                              500 ₽<br/>
                              0 ₽<br/>
                            </td>
                            <td>
                              12<br/>
                              10<br/>
                              0 ₽<br/>
                              1200 ₽<br/>
                            </td>
                            <td>
                              12<br/>
                              10<br/>
                              0 ₽<br/>
                              1200 ₽<br/>
                            </td>
                             <td>
                              12<br/>
                              10<br/>
                              0 ₽<br/>
                              0 ₽<br/>
                            </td>
                            <td></td>
                          </tr>
                          <tr>
                            <td>Морозов Андрей Викторович</td>
                            <td><Badge  bg="success"><span style={{fontSize:"1.0rem"}}>Активный</span></Badge></td>
                            <td>
                              Запланировано (часов)<br/>
                              Факт (часов)<br/>
                              Штраф (руб.)<br/>
                              Премия (руб.)<br/>
                            </td>
                            <td>
                              12<br/>
                              10<br/>
                              500 ₽<br/>
                              0 ₽<br/>
                            </td>
                            <td>
                              12<br/>
                              10<br/>
                              0 ₽<br/>
                              1200 ₽<br/>
                            </td>
                            <td>
                              12<br/>
                              10<br/>
                              0 ₽<br/>
                              1200 ₽<br/>
                            </td>
                             <td>
                              12<br/>
                              10<br/>
                              0 ₽<br/>
                              0 ₽<br/>
                            </td>
                            <td></td>
                          </tr>
                          <tr>
                           <td>Новиков Кирилл Алексеевич</td>
                            <td><Badge  bg="warning"><span style={{fontSize:"1.0rem"}}>Уволен</span></Badge></td>
                            <td>
                              Запланировано (часов)<br/>
                              Факт (часов)<br/>
                              Штраф (руб.)<br/>
                              Премия (руб.)<br/>
                            </td>
                            <td>
                              12<br/>
                              10<br/>
                              500 ₽<br/>
                              0 ₽<br/>
                            </td>
                            <td>
                              12<br/>
                              10<br/>
                              0 ₽<br/>
                              1200 ₽<br/>
                            </td>
                            <td>
                              12<br/>
                              10<br/>
                              0 ₽<br/>
                              1200 ₽<br/>
                            </td>
                             <td>
                              12<br/>
                              10<br/>
                              0 ₽<br/>
                              0 ₽<br/>
                            </td>
                            <td></td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
                  </Card.Body>
                </Card>



    </MpLayout>


  );
};
