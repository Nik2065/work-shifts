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

import {GetSiteUser, CreateSiteUserFromApi, 
  GetAllObjects, SaveSiteUserFromApi} from '../../services/apiService';

import { Navigate } from "react-router-dom";

//{/* Модальное окно добавления/редактирования пользователя сайта */}

export function ModalForSiteUser({show, onHide, siteUserId, updateSiteUsers}) {

    // пользователь
    const defaultSiteUserData = {
        login:'',
        roleCode:'admin',
        password:'',
        objects: [],
        objectsListIds: []
      };

    const [currentSiteUser, setCurrentSiteUser] = useState(defaultSiteUserData);
    const [createButtonDisabled, setCreateButtonDisabled] = useState(false);
    const [objectsList, setObjectsList] = useState([]);
    const [alertData, setAlertData] = useState({
      show: false,
      message: '',
      variant: 'success'
    });
    
    

    function resetForm() {
      setCurrentSiteUser(defaultSiteUserData);
      setAlertData({show: false, message: '', variant: 'success'});
      setCreateButtonDisabled(false);
    }

    useEffect(() => {
      
      // Получаем данные пользователя сайта по его id

      if (siteUserId) {
        GetSiteUser(siteUserId)
        .then(data => {
          console.log(data);
          if (data.isSuccess) {
            data.user.password='';
            setCurrentSiteUser(data.user);
            //updateObjects();

          }
          else {
            // Обработка ошибки
          }
        })
        .catch(error => console.log(error));
      }
    }, 
    [siteUserId]);

    //useEffect(() => {
    //    updateObjects();
    //}, []);

    //Обновляем список объектов
    function updateObjects() {
      console.log("updateObjects");
      GetAllObjects()
      .then(data => {
        console.log(data);
        if (data.isSuccess) {
          setObjectsList(data.objects);
        }
        else {
          // Обработка ошибки
        }
      })
      .catch(error => console.log(error));
    }



    function createSiteUser() {

      if(currentSiteUser.login.length < 3){
        setAlertData({message: 'Логин должен содержать действущую почту пользователя', show: true, variant: 'danger'});
        return;
      } else  if(currentSiteUser.password.length < 8){
        setAlertData({message: 'Пароль должен содержать не менее 8 символов', show: true, variant: 'danger'});
        return;
      }
      else 
        setAlertData({show: false, message: '', variant: 'success'});
      
      
      //todo: проверка полей
      let params = {
        login: currentSiteUser.login,
        password: currentSiteUser.password,
        roleCode: currentSiteUser.roleCode,
        objectsList: currentSiteUser.objectsListIds.join(';')
      };
      
      console.log("currentSiteUser", currentSiteUser);

      CreateSiteUserFromApi(params)
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


    function saveSiteUser(){
        //if(currentSiteUser.password.length < 8){
        //setAlertData({message: 'Пароль должен содержать не менее 8 символов', show: true, variant: 'danger'});
        //return;
        //}
        //else 
        setAlertData({show: false, message: '', variant: 'success'});
 
        let params = {
          id: siteUserId,
          login: currentSiteUser.login,
          password: currentSiteUser.password,
          roleCode: currentSiteUser.roleCode,
          objectsList: currentSiteUser.objectsListIds.join(';')
        }

        SaveSiteUserFromApi(params)
        .then(data => {
          console.log(data);

          if (data.isSuccess) {
            // Обработка успешного создания сотрудника
            setAlertData({message: data.message, show: true, variant: 'success'});
            setCreateButtonDisabled(true);
            //обновляем список сотрудников
            updateSiteUsers();
          }
          else {
            setAlertData({message: data.message, show: true, variant: 'danger'});
          }
        })
        .catch(error => console.log(error));
    }

    /*function isCheckboxChecked(id) {

      console.log("currentSiteUser.objectsListIds", currentSiteUser.objectsListIds);
        if (currentSiteUser && currentSiteUser.objectsListIds) {
           return currentSiteUser.objectsListIds.includes(id);
        } else 
          return false;
      }*/

    // Функция для проверки, включён ли чекбокс для объекта с указанным id
    function isCheckboxChecked(id) {
      return currentSiteUser.objectsListIds ? currentSiteUser.objectsListIds.includes(id) : false;
    }

    return (
      <Modal onExit={resetForm} show={show} onHide={onHide} onShow={updateObjects}>
        <Modal.Header closeButton>
          <Modal.Title>
            {
            siteUserId ? 'Редактировать пользователя' : 'Добавить пользователя'
            }
            </Modal.Title>
        </Modal.Header>
       <Modal.Body>
        <Form>
            <Form.Group className="mb-3">
              <Form.Label>Логин(email) пользователя</Form.Label>
              <Form.Control
                type="text"
                readOnly
                value={currentSiteUser.login}
                onChange={(e) => SetCurrentSiteUser({ ...currentSiteUser, login: e.target.value })}
                placeholder="Введите логин"
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Пароль</Form.Label>
              <Form.Control
                type="password"
                value={currentSiteUser.password}
                onChange={(e) => SetCurrentSiteUser({ ...currentSiteUser, password: e.target.value })}
                placeholder="Введите пароль"
              />
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Label>Роль</Form.Label>
              <Form.Select
                value={currentSiteUser.roleCode}
                onChange={(e) => {
                  SetCurrentSiteUser({ ...currentSiteUser, roleCode: e.target.value });
                  //if(roleCode === 'object_manager'){ }
                }}
                placeholder="Выберите роль"
              >
                <option value="admin">Администратор</option>
                <option value="object_manager">Начальник объекта</option>
                <option value="buh">Бухгалтерия</option>
              </Form.Select>
            </Form.Group>
            <div className="table-responsive" style={{maxHeight:"200px"}}>
              
              {
                objectsList ? 
                
              <Table bordered >
                <tbody>
                  {
                    objectsList.map((object) => (
                  <tr key={object.id}>
                  <td>
                    <Form.Check type="checkbox" 
                    label={object.name}
                    checked={isCheckboxChecked(object.id)} 
                    onChange={(e) => {
                    const { checked } = e.target;
                    const { id } = object;

                    const newObjectsList = checked
                      ? [...currentSiteUser.objectsListIds, id]
                      : currentSiteUser.objectsListIds.filter(objId => objId !== id);

                    setCurrentSiteUser({
                      ...currentSiteUser,
                      objectsListIds: newObjectsList
                    });
                        }}  />
                  </td>
                  </tr>
                    ))
                  }
                  
                </tbody>
              </Table>
                : <></>
              }
            </div>

        </Form>

        <Alert show={alertData.show} variant={alertData.variant}>
            {alertData.message}
        </Alert>
       </Modal.Body>
       <Modal.Footer>
          <Button variant="secondary" onClick={onHide}>
            Отмена
          </Button>
            {
            siteUserId ?
            <Button
            onClick={() => saveSiteUser()}
            variant="primary">
              Сохранить</Button>
            : 
            <Button 
            disabled={createButtonDisabled}
            onClick={() => createSiteUser()}
            variant="primary">Добавить</Button>
            }
       </Modal.Footer>
      </Modal>
    )


}