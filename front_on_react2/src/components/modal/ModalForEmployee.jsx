import React, {useState, useEffect} from 'react';

import {
  Container, 
  Row, 
  Col, 
  Card, 
  Button, 
  Modal,
  Form,
  Table,
  Alert
} from 'react-bootstrap';
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";

import {GetEmployee, CreateEmployee, 
  GetAllObjects, SaveEmployeeFromApi} from '../../services/apiService';

import { registerLocale, setDefaultLocale } from  "react-datepicker";
import { ru } from 'date-fns/locale/ru';
registerLocale('ru', ru)

//{/* Модальное окно добавления/редактирования сотрудника */}

export function ModalForEmployee({showEmpModal, setShowEmpModal, employeeId, updateEmployees}) {

    // сотрудник
    const defaultEmpData = {fio:'', 
        age:30,
        chopCertificate: false,
        objectId: 0,
        bankName: 'Альфа',
        emplOptions: 'Карта',
      };

      const [currentEmployee, SetCurrentEmployee] = useState(defaultEmpData);
      const [objectsList, setObjectsList] = useState([]);
      const [startDate, setStartDate] = useState(new Date());
      const [endDate, setEndDate] = useState(new Date());

      const [alertData, setAlertData] = useState({
        show: false,
        message: '',
        variant: 'success'
      });
    
    const[createButtonDisabled, setCreateButtonDisabled] = useState(false);

    function resetForm() {
      SetCurrentEmployee(defaultEmpData);
      setAlertData({show: false, message: '', variant: 'success'});
      setCreateButtonDisabled(false);
    }

    useEffect(() => {
      
      // Получаем данные сотрудника по его id

      if (employeeId) {
        GetEmployee(employeeId)  
        .then(response => response.json())
        .then(data => {
          console.log(data);
          if (data.isSuccess) {
            SetCurrentEmployee(data.employee);
          }
          else {
            // Обработка ошибки
          }
        })
        .catch(error => console.log(error));

      }
    }, 
    [employeeId]);

    //Обновляем список объектов
    function updateObjects() {
      console.log("updateObjects");
      GetAllObjects()
      .then(data => {
        console.log(data);
        if (data.isSuccess) {
          setObjectsList(data.objects);
          //устанавливаем айдишник для пользователя 
          if(data.objects.length>0)
            SetCurrentEmployee({ ...currentEmployee, objectId: data.objects[0].id })
        }
        else {
          // Обработка ошибки
        }
      })
      .catch(error => console.log(error));
    }



    function createEmployee() {
      console.log("createEmployee");
      
      setAlertData({message: "", show: false, variant: ''});
        if(currentEmployee.fio.length<5){
          setAlertData({message: "Имя сотрудника должно быть не менее 5 символов", show: true, variant: 'danger'});
          return;
        }
        if(currentEmployee.age<18 || currentEmployee.age >60){
          setAlertData({message: "Возраст должен быть в диапазоне от 18 до 60 лет", show: true, variant: 'danger'});
          return;
        }


      CreateEmployee(currentEmployee)
      .then(data => {
        console.log(data);
        if (data.isSuccess) {
          // Обработка успешного создания сотрудника
          setAlertData({message: data.message, show: true, variant: 'success'});
          setCreateButtonDisabled(true);
          //обновляем список сотрудников
          updateEmployees();
        }
        else {
          setAlertData({message: data.message, show: true, variant: 'danger'});
        }
      })
      .catch(error => console.log(error));


    }

    //Обновляем данные сотрудника
    function updateEmployee() {
        setAlertData({message: "", show: false, variant: ''});
        if(currentEmployee.fio.length<5){
          setAlertData({message: "Имя сотрудника должно быть не менее 5 символов", show: true, variant: 'danger'});
          return;
        }
        if(currentEmployee.age<18 || currentEmployee.age >60){
          setAlertData({message: "Возраст должен быть в диапазоне от 18 до 60 лет", show: true, variant: 'danger'});
          return;
        }

      console.log("updateEmployee");

      SaveEmployeeFromApi(currentEmployee)
      .then(data => {
        console.log(data);
        if (data.isSuccess) {
          // Обработка успешного редактирования сотрудника
          setAlertData({message: data.message, show: true, variant: 'success'});
          setCreateButtonDisabled(true);
          //обновляем список сотрудников
          updateEmployees();
        }
        else {
          setAlertData({message: data.message, show: true, variant: 'danger'});
        }
      })
      .catch(error => console.log(error));
    }

    function addWorkShift(){
      
    }


    return (
        <Modal onExit={resetForm} show={showEmpModal} onHide={() => setShowEmpModal(false)} onShow={updateObjects}>
            <Modal.Header closeButton>
            <Modal.Title>
            {
            employeeId ? 'Редактировать сотрудника' : 'Добавить сотрудника'
            }
            <br/>
          </Modal.Title>
        </Modal.Header>
        <Modal.Body>
        <Form>
          <Row >
            <Form.Group as={Col} md={12} className="mb-3">
              <Form.Label>Имя сотрудника</Form.Label>
              <Form.Control
                type="text"
                value={currentEmployee.fio}
                onChange={(e) => SetCurrentEmployee({ ...currentEmployee, fio: e.target.value })}
                placeholder="Введите имя"
              />
            </Form.Group>
            <Form.Group as={Col} md={6} className="mb-3">
              <Form.Label>Возраст</Form.Label>
              <Form.Control
                type="text"
                value={currentEmployee.age}
                onChange={(e) => {
                  e.target.value = e.target.value.replace(/[^\d]/g, '');
                  SetCurrentEmployee({ ...currentEmployee, age: parseInt(e.target.value) })
                }}
                placeholder="Введите возраст"
              />
            </Form.Group>

            <Form.Group as={Col} md={6} className="mb-3">
              <Form.Label>&nbsp;</Form.Label>
              <Form.Check
                label="Удостоверение ЧОП"
                type="checkbox"
                checked={currentEmployee.chopCertificate}
                onChange={(e) => {
                  //console.log(currentEmployee);
                  //console.log(e.target.checked);
                  SetCurrentEmployee({ ...currentEmployee, chopCertificate: e.target.checked })}
                }
              />
            </Form.Group>
            
            <Form.Group as={Col} md={12} className="mb-3">
              <Form.Label>Объект</Form.Label>
              <Form.Select

                value={currentEmployee.object}
                onChange={(e) => SetCurrentEmployee({ ...currentEmployee, objectId: e.target.value })}
                placeholder="Выберите объект"
              >
                
                {
                  objectsList ?
                  objectsList.map(obj => (
                    <option key={obj.id} value={obj.id}>{obj.name}</option>
                  ))
                  : null
                }
              </Form.Select>
            </Form.Group>
            <Form.Group as={Col} md={6} className="mb-3">
              <Form.Label>Банк</Form.Label>
              <Form.Select

                value={currentEmployee.bankName}
                onChange={(e) => SetCurrentEmployee({ ...currentEmployee, bankName: e.target.value })}
                placeholder="Выберите Банк"
              >
                <option  value="Альфа">Альфа</option>
                <option value="Сбер">Сбер</option>
                <option value="ВТБ">ВТБ</option>
                <option value="ПСБ">ПСБ</option>
              </Form.Select>
            </Form.Group>

            <Form.Group as={Col} md={6} className="mb-3">
              <Form.Label>Оформление</Form.Label>
              <Form.Select

                value={currentEmployee.emplOptions}
                onChange={(e) => SetCurrentEmployee({ ...currentEmployee, emplOptions: e.target.value })}
                placeholder="Выберите оформление"
              >
                <option value="Карта">Карта</option>
                <option value="Ведомость">Ведомость</option>
              </Form.Select>
            </Form.Group>
          <Card>
            <Card.Body>
              <Card.Title>Вахты</Card.Title>
              <Row>
                <Col md={4}>
                  <DatePicker  locale="ru" selected={startDate} onChange={(date) => setStartDate(date)} />
                </Col>
                <Col md={4}>
                  <DatePicker locale="ru" selected={endDate} onChange={(date) => setEndDate(date)} />
                </Col>
                <Col md={4} style={{textAlign:"right"}}>
                  <Button onClick={addWorkShift}
                  variant='outline-primary' 
                  size='sm'>Добавить</Button>
                </Col>
              </Row>

                  <br/>
                <div className="table-responsive">
              <Table bordered>
                <thead>
                  <tr>
                    <th>Дата начала</th>
                    <th>Дата окончания</th>
                  </tr>
                </thead>
                <tbody>
                  
                </tbody>
              </Table>
              </div>
            </Card.Body>
          </Card>
          </Row>
        </Form>
        <br/>
        <Alert show={alertData.show} variant={alertData.variant}>
            {alertData.message}
        </Alert>

        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowEmpModal(false)}>
            Закрыть
          </Button>
            {
            employeeId ? 
            <Button 
            disabled={createButtonDisabled} onClick={updateEmployee}
            variant="primary">Сохранить</Button>
            : 
            <Button 
            disabled={createButtonDisabled} onClick={createEmployee} variant="primary">Добавить</Button>
            }
        </Modal.Footer>
        </Modal>
    )
  
}