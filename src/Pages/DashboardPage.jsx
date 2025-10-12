import React, { useState, useEffect, useContext } from 'react';
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
import { 
  
  User, 
  Clock, 
  Play, 
  Pause, 
  Plus, 
  Edit3, 
  Trash2,
  ChevronLeft,
  ChevronRight,
  Building2
} from 'lucide-react';


import Calendar from 'react-calendar';
import 'react-calendar/dist/Calendar.css';



export function DashboardPage() {

  useEffect(() => {
    const today = new Date();
    //запрашиваем данные с сервера за выбранную дату
    
  }, []);

  const [selectedDate, setSelectedDate] = useState(new Date(2025, 9, 10)); // 10 октября 2025
  //const [currentMonth, setCurrentMonth] = useState(9); // Октябрь
  //const [currentYear, setCurrentYear] = useState(2025);
  const [showAddModal, setShowAddModal] = useState(false);
  const [editingEmployee, setEditingEmployee] = useState(null);
  const [newEmployee, setNewEmployee] = useState({ name: '', position: '', status: 'active' });

  // Mock данные сотрудников для разных дат
  const [employeesData, setEmployeesData] = useState({
    '2025-10-10': [
      { id: 1, name: 'Иванов Иван Иванович', position: 'Разработчик', hours: 8.5, isActive: false },
      { id: 2, name: 'Петрова Мария Сергеевна', position: 'Дизайнер', hours: 7.0, isActive: true },
      { id: 3, name: 'Сидоров Алексей Владимирович', position: 'Менеджер', hours: 6.5, isActive: false }
    ],
    '2025-10-11': [
      { id: 1, name: 'Иванов Иван Иванович', position: 'Разработчик', hours: 7.5, isActive: false },
      { id: 2, name: 'Петрова Мария Сергеевна', position: 'Дизайнер', hours: 8.0, isActive: false },
      { id: 4, name: 'Козлова Елена Андреевна', position: 'Тестировщик', hours: 9.0, isActive: true }
    ]
  });

  // Все сотрудники (для выпадающего списка)
  const [allEmployees] = useState([
    { id: 1, name: 'Иванов Иван Иванович', object: 'Объект А', age: 32, dateAdded: '2023-01-15', status: 'active' },
    { id: 2, name: 'Петрова Мария Сергеевна', object: 'Объект Б', age: 28, dateAdded: '2023-03-22', status: 'active' },
    { id: 3, name: 'Сидоров Алексей Владимирович', object: 'Объект А', age: 45, dateAdded: '2022-11-08', status: 'fired' },
    { id: 4, name: 'Козлова Елена Андреевна', object: 'Объект В', age: 35, dateAdded: '2023-07-14', status: 'active' },
    { id: 5, name: 'Волков Дмитрий Павлович', object: 'Объект Б', age: 29, dateAdded: '2023-09-30', status: 'active' }
  ]);

  // Форматирование даты
  const formatDate = (date) => {
    return date.toISOString().split('T')[0];
  };

  // Получение сотрудников на выбранную дату
  const getCurrentEmployeesData = () => {
    const dateKey = formatDate(selectedDate);
    return employeesData[dateKey] || [];
  };

  // Обновление данных сотрудников
  const updateEmployeesData = (updatedEmployees) => {
    const dateKey = formatDate(selectedDate);
    setEmployeesData(prev => ({
      ...prev,
      [dateKey]: updatedEmployees
    }));
  };

  // Обработчики таймера
  const handleStartTimer = (id) => {
    const currentEmployees = getCurrentEmployeesData();
    const updatedEmployees = currentEmployees.map(emp =>
      emp.id === id ? { ...emp, isActive: true } : emp
    );
    updateEmployeesData(updatedEmployees);
  };



  const handleAddHours = (id, hours) => {
    const currentEmployees = getCurrentEmployeesData();
    const updatedEmployees = currentEmployees.map(emp =>
      emp.id === id ? { ...emp, hours: emp.hours + hours } : emp
    );
    updateEmployeesData(updatedEmployees);
  };

  // Обработчики сотрудников
  const handleAddEmployee = () => {
    if (newEmployee.name && newEmployee.position) {
      const currentEmployees = getCurrentEmployeesData();
      const newEmp = {
        id: Date.now(),
        name: newEmployee.name,
        position: newEmployee.position,
        hours: 0,
        isActive: false
      };
      updateEmployeesData([...currentEmployees, newEmp]);
      setNewEmployee({ name: '', position: '', status: 'active' });
      setShowAddModal(false);
    }
  };

  const handleEditEmployee = (employee) => {
    setEditingEmployee(employee);
    setNewEmployee({ 
      name: employee.name, 
      position: employee.position, 
      status: 'active' 
    });
    setShowAddModal(true);
  };

  const handleUpdateEmployee = () => {
    if (newEmployee.name && newEmployee.position && editingEmployee) {
      const currentEmployees = getCurrentEmployeesData();
      const updatedEmployees = currentEmployees.map(emp =>
        emp.id === editingEmployee.id
          ? { ...emp, name: newEmployee.name, position: newEmployee.position }
          : emp
      );
      updateEmployeesData(updatedEmployees);
      setNewEmployee({ name: '', position: '', status: 'active' });
      setEditingEmployee(null);
      setShowAddModal(false);
    }
  };

  const handleDeleteEmployee = (id) => {
    const currentEmployees = getCurrentEmployeesData();
    const updatedEmployees = currentEmployees.filter(emp => emp.id !== id);
    updateEmployeesData(updatedEmployees);
  };

  

  const currentEmployees = getCurrentEmployeesData();
  const totalHours = currentEmployees.reduce((sum, emp) => sum + emp.hours, 0);
  const activeEmployees = currentEmployees.filter(emp => emp.isActive).length;

  return (
    <Row>
      <Col lg={3} className="mb-4">
        <Calendar  />

      </Col>
      
      <Col lg={9}>
        {/* Date Header */}
        <Card className="border-0 shadow-sm mb-4">
          <Card.Body>
            <div className="d-flex justify-content-between align-items-center">
              <div>
                <h4 className="fw-semibold mb-1">
                  {selectedDate.toLocaleDateString('ru-RU', { 
                    weekday: 'long', 
                    year: 'numeric', 
                    month: 'long', 
                    day: 'numeric' 
                  })}
                </h4>
                <p className="text-muted mb-0">
                  Отработано: {totalHours.toFixed(1)} часов • Активных: {activeEmployees}
                </p>
              </div>
              <Button
                variant="primary"
                onClick={() => {
                  setEditingEmployee(null);
                  setNewEmployee({ name: '', position: '', status: 'active' });
                  setShowAddModal(true);
                }}
              >
                <Plus size={16} className="me-2" />
                Добавить запись о времени
              </Button>
            </div>
          </Card.Body>
        </Card>

        {/* Employees List */}
        {currentEmployees.length === 0 ? (
          <Card className="border-0 shadow-sm">
            <Card.Body className="text-center py-5">
              <User size={64} className="text-muted mb-3" />
              <h5 className="text-muted">Нет данных</h5>
              <p className="text-muted mb-0">
                За выбранную дату нет записей о работе сотрудников
              </p>
            </Card.Body>
          </Card>
        ) : (
          <div className="space-y-3">
            {currentEmployees.map((employee) => (
              <Card key={employee.id} className="border-0 shadow-sm">
                <Card.Body>
                  <div className="d-flex justify-content-between align-items-start mb-3">
                    <div className="d-flex align-items-center">
                      <div className={`rounded-circle p-3 me-3 ${employee.isActive ? 'bg-success' : 'bg-light'}`}>
                        <User className={employee.isActive ? 'text-white' : 'text-muted'} size={24} />
                      </div>
                      <div>
                        <h5 className="fw-semibold mb-1">{employee.name}</h5>
                        <p className="text-muted mb-0">{employee.position}</p>
                      </div>
                    </div>
                    <div className="d-flex align-items-center">
                      <div className="text-end me-3">
                        <div className="d-flex align-items-center">
                          <Clock size={16} className="text-muted me-2" />
                          <span className="fw-bold fs-5">{employee.hours.toFixed(1)}ч</span>
                        </div>
                        {employee.isActive && (
                          <Badge bg="success" className="mt-1">
                            <Play size={12} className="me-1" />
                            Активен
                          </Badge>
                        )}
                      </div>
                      <div className="d-flex">
                        <Button
                          variant="outline-primary"
                          size="sm"
                          className="me-2"
                          onClick={() => handleEditEmployee(employee)}
                        >
                          <Edit3 size={14} />
                        </Button>
                        <Button
                          variant="outline-danger"
                          size="sm"
                          onClick={() => handleDeleteEmployee(employee.id)}
                        >
                          <Trash2 size={14} />
                        </Button>
                      </div>
                    </div>
                  </div>

                  <div className="d-flex justify-content-between align-items-center">
                    <div className="d-flex gap-2">
                      {employee.isActive ? (
                        <Button
                          variant="danger"
                          size="sm"
                          onClick={() => handleStopTimer(employee.id)}
                        >
                          <Pause size={14} className="me-1" />
                          Остановить
                        </Button>
                      ) : (
                        <Button
                          variant="success"
                          size="sm"
                          onClick={() => handleStartTimer(employee.id)}
                        >
                          <Play size={14} className="me-1" />
                          Начать работу
                        </Button>
                      )}
                      <div className="d-flex gap-1">
                        <Button
                          variant="outline-secondary"
                          size="sm"
                          onClick={() => handleAddHours(employee.id, 0.5)}
                        >
                          +0.5ч
                        </Button>
                        <Button
                          variant="outline-secondary"
                          size="sm"
                          onClick={() => handleAddHours(employee.id, 1)}
                        >
                          +1ч
                        </Button>
                      </div>
                    </div>
                    <div className="text-muted small">
                      Норма: 8ч • Отклонение: {(employee.hours - 8).toFixed(1)}ч
                    </div>
                  </div>
                </Card.Body>
              </Card>
            ))}
          </div>
        )}
      </Col>

      {/* Модальное окно добавления/редактирования сотрудника */}
      <Modal show={showAddModal} onHide={() => setShowAddModal(false)}>
        <Modal.Header closeButton>
          <Modal.Title>
            {editingEmployee ? 'Редактировать сотрудника' : 'Добавить сотрудника'}
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
    </Row>
  );
};

export default DashboardPage;