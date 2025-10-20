import React, {useState, useEffect} from 'react';

import {
  Container, 
  Row, 
  Col, 
  Card, 
  Button, 
  Table,
  Modal,
  Form
} from 'react-bootstrap';

//{/* Модальное окно добавления/редактирования сотрудника */}

export function ModalForWorkShift({showShiftsModal, setShowShiftsModal}) {

    //имя сотрудника
    //const [newEmployee, SetNewEmployee] = useState({name:'', age:30, chop:false})
    //const [dates1, setDates1] = useState([])


    useEffect(() => {
      //setDates(GetDates())
      //setDates1(GetDates(0, 10));
    }, [])



    /*function GetDates(start, end){
      let arr = []
      for (let i = start; i < end; i++) {
        let d = new Date();
        d.setDate(new Date().getDate() + i);
        arr.push(d);
      }
      return arr;
    }*/

    function GetDayOfWeek(day){
      switch(day){
        case 0: return 'Воскресенье';
        case 1: return 'Понедельник';
        case 2: return 'Вторник';
        case 3: return 'Среда';
        case 4: return 'Четверг';
        case 5: return 'Пятница';
        case 6: return 'Суббота';
      }
    }

    return (
        <Modal size='lg' show={showShiftsModal} onHide={() => setShowShiftsModal(false)}>
            <Modal.Header closeButton>
            <Modal.Title>
                Редактирование смен
            {
            //editingEmployee ? 'Редактировать сотрудника' : 'Добавить сотрудника'
            }
          </Modal.Title>
        </Modal.Header>
        <Modal.Body>
              <div className='table-responsive'>
            <Table bordered hover>
            <thead>
              <tr>
                <th width="40%">Дата начала вахты</th>
                <th width="40%">Дата конца вахты</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {
                

              }
            
            </tbody>
            </Table>
            </div>

        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setcho(false)}>
            Отмена
          </Button>
          <Button 
            variant="primary" 
            
          >
            Сохранить
        </Button>
        </Modal.Footer>
        </Modal>
    )
    
  

}