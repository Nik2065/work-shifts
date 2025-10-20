import React, {useState, useEffect} from 'react';

import {
  Container, 
  Row, 
  Col, 
  Card, 
  Button, 
  Badge,
  Modal,
  Form,
  Table
} from 'react-bootstrap';

import {GetEmployee} from '../../services/apiService';


//{/* Модальное окно добавления/редактирования сотрудника */}

export function ModalForEmployee({showEmpModal, setShowEmpModal, employeeId}) {

    //имя сотрудника
    const [currentEmployee, SetCurrentEmployee] = useState(
      {name:'', 
        age:30, 
        chopCertificate:false
      })
    
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


    return (
        <Modal show={showEmpModal} onHide={() => setShowEmpModal(false)}>
            <Modal.Header closeButton>
            <Modal.Title>
            {
            employeeId ? 'Редактировать сотрудника' : 'Добавить сотрудника'
            }
            <br/>
            
            employeeId={employeeId}
          </Modal.Title>
        </Modal.Header>
        <Modal.Body>
        <Form>
            <Form.Group className="mb-3">
              <Form.Label>Имя сотрудника</Form.Label>
              <Form.Control
                type="text"
                value={currentEmployee.fio}
                onChange={(e) => SetCurrentEmployee({ ...currentEmployee, fio: e.target.value })}
                placeholder="Введите имя"
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Возраст</Form.Label>
              <Form.Control
                type="text"
                value={currentEmployee.age}
                onChange={(e) => SetCurrentEmployee({ ...currentEmployee, age: e.target.value })}
                placeholder="Введите возраст"
              />
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Label>Удостоверение ЧОП</Form.Label>
              <Form.Check
                
                value={currentEmployee.chopCertificate}
                onChange={(e) => SetCurrentEmployee({ ...currentEmployee, chopCertificate: e.target.value })}
              />
            </Form.Group>
            
            <Form.Group className="mb-3">
              <Form.Label>Объект</Form.Label>
              <Form.Select

                value={currentEmployee.object}
                onChange={(e) => SetCurrentEmployee({ ...currentEmployee, object: e.target.value })}
                placeholder="Выберите объект"
              >
                <option value="Объект А">Объект А</option>
                <option value="Объект Б">Объект Б</option>
                <option value="Объект Д">Объект Д</option>
              </Form.Select>
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Банк</Form.Label>
              <Form.Select

                value={currentEmployee.bankName}
                onChange={(e) => SetCurrentEmployee({ ...currentEmployee, bankName: e.target.value })}
                placeholder="Выберите Банк"
              >
                <option value="Альфа">Альфа</option>
                <option value="Сбер">Сбер</option>
                <option value="ВТБ">ВТБ</option>
                <option value="ПСБ">ПСБ</option>
              </Form.Select>
            </Form.Group>

            <Form.Group className="mb-3">
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
    <Table>
      <tbody>
        
      </tbody>
    </Table>
  </Card.Body>
</Card>
        </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowEmpModal(false)}>
            Отмена
          </Button>
          <Button 
            variant="primary" 
            
          >
            Добавить
        </Button>
        </Modal.Footer>
        </Modal>
    )
    
  return2 (
      
      <Modal show={showAddModal} onHide={() => setShowAddModal(false)}>
        <Modal.Header closeButton>
          <Modal.Title>
            {
            //editingEmployee ? 'Редактировать сотрудника' : 'Добавить сотрудника'
            }
          </Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Имя сотрудника</Form.Label>
              <Form.Control
                type="text"
                value={newEmployee.name}
                onChange={(e) => setNewEmployee({ ...newEmployee, name: e.target.value })}
                placeholder="Введите имя"
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Должность</Form.Label>
              <Form.Control
                type="text"
                value={newEmployee.position}
                onChange={(e) => setNewEmployee({ ...newEmployee, position: e.target.value })}
                placeholder="Введите должность"
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Статус</Form.Label>
              <Form.Select
                value={newEmployee.status}
                onChange={(e) => setNewEmployee({ ...newEmployee, status: e.target.value })}
              >
                <option value="active">Активен</option>
                <option value="fired">Уволен</option>
              </Form.Select>
            </Form.Group>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowAddModal(false)}>
            Отмена
          </Button>
          <Button 
            variant="primary" 
            onClick={editingEmployee ? handleUpdateEmployee : handleAddEmployee}
          >
            {editingEmployee ? 'Сохранить' : 'Добавить'}
          </Button>
        </Modal.Footer>
      </Modal>
  );
}