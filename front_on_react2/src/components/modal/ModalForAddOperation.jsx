import React, {useState, useEffect} from 'react';
import {
  Row, 
  Col, 
  Button, 
  Modal,
  Form,
  Alert
} from 'react-bootstrap';

import {GetEmployee, CreateFinOperationFromApi
    , DeleteFinOperationFromApi
  } from '../../services/apiService';


//{/* Модальное окно добавления/редактирования сотрудника */}

export function ModalForAddOperation({employeeId, 
                        showOperationsModal, 
                        setShowOperationsModal, 
                        selectedDate, updateTable}) {

    const [currentEmployee, setCurrentEmployee] = useState({fio:''});
    const [penaltySelected, setPenaltySelected] = useState(false);//если true, то штраф
    const [sum, setSum] = useState(0);
    const [comment, setComment] = useState('');
    const [disableSaveButton, setDisableSaveButton] = useState(false);
    const [alertData, setAlertData] = useState({
        show: false,
        message: '',
        variant: 'danger'
    });



    function GetEmployeeOnShow(){

    //console.log("GetEmployeeOnShow");
    //console.log("employeeId", employeeId);

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
    }


    //сохранить результатфинансовой операции
    function saveFinOperation(){
        setAlertData({show: false});
        if(currentEmployee){
            console.log("saveFinOperation");
            console.log(currentEmployee);

            let params = {
                EmployeeId: employeeId,
                Sum: sum,
                Comment: comment,
                IsPenalty: penaltySelected,
                Date: selectedDate
            };

            //console.log("params", params);

            CreateFinOperationFromApi(params)
            .then(data => {
                console.log(data);
                if (data.isSuccess) {
                    // Обработка успешного сохранения
                    setAlertData({show: true, message: data.message, variant: "success"});   
                    updateTable();
                    setDisableSaveButton(true);
                }
                else {
                    // Обработка ошибки
                    setAlertData({show: true, message: data.message, variant: "danger"});   
                }
            })
            .catch(error => {
                console.log(error);
                setAlertData({show: true, message: error.message, variant: "danger"});
            }); 

        }
    }

    function resetFormData(){
        setDisableSaveButton(false);
        setSum(0);
        setComment('');
    }


    return (
        <Modal 
        show={showOperationsModal} 
        onHide={() => setShowOperationsModal(false)}
        onShow={() => {GetEmployeeOnShow(); resetFormData();}}>
            <Modal.Header closeButton>
                <Modal.Title>Начисления и списания для:<br/>
                    {
                    currentEmployee ? 
                    currentEmployee.fio : null
                }</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Form>
                    <Row>

                    
                    <Form.Group as={Col} md={6} className='mb-3' >
                        <Form.Label>Тип операции</Form.Label>
                        <Form.Check style={{fontSize:"1.2rem"}}
                        onChange={() => setPenaltySelected(false)}
                        checked={!penaltySelected}
                        type="radio"
                        label="Начисление"
                        />
                        <Form.Check style={{fontSize:"1.2rem"}}
                        onChange={(e) => setPenaltySelected(true)}
                        checked={penaltySelected}
                        type="radio"
                        label="Штраф"
                        />
                    </Form.Group>


                    <Form.Group as={Col} md={6} className='mb-3' >
                        <Form.Label>Сумма</Form.Label>
                        <Form.Control type="number" 
                        value={sum}
                        onChange={e=>setSum(e.target.value)} 
                        placeholder="" />
                    </Form.Group>
                    </Row>
                        <Form.Group  className='mb-3'>
                        <Form.Label>Комментарий (если нужно)</Form.Label>
                        <Form.Control
                        value={comment}
                        onChange={e=>setComment(e.target.value)}
                        type="text" />
                    </Form.Group>
                    
                </Form>
                <Alert show={alertData.show} dismissible onClose={() => setAlertData({show: false})}  variant={alertData.variant}>
                    {alertData.message}
                </Alert>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={() => setShowOperationsModal(false)}>Закрыть</Button>
                <Button disabled={disableSaveButton} variant="primary" onClick={saveFinOperation}
                type="button">Сохранить</Button>
            </Modal.Footer>
        </Modal>
    )

}


