import React ,{ useState, useEffect} from 'react';
import { 
  Container, Row, Col, Nav, 
  Navbar, Card, Button, Badge,
  Dropdown, Form
} from 'react-bootstrap';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap-icons/font/bootstrap-icons.css';

import '../dashboard.css';
import { MpLayout } from './MpLayout';

import Calendar from 'react-calendar';
//import 'react-calendar/dist/Calendar.css';
import '../calendar.css';
import { ModalForEmployee } from '../components/ModalForEmployee';
import { ModalForWorkShift } from '../components/ModalForWorkShift';


export function MpEmploeesPage () {

const [showAddModal, setShowAddModal] = useState(false);
const [showShiftsModal, setShowShiftsModal] = useState(false);

  return (
    <MpLayout>

            {/* Заголовок страницы */}
            <div className="d-flex justify-content-between align-items-center mb-4">
              <div>
                <h2 className="mb-1">Сотрудники</h2>
                <p className="text-muted mb-0"></p>
              </div>
              <Button onClick={()=> setShowAddModal(true)} variant="primary" className="d-flex align-items-center">
                <i className="bi bi-plus-circle me-2"></i>
                Добавить сотрудника
              </Button>
            </div>

          
 


    <Row>
      <Col lg={3} className="mb-4">
        <Calendar  />
      </Col>
      <Col lg={9} className="mb-4">

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
                      <table className="table table-bordered table-hover">
                        <thead>
                          <tr>
                            <th rowSpan={2} width="30%">Фамилия Имя Отчество</th>
                            <th rowSpan={2} width="10%">Объект</th>
                            <th colSpan={3} width="40%" className='text-center' style={{fontSize:"1.2rem"}}>15-окт-2025</th>
                            <th rowSpan={2} width="20%" style={{verticalAlign:"middle"}} >Действия<br/></th>
                          </tr>
                          <tr>
                            <th width="10%">Смена</th>
                            <th width="10%">Отработанно часов</th>
                            <th width="20%">Ставка в час, руб.</th>
                            
                          </tr>
                        </thead>
                        <tbody>
                          <tr>
                            <td>Фёдоров Константин Николаевич</td>
                            <td>Объект А</td>
                            <td>
                              
                              <Form.Check type='checkbox'   />

                            </td>
                            <td>
                              <Form.Control type='text'  />
                            </td>
                            <td>
                            <Form.Select>
                                <option>1000</option>
                                <option>1200</option>
                                <option>1400</option>
                                <option>1600</option>
                                <option>1800</option>
                                <option>2000</option>
                              </Form.Select>
                            </td>
                            <td>
                              <Button variant="outline-primary" size="sm">Редактировать анкету</Button>
                              <br/><br/>
                              <Button onClick={()=>setShowShiftsModal(true)} variant="outline-primary" size="sm">Заполнить смены</Button>
                            </td>
                          </tr>


                          <tr>
                            <td>Морозов Андрей Викторович</td>
                            <td>Объект А</td>
                            <td>
                              
                              <Form.Check type='checkbox' checked={true}  />

                            </td>
                            <td>
                              <Form.Control type='text'  />
                            </td>
                            <td>
                            <Form.Select>
                                <option>1000</option>
                                <option>1200</option>
                                <option>1400</option>
                                <option>1600</option>
                                <option>1800</option>
                                <option>2000</option>
                              </Form.Select>
                            </td>
                            <td>
                              <Button variant="outline-primary" size="sm">Редактировать анкету</Button>
                              <br/><br/>
                              <Button variant="outline-primary" size="sm">Заполнить смены</Button>
                            </td>
                          </tr>


                          <tr>
                          <td>Новиков Кирилл Алексеевич</td>
                          <td>Объект А</td>
                            <td>
                              
                              <Form.Check type='checkbox' checked={true}  />

                            </td>
                            <td>
                              <Form.Control type='text'  />
                            </td>
                            <td>
                            <Form.Select>
                                <option>1000</option>
                                <option>1200</option>
                                <option>1400</option>
                                <option>1600</option>
                                <option>1800</option>
                                <option>2000</option>
                              </Form.Select>
                            </td>
                            <td>
                              <Button variant="outline-primary" size="sm">Редактировать анкету</Button>
                              <br/><br/>
                              <Button variant="outline-primary" size="sm">Заполнить смены</Button>
                            </td>
                          </tr>
                        
                        </tbody>
                      </table>
                      <div style={{textAlign:"right"}}>
                        
                          <Button variant="primary">Сохранить</Button>
                      </div>
                    </div>
                  </Card.Body>
                </Card>

      </Col>
    </Row>

    <ModalForEmployee showAddModal={showAddModal} setShowAddModal={setShowAddModal}  />
    <ModalForWorkShift showShiftsModal={showShiftsModal} setShowShiftsModal={setShowShiftsModal}  />



    </MpLayout>


  );
};
