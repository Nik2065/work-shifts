import React, {useState, useEffect} from 'react';

import {
  Container, 
  Card, 
  Button, 
  Badge,
  Modal,
  Form,
  Table,
  Alert, Spinner
} from 'react-bootstrap';

import {GetSiteUser, CreateSiteUser} from '../../services/apiService';


//{/* Модальное окно добавления/редактирования пользователя сайта */}

export function ModalForSiteUser({show, onHide, siteUserId, updateSiteUsers}) {

    // пользователь
    const defaultSiteUserData = {
        email:'',
        fio:'', 
        objects: [],
      };

    const [currentSiteUser, SetCurrentSiteUser] = useState(defaultSiteUserData);
    const [createButtonDisabled, setCreateButtonDisabled] = useState(false);
    const [objectsList, setObjectsList] = useState([]);
    const [alertData, setAlertData] = useState({
      show: false,
      message: '',
      variant: 'success'
    });
    
    

    function resetForm() {
      SetCurrentSiteUser(defaultSiteUserData);
      setAlertData({show: false, message: '', variant: 'success'});
      setCreateButtonDisabled(false);
    }

    useEffect(() => {
      
      // Получаем данные пользователя сайта по его id

      if (siteUserId) {
        GetSiteUser(employeeId)
        .then(response => response.json())
        .then(data => {
          console.log(data);
          if (data.isSuccess) {
            SetCurrentSiteUser(data.user);
          }
          else {
            // Обработка ошибки
          }
        })
        .catch(error => console.log(error));
      }
    }, 
    [siteUserId]);

    useEffect(() => {
      if (siteUserId) {
        //updateSiteUsers();
      }
    }, []);

    //Обновляем список объектов
    function updateObjects() {
      
    }

    function createSiteUser() {
      console.log("createSiteUser");
      
      //todo: проверка полей

      createSiteUser(currentSiteUser)
      .then(response => response.json())
      .then(data => {
        console.log(data);
        if (data.isSuccess) {
          // Обработка успешного создания сотрудника
          setAlertData({message: data.message, show: true, variant: 'success'});
          setCreateButtonDisabled(true);
          //обновляем список сотрудников
          //updateEmployees();
        }
        else {
          setAlertData({message: data.message, show: true, variant: 'danger'});
        }
      })
      .catch(error => console.log(error));


    }

    return (
      <Modal show={show} onHide={onHide}>
        <Modal.Header closeButton>
          <Modal.Title>
            {
            siteUserId ? 'Редактировать сотрудника' : 'Добавить сотрудника'
            }
            </Modal.Title>
        </Modal.Header>
       <Modal.Body>
        <Form>
            <Form.Group className="mb-3">
              <Form.Label>Логин(email) пользователя</Form.Label>
              <Form.Control
                type="text"
                value={currentSiteUser.login}
                onChange={(e) => SetCurrentSiteUser({ ...currentSiteUser, login: e.target.value })}
                placeholder="Введите логин"
              />
            </Form.Group>
            
            <Form.Group className="mb-3">
              <Form.Label>Роль</Form.Label>
              <Form.Select
                value={currentSiteUser.roleCode}
                onChange={(e) => SetCurrentSiteUser({ ...currentSiteUser, roleCode: e.target.value })}
                placeholder="Выберите роль"
              >
                <option value="admin">Администратор</option>
                <option value="object_manager">Начальник объекта</option>
                <option value="buh">Бухгалтерия</option>
              </Form.Select>
            </Form.Group>
            <div className="table-responsive">
              <Table>
                <tbody>
                  <td>
                    <Form.Check type="checkbox" checked={true} 
                    />
                  </td>
                </tbody>
              </Table>
            </div>

        </Form>

        <Alert show={alertData.show} variant={alertData.variant}>
            {alertData.message}
        </Alert>
       </Modal.Body>
       <Modal.Footer>
            {
            siteUserId ? 
            <Button  variant="primary">Сохранить</Button>
            : 
            <Button disabled={createButtonDisabled} onClick={createSiteUser} variant="primary">Добавить</Button>
            }
       </Modal.Footer>
      </Modal>
    )

/*
    return2 (
        <Modal onExit={resetForm} show={showEmpModal} onHide={() => setShowEmpModal(false)}>
            
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
                onChange={(e) => {
                  e.target.value = e.target.value.replace(/[^\d]/g, '');
                  SetCurrentEmployee({ ...currentEmployee, age: parseInt(e.target.value) })
                }}
                placeholder="Введите возраст"
              />
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Label>Удостоверение ЧОП</Form.Label>
              <Form.Check
                
                checked={currentEmployee.chopCertificate}
                onChange={(e) => {
                  //console.log(currentEmployee);
                  //console.log(e.target.checked);
                  SetCurrentEmployee({ ...currentEmployee, chopCertificate: e.target.checked })}
                }
              />
            </Form.Group>
            
            <Form.Group className="mb-3">
              <Form.Label>Объект</Form.Label>
              <Form.Select

                value={currentEmployee.object}
                onChange={(e) => SetCurrentEmployee({ ...currentEmployee, object: e.target.value })}
                placeholder="Выберите объект"
              >
                <option  value="Объект А">Объект А</option>
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
                <option  value="Альфа">Альфа</option>
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
        <br/>
        <Alert show={alertData.show} variant={alertData.variant}>
            {alertData.message}
        </Alert>

        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowEmpModal(false)}>
            Отмена
          </Button>
            {
            employeeId ? 
            <Button  variant="primary">Сохранить</Button>
            : 
            <Button disabled={createButtonDisabled} onClick={createEmployee} variant="primary">Добавить</Button>
            }
        </Modal.Footer>
      
        </Modal>
    )
  */
}