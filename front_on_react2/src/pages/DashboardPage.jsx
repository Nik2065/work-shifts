import React ,{ useState, useEffect} from 'react';
import { 
  Container, Row, Col, Toast, Table, ToastContainer ,
   Card, Button, 
  Form, Spinner
} from 'react-bootstrap';
import { SaveIcon, Trash2 } from 'lucide-react';
import '../dashboard.css';
import '../calendar.css';
import { ModalForEmployee } from '../components/modal/ModalForEmployee';
import { ModalForWorkShift } from '../components/modal/ModalForWorkShift';
import { ModalForAddOperation } from '../components/modal/ModalForAddOperation';
import {GetWorkHoursList, 
  SaveWorkHoursItemOnServer, GetAllObjects,
  GetEmployeeWithFinOpListFromApi,
  DeleteFinOperationFromApi
} from '../services/apiService';
import {getDateFormat1} from '../services/commonService';
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import { registerLocale, setDefaultLocale } from  "react-datepicker";
import { ru } from 'date-fns/locale/ru';
registerLocale('ru', ru)



export function DashboardPage () {
    
    const [showEmpModal, setShowEmpModal] = useState(false);
    const [showShiftsModal, setShowShiftsModal] = useState(false);
    const [employeeId, setEmployeeId] = useState(null);
    const [employeeList, setEmployeeList] = useState([]);
    const [currentDate, setCurrentDate] = useState(new Date());
    const [workHoursList, setWorkHoursList] = useState({});
    const [savingWorkHours, setSavingWorkHours] = useState(false);
    const 
    [showToastMsg, setShowToastMsg] = useState({
      show: false,
      msg: "",
      variant: "success",
    });
    const [objectsList, setObjectsList] = useState([]);
    const [selectedObject, setSelectedObject] = useState(-1);
    const [fioToSearch, setFioToSearch] = useState("");
    const [showOperationsModal, setShowOperationsModal] = useState(false);
    const [savingWorkHoursEmplId, setSavingWorkHoursEmplId] = useState(null);
    //фильтр для таблицы //на вахте
    const [isInWorkShift, setIsInWorkShift] = useState(-1);

    useEffect(() => {
        //updateEmployeeList();
        updateEmployeeListAndFinOperations();
        }
    , []);

    useEffect(() => {
        updateObjects();
        }
    , []);
    
    
    //Обновляем список объектов
    function updateObjects() {
      console.log("updateObjects");
      GetAllObjects()
      .then(data => {
        console.log(data);
        if (data.isSuccess) {
          setObjectsList(data.objects);
          //устанавливаем айдишник для пользователя 
          //if(data.objects.length>0)
          //  setSelectedObject(data.objects[0].id)
        }
        else {
          // Обработка ошибки
        }
      })
      .catch(error => console.log(error));
    }



      //список отработанных часов
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

      //меняем ставку в час на форме
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

    //обновляем таблицу с сотрудниками и фин операциями
    //финансовые операции загружаются только на выбранную дату
    function updateEmployeeListAndFinOperations() {
      
      const params = {
        date: currentDate,
        objectId: selectedObject,
        isInWorkShift: isInWorkShift,
      }

      GetEmployeeWithFinOpListFromApi(params)
        .then((data) => {
            console.log(data);

            if(data.isSuccess){
              let empList = data.employeesList;

                //if(selectedObject && selectedObject != -1){
                //  empList = empList.filter(item => item.objectId == selectedObject);
                //}
                if(fioToSearch && fioToSearch.length > 0){
                  empList = empList.filter(item => item.fio.toLowerCase().includes(fioToSearch.toLowerCase()));
                }

                setEmployeeList(empList);
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

    //отображаем финансовые операции в таблице
    //при смене даты
    function updateFinOperations(){
      
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
          ToastShowAndHide({show: true, msg: data.message, variant: "success"});  
        }
        else {
           ToastShowAndHide({show: true, msg: data.message, variant: "danger"});
        }
      })
      .catch((error) => {
        setSavingWorkHours(false);
          ToastShowAndHide({show: true, msg: "Ошибка при сохранении отработанных часов", variant: "danger"});
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
      }, 2000);
      
    }
  
    function onDateChange(date){
      if(date){
        setCurrentDate(date);
        //updateWorkHours(date);
        //updateEmployeeListAndFinOperations();
      }
    }

    function onObjectChange(objId){
      setSelectedObject(objId);
      //фильтруем список сотрудников
      //updateEmployeeList();
    }
    
    function handleEmployeeClick(id) {
        setEmployeeId(id);
        setShowEmpModal(true);
    }

    function deleteFinOperation(id){
        console.log("deleteFinOperation");
        console.log(id);
        const params = {
          OperationId: id
        };
        DeleteFinOperationFromApi(params)
        .then(data => {
          console.log(data);
          if (data.isSuccess) {
            // Обработка успешного удаления
            ToastShowAndHide({show: true, msg: data.message, variant: "success"});  
            updateEmployeeListAndFinOperations();
          }
          else {
            // Обработка ошибки
            ToastShowAndHide({show: true, msg: data.message, variant: "danger"});  
          }
        })
        .catch(error => console.log(error));
    }



    return (
        <Container expand="lg">
            <br/>
        {/* Заголовок страницы */}
            <div className="d-flex justify-content-between align-items-center mb-4">
              <div>
                <h2 className="mb-1">Учет времени</h2>
                <p className="text-muted mb-0"></p>
              </div>
              <Button onClick={()=> {setEmployeeId(null); setShowEmpModal(true);}} variant="primary" className="d-flex align-items-center">
                <i className="bi bi-plus-circle me-2"></i>
                Добавить сотрудника
              </Button>
            </div>

            

    <Row>
      <Col lg={12} className="mb-4">

      <Card className=" h-100">
                  <Card.Header className="bg-white border-0">
                    <Row className="align-items-end">
                      <Form.Group as={Col} sm={2}>
                        <Form.Label>Дата &nbsp;</Form.Label><br/>
                        <DatePicker style={{width: "100%"}} 
                        className='form-control' locale="ru" 
                        selected={currentDate} 
                        onChange={(date) => {
                          onDateChange(date);
                          }} />

                      </Form.Group>

                      <Form.Group as={Col} sm={2}>
                        <Form.Label>На вахте &nbsp;</Form.Label><br/>

                          <Form.Select 
                          value={isInWorkShift}
                          onChange={e => setIsInWorkShift(e.target.value)}>
                              <option value={-1}>Все</option>
                              <option value={1}>Да</option>
                              <option value={2}>Нет</option>
                          </Form.Select>


                      </Form.Group>

                      <Form.Group as={Col} sm={2}>
                      <Form.Label>Объект &nbsp;</Form.Label>
                      <Form.Select

                        value={selectedObject}
                        onChange={(e) => onObjectChange(e.target.value)}
                        placeholder="Выберите объект"
                      >
                        <option key={-1} value={-1}>Все</option>

                        {
                          objectsList ?
                          objectsList.map(obj => (
                            <option key={obj.id} value={obj.id}>{obj.name}</option>
                          ))
                          : null
                        }
                      </Form.Select>
                      </Form.Group>
                      


                      <Form.Group as={Col} sm={4}>
                      <Form.Label>Поиск по имени &nbsp;</Form.Label>
                      <Form.Control 
                      value={fioToSearch}
                      onChange={(e) => setFioToSearch(e.target.value)}
                      type='text' 
                      placeholder='ФИО' />
                      </Form.Group>


                      <Form.Group as={Col} sm={2} style={{textAlign:"right"}}>
                        
                      <Button 
                      onClick={updateEmployeeListAndFinOperations}
                      variant="primary" 
                      className="">Показать</Button>
                      </Form.Group>

                    </Row>
                  </Card.Header>
                  <Card.Body>
                    <br/>
                    <div className="table-responsive">
                        {
                    employeeList ? 
                      <table className="table table-bordered table-hover">
                        <thead>
                          <tr>
                            <th rowSpan={2} width="5%"><strong>Id</strong></th>
                            <th rowSpan={2} width="30%"><strong>Фамилия Имя Отчество</strong></th>
                            <th rowSpan={2} width="10%"><strong>Объект</strong></th>
                            <th colSpan={3} width="30%" className='text-center' style={{fontSize:"1.2rem"}}>{getDateFormat1(currentDate)}</th>
                            <th colSpan={2} rowSpan={2} width="25%" style={{verticalAlign:"middle"}} ><strong>Действия</strong><br/></th>
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
                                      <>
                                         <tr key={employee.id.toString() + 'a'}>
                                          <td>
                                            {employee.id}
                                          </td>
                                        <td>
                                            <a className='button button-link' style={{fontSize:"1.1rem"}} href="#" 
                                            onClick={()=> handleEmployeeClick(employee.id)}>
                                            {employee.fio}
                                            </a>
                                            {
                                              employee.isInWorkShift ?
                                              <> <br/>
                                              <span style={{fontSize:"0.9rem"}} className='badge bg-success'> 
                                                {
                                                  getDateFormat1(new Date(employee.workShiftStart))
                                                }
                                                &nbsp;-&nbsp;
                                                {getDateFormat1(new Date(employee.workShiftEnd))}
                                              </span>
                                              </>
                                              : null
                                            }
                                          </td>
                                        <td>{employee.objectName}</td>
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
                                            if(value >= 0 && value <= 24){
                                                SetHours(employee.id, value);
                                            }
                                            else{
                                                ToastShowAndHide({show: true, msg: "Часов должно быть в диапазоне от 0 до 24", variant: "danger"});
                                            }
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
                                            //console.log("value", value);
                                            if(value >= 0 && value <= 10000){
                                              SetRate(employee.id, value);
                                            }
                                            else{
                                                ToastShowAndHide({show: true, msg: "Ставка должна быть в диапазоне от 0 до 10000", variant: "danger"});
                                            }
                                        }}


                                        value={GetRate(employee.id)}
                                        type='number'
                                        min={0}
                                        max={10000}
                                        />
                                        </td>
                                        
                                        <td>
 <Button 
                                        disabled={savingWorkHours && savingWorkHoursEmplId == employee.id } 
                                        title='Сохранить' 
                                        onClick={()=>{setSavingWorkHoursEmplId(employee.id);  SaveWorkHoursItem(employee.id);}} variant="outline-primary" size="sm">
                                        {
                                        savingWorkHours && savingWorkHoursEmplId == employee.id ? 
                                        <Spinner animation="border" size="sm"/>
                                        : <SaveIcon />
                                        }
                                        </Button>
                                        </td>
                                        <td>
                                        <Button onClick={()=> {
                                          setShowOperationsModal(true);
                                          setSavingWorkHoursEmplId(employee.id); }}
                                           variant="outline-primary" size="sm">Начисл./Списания</Button>
      
                                        </td>
                                    </tr>
                                    {
                                      employee.finOperations ?
                                      <tr key={employee.id.toString() + "b"}>
                                              <td  colSpan="8">
                                                <FinTable operations={employee.finOperations} deleteFinOperation={deleteFinOperation} />
                                                </td>
                                      </tr>
                                      : null 

                                      //конец цикла map
                                    }
                                    </>
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

    <ModalForEmployee 
      employeeId={employeeId} 
      showEmpModal={showEmpModal} 
      setShowEmpModal={setShowEmpModal} />
    
    <ModalForWorkShift 
       employeeId={savingWorkHoursEmplId} 
       showShiftsModal={showShiftsModal} 
       setShowShiftsModal={setShowShiftsModal} 
       updateEmployees={updateEmployeeListAndFinOperations}  />
    
    <ModalForAddOperation 
        employeeId={savingWorkHoursEmplId}
        showOperationsModal={showOperationsModal} 
        setShowOperationsModal={setShowOperationsModal}
        selectedDate={currentDate}
        updateTable={updateEmployeeListAndFinOperations}
    />

    <ToastMsg 
      data={showToastMsg} 
      setShowToastMsg={setShowToastMsg} />


    </Container>
    );
}


function FinTable({operations, deleteFinOperation}){
  return (
    <Table className="p-1" bordered style={{fontSize:"0.8rem", }}>
      <tbody>
        
        {
          operations.map((op) => 
            
            (
            <tr  className="p-0" key={op.id.toString() + "fin"}>
              
              <td className="py-0" style={{backgroundColor:op.isPenalty ? "#f9d5e5" : "#96ceb4"}}
              >{op.isPenalty ? "Списание" : "Начисление"}</td>
              <td className="py-0">Сумма: {op.sum}</td>
              <td className="py-0">Комментарий: {op.comment}</td>
              <td className="py-0" style={{textAlign:"center"}}>
                <Button style={{fontSize:"0.4rem", color:"lightgrey"}} size="sm" variant="link" onClick={()=>deleteFinOperation(op.id)}>
                  <Trash2  />
                </Button>
              </td>
            </tr>
          ))
        }
      </tbody>
    </Table>
  )
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
        <p style={{color:"white", fontSize:"1.1rem"}}>
          {data.msg}
        </p>
      </Toast.Body>
       
    </Toast>
    </ToastContainer>
  );
}
