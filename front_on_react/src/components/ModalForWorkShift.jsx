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
    const [dates1, setDates1] = useState([])
    const [dates2, setDates2] = useState([])
    const [dates3, setDates3] = useState([])


    useEffect(() => {
      //setDates(GetDates())
      setDates1(GetDates(0, 10));
      setDates2(GetDates(10, 20));
      setDates3(GetDates(20, 30));
    }, [])



    function GetDates(start, end){
      let arr = []
      for (let i = start; i < end; i++) {
        let d = new Date();
        d.setDate(new Date().getDate() + i);
        arr.push(d);
      }
      return arr;
    }

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
                <th width="20%">Дата</th>
                <th width="13%">Смена</th>
                <th width="20%">Дата</th>
                <th width="13%">Смена</th>
                <th width="20%">Дата</th>
                <th width="13%">Смена</th>
              </tr>
            </thead>
            <tbody>
              {
                
                 dates1.map((date, i) => {
                  return (
                  <tr>
                  <td>
                    {dates1[i].getDate() + '.' + (dates1[i].getMonth() + 1) 
                    + '.' + dates1[i].getFullYear() + " " 
                    + GetDayOfWeek(dates1[i].getDay())}
                  </td>
                  <td>
                    <Form.Check type='checkbox' style={{fontSize:"1.2rem"}}>
                    </Form.Check>
                  </td>
                  <td>
                    {dates2[i].getDate() + '.' + (dates2[i].getMonth() + 1) 
                    + '.' + dates2[i].getFullYear() + " " 
                    + GetDayOfWeek(dates2[i].getDay())}
                  </td>
                  <td>
                    <Form.Check type='checkbox' style={{fontSize:"1.2rem"}}>
                    </Form.Check>
                  </td>
                  <td>
                    {dates3[i].getDate() + '.' + (dates3[i].getMonth() + 1) 
                    + '.' + dates3[i].getFullYear() + " " 
                    + GetDayOfWeek(dates3[i].getDay())}
                  </td>
                  <td>
                    <Form.Check type='checkbox' style={{fontSize:"1.2rem"}}>
                    </Form.Check>
                  </td>

                  </tr>
                )
              })
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