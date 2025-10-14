import React, {useState, useEffect} from 'react';

import {
  Container, 
  Row, 
  Col, 
  Card, 
  Button, 
  Badge,
  Modal,
  Form
} from 'react-bootstrap';

//{/* Модальное окно добавления/редактирования сотрудника */}

export function ModalForEmployee({showAddModal, setShowAddModal}) {

    //имя сотрудника
    const [newEmployee, SetNewEmployee] = useState({name:'', age:30, chop:false})



    return (
        <Modal show={showAddModal} onHide={() => setShowAddModal(false)}>
            <Modal.Header closeButton>
            <Modal.Title>
                Добавить сотрудника
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
                onChange={(e) => SetNewEmployee({ ...newEmployee, name: e.target.value })}
                placeholder="Введите имя"
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Возраст</Form.Label>
              <Form.Control
                type="text"
                value={newEmployee.age}
                onChange={(e) => SetNewEmployee({ ...newEmployee, age: e.target.value })}
                placeholder="Введите возраст"
              />
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Label>Удостоверение ЧОП</Form.Label>
              <Form.Check
                
                value={newEmployee.chop}
                onChange={(e) => SetNewEmployee({ ...newEmployee, chop: e.target.value })}
                placeholder="Введите возраст"
              />
            </Form.Group>
            
            <Form.Group className="mb-3">
              <Form.Label>Объект</Form.Label>
              <Form.Select

                value={newEmployee.object}
                onChange={(e) => SetNewEmployee({ ...newEmployee, object: e.target.value })}
                placeholder="Выберите объект"
              >
                <option value="Объект А">Объект А</option>
                <option value="Объект Б">Объект Б</option>
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