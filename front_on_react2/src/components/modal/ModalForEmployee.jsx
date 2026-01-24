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

import {CreateWorkShiftFromApi, DeteleWorkShiftFromApi, GetWorkShifts} from '../../services/apiService';
import {getDateFormat2} from '../../services/commonService';
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

      const [currentEmployee, setCurrentEmployee] = useState(defaultEmpData);
      const [objectsList, setObjectsList] = useState([]);
      const [startDate, setStartDate] = useState(new Date());
      const [endDate, setEndDate] = useState(new Date());
      const [workShiftList, setWorkShiftList] = useState([]);

      const [alertData, setAlertData] = useState({
        show: false,
        message: '',
        variant: 'success'
      });
      
      //для таблицы вахт
      const [alertData2, setAlertData2] = useState({
        show: false,
        message: '',
        variant: 'success'
      });
    
    const[createButtonDisabled, setCreateButtonDisabled] = useState(false);

    function resetForm() {
      setCurrentEmployee(defaultEmpData);
      setAlertData({show: false, message: '', variant: 'success'});
      setCreateButtonDisabled(false);
    }


    /*useEffect(() => {
      
      // Получаем данные сотрудника по его id

      if (employeeId) {
        GetEmployee(employeeId)  
        .then(response => response.json())
        .then(data => {
          console.log(data);
          if (data.isSuccess) {
            setCurrentEmployee(data.employee);
          }
          else {
            // Обработка ошибки
          }
        })
        .catch(error => console.log(error));

      }
    }, 
    [employeeId]);*/


    //получаем данные сотрудника
    function GetEmployeeOnShow(){

      if (employeeId) {
        GetEmployee(employeeId)  
        .then(data => {
          console.log(data);
          if (data.isSuccess) {
            setCurrentEmployee(data.employee);
            if(data.employee.workShiftList)
              setWorkShiftList(data.employee.workShiftList);
          }
          else {
            // Обработка ошибки
          }
        })
        .catch(error => console.log(error));

      }
    }



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
            setCurrentEmployee({ ...currentEmployee, objectId: data.objects[0].id })
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

    async function addWorkShift(){

      setAlertData2({message: "", show: false, variant: ''});

      const params = {
        employeeId: employeeId,
        start: startDate.toISOString(),
        end: endDate.toISOString()
      };


      CreateWorkShiftFromApi(params)
      .then(data => {
        console.log(data);
        if (data.isSuccess) {
          // Обработка успешного создания вахты
          //setAlertData2({message: data.message, show: true, variant: 'success'});
          //setCreateButtonDisabled(true);
          alert("Вахта успешно создана");

        }
        else {
          //setAlertData2({message: data.message, show: true, variant: 'danger'});
          alert("Ошибка добавления вахты:" + data.message);
        }
      })
      .catch(error => console.log(error));
    }

    async function deleteWorkShift(wsId){
      const params = {
        workShiftId: wsId,
      };

      DeteleWorkShiftFromApi(params)
      .then(data => {
        console.log(data);
        if (data.isSuccess) {
          // Обработка удаления записи о вахте
          alert("Запись о вахте успешно удалена");
        }
        else {
          //setAlertData({message: data.message, show: true, variant: 'danger'});
          alert("Ошибка при удалении записи о вахте");
        }
      })
      .catch(error => 
        console.log(error));
    }


    async function updateWorkShiftTable(){
      const params = {
        employeeId: employeeId,
      };

      GetWorkShifts(params)
      .then(data => {
        console.log(data);
        if (data.isSuccess) {
          //todo: крутилка
          setWorkShiftList(data.workShiftList);
        }
        else {
        }
      })
      .catch(error => 
        console.log(error));
    }
    
    async function addWorkShiftHandler(){
        await addWorkShift(); 
        await updateWorkShiftTable();
    }

    //удаление вахты
    async function deleteWorkShiftHandler(workshiftId) {
      await deleteWorkShift(workshiftId); 
      await updateWorkShiftTable();
    }



    return (
        <Modal size='xl' onExit={resetForm} show={showEmpModal} 
        onHide={() => setShowEmpModal(false)} 
        onShow={() => {
                        updateObjects();
                        GetEmployeeOnShow();
                        }}>
            <Modal.Header closeButton>
            <Modal.Title>
            {
            employeeId ? 'Редактировать сотрудника' : 'Добавить сотрудника'
            }
            <br/>
          </Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Row>
            <Col lg={6}>
            



        <Form>
          <Row >
            <Form.Group as={Col} md={12} className="mb-3">
              <Form.Label>Имя сотрудника</Form.Label>
              <Form.Control
                type="text"
                value={currentEmployee.fio}
                onChange={(e) => setCurrentEmployee({ ...currentEmployee, fio: e.target.value })}
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
                  setCurrentEmployee({ ...currentEmployee, age: parseInt(e.target.value) })
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
                  setCurrentEmployee({ ...currentEmployee, chopCertificate: e.target.checked })}
                }
              />
            </Form.Group>
            
            <Form.Group as={Col} md={12} className="mb-3">
              <Form.Label>Объект</Form.Label>
              <Form.Select

                value={currentEmployee.object}
                onChange={(e) => setCurrentEmployee({ ...currentEmployee, objectId: e.target.value })}
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
              <Form.Label>Оформление</Form.Label>
              <Form.Select
                value={currentEmployee.emplOptions}
                onChange={(e) => setCurrentEmployee({ ...currentEmployee, emplOptions: e.target.value })}
                placeholder="Выберите оформление"
              >
                <option value="Карта">Карта</option>
                <option value="Ведомость">Ведомость</option>
              </Form.Select>
            </Form.Group>

            <Form.Group as={Col} md={6} className="mb-3">
              <Form.Label>Банк</Form.Label>
              <Form.Select

                value={currentEmployee.bankName}
                onChange={(e) => setCurrentEmployee({ ...currentEmployee, bankName: e.target.value })}
                placeholder="Выберите Банк"
              >
                <option  value="Альфа">Альфа</option>
                <option value="Сбер">Сбер</option>
                <option value="ВТБ">ВТБ</option>
                <option value="ПСБ">ПСБ</option>
                <option value="Т-Банк">Т-Банк</option>
              </Form.Select>
            </Form.Group>
          
          </Row>
        </Form>
        <br/>
        <Alert show={alertData.show} variant={alertData.variant}>
            {alertData.message}
        </Alert>

            </Col>


            <Col>
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
                  <Button onClick={addWorkShiftHandler}
                  variant='outline-primary' 
                  size='sm'>Добавить</Button>
                </Col>
              </Row>

                  <br/>
                <div className="table-responsive" style={{maxHeight:"200px"}}>
              <Table bordered>
                <thead>
                  <tr>
                    <th width="40%">Дата начала</th>
                    <th width="40%">Дата окончания</th>
                    <th width="20%"></th>
                  </tr>
                </thead>
                <tbody>
                  {
                    workShiftList ?
                    workShiftList.map(w => (
                      <tr key={w.id}>
                        <td>{getDateFormat2(w.start)}</td>
                        <td>{getDateFormat2(w.end)}</td>
                        <td><Button title='Удалить'
                            onClick={() => deleteWorkShiftHandler(w.id)} 
                            variant='outline-danger' size='sm'>Х</Button></td>
                      </tr>
                    ))
                    : null
                  }
                </tbody>
              </Table>
              </div>
              <Alert variant={alertData2.variant} show={alertData2.show} >
                    {alertData2.message}
              </Alert>
            </Card.Body>
          </Card>

            </Col>
          </Row>

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