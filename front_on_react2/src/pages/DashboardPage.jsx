import React ,{ useState, useEffect} from 'react';
import { 
  Container, Row, Col, Toast, ToastContainer ,
   Card, Button, 
  Form, Spinner
} from 'react-bootstrap';
import { SaveIcon } from 'lucide-react';

import '../dashboard.css';
//import { MpLayout } from './MpLayout';

import Calendar from 'react-calendar';
//import 'react-calendar/dist/Calendar.css';
import '../calendar.css';
import { ModalForEmployee } from '../components/modal/ModalForEmployee';
import { ModalForWorkShift } from '../components/modal/ModalForWorkShift';
import {GetEmployeeList, GetWorkHoursList, SaveWorkHoursItemOnServer } from '../services/apiService';
import {getDateFormat1} from '../services/commonService';

export function DashboardPage () {
    
    const [showEmpModal, setShowEmpModal] = useState(false);
    const [showShiftsModal, setShowShiftsModal] = useState(false);
    const [employeeId, setEmployeeId] = useState(null);
    const [employeeList, setEmployeeList] = useState([]);
    //const [workShiftsList, setWorkShiftsList] = useState([]);
    const [currentDate, setCurrentDate] = useState(new Date());
    const [workHoursList, setWorkHoursList] = useState({});
    const [savingWorkHours, setSavingWorkHours] = useState(false);
    const [showToastMsg, setShowToastMsg] = useState({
      show: false,
      msg: "sdkfhj skdhk jdfhksdfh",
      variant: "success",
    });


    useEffect(() => {
        updateEmployeeList();
        }
    , []);

    
    /*function onChangeRate(val, employeeId) {
        console.log(val);
        //console.log(/'^[0-9]*$'/.test(val));
        const value = val.replace(/[^\d]/g, '')
        //e.target.value = e.target.value.replace(/[^\d]/g, '');
        
        let newList = {...workShiftsList};

        //const item = workShiftsList[employeeId];
        //console.log({item});
        if(newList[employeeId]){
           newList[employeeId].rate = value;
        }else{
            newList[employeeId] = {
              rate: value,
              hours: 8,
              date: currentDate,
              employeeId: employeeId,
            };
        }

        setWorkShiftsList(newList);

        console.log(workShiftsList);
    }*/




      function GetHours(employeeId) {
        let result = 0;
        if(workHoursList && workHoursList.length > 0){
          let workHoursItem = workHoursList.filter(item => item.employeeId === employeeId)[0];
          if(workHoursItem){
            result = workHoursItem.hours;
          }
        }
        return result;
      }

      function GetRate(employeeId) {
       let result = 0;
       if(workHoursList && workHoursList.length > 0){
          let workHoursItem = workHoursList.filter(item => item.employeeId === employeeId)[0];
          if(workHoursItem){
            result = workHoursItem.rate;
          }
        }
        return result;
      }

      function SetRate(employeeId, rate) {
        if(workHoursList && workHoursList.length > 0){
          let newList = workHoursList.slice();
          //console.log("newList", newList);
          let item = newList.filter(item => item.employeeId === employeeId)[0];
          //console.log("item", item);
          if(item){
            item.rate = rate;
          }else{
            const item2 = {
              rate: rate,
              hours: 8,
              date: currentDate,
              employeeId: employeeId,
            };
            
            newList.push(item2);
          }

          setWorkHoursList(newList);
        }
        else{
          setWorkHoursList([{
            rate: rate,
            hours: 8,
            date: currentDate,
            employeeId: employeeId,
          }]);
        }
      }


      function SetHours(employeeId, hours) {
        
        if(workHoursList && workHoursList.length > 0){
          let newList = workHoursList.slice();
          //console.log("newList", newList);
          let item = newList.filter(item => item.employeeId === employeeId)[0];
          //console.log("item", item);
          if(item){
            item.hours = hours;
          }else{
            const item2 = {
              rate: 0,
              hours: hours,
              date: currentDate,
              employeeId: employeeId,
            };
            
            newList.push(item2);
          }

          setWorkHoursList(newList);
        }
        else{
          setWorkHoursList([{
            rate: 0,
            hours: hours,
            date: currentDate,
            employeeId: employeeId,
          }]);
        }


      }





    function SaveWorkShiftItem(){
        console.log("SaveWorkShiftItem");
    }


    function updateEmployeeList() {

      GetEmployeeList()
        .then((response) => response.json())
        .then((data) => {
            console.log(data);

            if(data.isSuccess){
                setEmployeeList(data.employeesList);
                //теперь скачиваем отработанные часы
                updateWorkHours(currentDate);
            }
        })
        .catch((error) => console.error('Ошибка при получении данных сотрудников:', error));
    }

    function updateWorkHours(date){

        GetWorkHoursList(date)
        .then((response) => response.json())
        .then((data) => {
            console.log(data);

            if(data.isSuccess){
                setWorkHoursList(data.workHoursList);
            }
          })
          .catch((error) => {
            console.error('Ошибка при получении данных отработанных часов:', error);
          });
    }

    function SaveWorkHoursItem(employeeId){
      console.log("SaveWorkHoursItem");

      const params = {
        employeeId: employeeId,
        hours: GetHours(employeeId),
        rate: GetRate(employeeId),
        date: currentDate,
      };
      setSavingWorkHours(true);//анимация
      SaveWorkHoursItemOnServer(params)
      .then((response) => response.json())
      .then((data) => {
        setSavingWorkHours(false);
        console.log(data);
        if(data.isSuccess){
          //?
        }
      })
      .catch((error) => {
        setSavingWorkHours(false);
          console.error('Ошибка при сохранении отработанных часов:', error);
      });

    }

    function ToastShowAndHide(data){
      setShowToastMsg(data);

      setTimeout(() => {
        setShowToastMsg({
          show: false,
          msg: "",
          variant: "",
        });
      }, 1200);
      
    }
  

    function handleEmployeeClick(id) {
        setEmployeeId(id);
        setShowEmpModal(true);
    }


    return (
        <Container expand="lg">
            <br/>
        {/* Заголовок страницы */}
            <div className="d-flex justify-content-between align-items-center mb-4">
              <div>
                <h2 className="mb-1">Сотрудники</h2>
                <p className="text-muted mb-0"></p>
              </div>
              <Button onClick={()=> {setEmployeeId(null); setShowEmpModal(true);}} variant="primary" className="d-flex align-items-center">
                <i className="bi bi-plus-circle me-2"></i>
                Добавить сотрудника
              </Button>
            </div>

            

    <Row>
      <Col lg={3} className="mb-4">
        <Calendar  />
      </Col>
      <Col lg={9} className="mb-4">

      <Card className=" h-100">
                  <Card.Header className="bg-white border-0">
                    <Row>
                      <Col sm={4}> 
                      <Form.Control type='text' placeholder='Поиск по ФИО' />
                      </Col>
                      <Col sm={1}>
                        <Button sm={1} variant="outline-secondary" className="d-flex align-items-center">Поиск</Button>
                      </Col>
                    </Row>
                  </Card.Header>
                  <Card.Body>
                    <div className="table-responsive">
                        {
                    employeeList ? 
                      <table className="table table-bordered table-hover">
                        <thead>
                          <tr>
                            <th rowSpan={2} width="5%">Id</th>
                            <th rowSpan={2} width="30%">Фамилия Имя Отчество</th>
                            <th rowSpan={2} width="10%">Объект</th>
                            <th colSpan={3} width="30%" className='text-center' style={{fontSize:"1.2rem"}}>{getDateFormat1(currentDate)}</th>
                            <th rowSpan={2} width="25%" style={{verticalAlign:"middle"}} >Действия<br/></th>
                          </tr>
                          <tr>
                            <th width="10%">Смена</th>
                            <th width="10%">Отработанно часов</th>
                            <th width="10%">Ставка в час, руб.</th>
                            
                          </tr>
                        </thead>
                        <tbody>
                            {
                                employeeList.map((employee) => {
                                    return (
                                         <tr key={employee.id}>
                                          <td>
                                            {employee.id}
                                          </td>
                                        <td>
                                            <a className='button button-link' href="#" 
                                            onClick={()=> handleEmployeeClick(employee.id)}>
                                            {employee.fio}
                                            </a>
                                          </td>
                                        <td>{employee.object}</td>
                                        <td>
                                        Да
                                        </td>
                                        <td>
                                        <Form.Control 
                                        type='number'
                                        onChange={(e) => {
                                            //const value = e.target.value.replace(/[^\d ]/g, '');
                                            const value = e.target.value;
                                            //if(value == '' || value >= 0){
                                            SetHours(employee.id, value);
                                            //}
                                        }}
                                        min={0}
                                        max={24}
                                        value={GetHours(employee.id)}
                                        
                                        />
                                        </td>
                                        <td>
                                        <Form.Control 
                                        //onChange={(e) => onChangeRate(e.target.value, employee.id)} 
                                        //value={workShiftsList[employee.id]?.rate}
                                        onChange={(e) => {
                                            //const value = e.target.value.replace(/[^\d ]/g, '');
                                            const value = e.target.value;
                                            console.log("value", value);
                                            SetRate(employee.id, value);
                                        }}


                                        value={GetRate(employee.id)}
                                        type='number'
                                        min={0}
                                        max={20000}
                                        />
                                        </td>
                                        <td>
                                        <Row style={{width:"100%"}}>
                                        <Col>
                                        <Button disabled={savingWorkHours} title='Сохранить' onClick={()=>SaveWorkHoursItem(employee.id)} variant="outline-primary" size="sm">
                                        {
                                        savingWorkHours ?
                                        <Spinner animation="border" size="sm"/>
                                        : <SaveIcon />
                                        }
                                        </Button>
                                        </Col>
                                        <Col>
                                        <Button onClick={()=>alert("доп форма")} variant="outline-primary" size="sm">Доп.рабочие часы</Button>
                                        </Col>
                                        </Row>
                                        </td>
                                    </tr>
                                    );
                                })
                            }
                        
                        
                        </tbody>
                      </table>
                      : 
                      <div>Сотрудников не найдено</div>
                        }
                      <div style={{textAlign:"right"}}>
                        

                      </div>
                    </div>
                  </Card.Body>
                </Card>

      </Col>
    </Row>

    <ModalForEmployee employeeId={employeeId} showEmpModal={showEmpModal} setShowEmpModal={setShowEmpModal}  />
    <ModalForWorkShift  showShiftsModal={showShiftsModal} setShowShiftsModal={setShowShiftsModal} updateEmployees={updateEmployeeList}  />
    <ToastMsg data={showToastMsg} setShowToastMsg={setShowToastMsg} />
    <Button 
    onClick={()=> {
      
      ToastShowAndHide({ show: true, variant: "success", msg: "Успешно" })
    }
    }>show toast</Button>
        </Container>
    );
}



function ToastMsg({data, setShowToastMsg}) {
  return (
    <ToastContainer position="top-end" className="p-3" style={{ zIndex: 1 }}>
    <Toast duration={3000} show={data.show} 
    onClose={() => setShowToastMsg({ show: false, variant: "" })}  
    bg={data.variant} >
      <Toast.Header>
        <strong className="me-auto">{
          data.variant === "success" ? "Успешно" : "Ошибка"
          }</strong>
      </Toast.Header>
      <Toast.Body>

        <p>
          {data.msg}
        </p>
      </Toast.Body>
       
    </Toast>
    </ToastContainer>
  );
}
