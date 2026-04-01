import React ,{ useState, useEffect, useMemo } from 'react';
import { 
  Container, Row, Col, Toast, Table, ToastContainer ,
   Card, Button, 
  Form, Spinner,
  FormControl
} from 'react-bootstrap';
import { SaveIcon, Trash2, RussianRuble, Check, FormInput  } from 'lucide-react';

import '../dashboard.css';
import '../calendar.css';
import { ModalForEmployee } from '../components/modal/ModalForEmployee';
import { ModalForWorkShift } from '../components/modal/ModalForWorkShift';
import { ModalForAddOperation } from '../components/modal/ModalForAddOperation';
import {GetWorkHoursList, 
  SaveWorkHoursItemOnServer, GetAllObjects,
  GetEmployeeWithFinOpListFromApi,
  DeleteFinOperationFromApi,
  SetEmployeePayout
} from '../services/apiService';
import {getDateFormat1} from '../services/commonService';
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import { registerLocale, setDefaultLocale } from  "react-datepicker";
import { ru } from 'date-fns/locale/ru';
//import {converDateToIsoStringWithTimeZone} from '../services/commonService';
registerLocale('ru', ru)



export function DashboardPage () {

    const [showEmpModal, setShowEmpModal] = useState(false);
    const [showShiftsModal, setShowShiftsModal] = useState(false);
    const [employeeId, setEmployeeId] = useState(null);
    const [employeeList, setEmployeeList] = useState([]);
    const [currentDate, setCurrentDate] = useState(new Date());
    const [tempDate, setTempDate] = useState(new Date());

    const [workHoursList, setWorkHoursList] = useState([]);
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
    const [savingAllWorkHours, setSavingAllWorkHours] = useState(false);
    const [loadingMainTable, setLoadingMainTable] = useState(false);
    //фильтр для таблицы //на вахте
    const [isInWorkShift, setIsInWorkShift] = useState(-1);

    // Обновление таблицы при изменении фильтров: дата, на вахте, объект (поиск по имени — только фильтрация без запроса)
    useEffect(() => {
        setCurrentDate(tempDate);
        updateEmployeeListAndFinOperations(tempDate);
    }, [tempDate, isInWorkShift, selectedObject]);

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



      function GetWorkItem(employeeId) {
        if (!workHoursList || workHoursList.length === 0) return null;
        return workHoursList.find(item => item.employeeId === employeeId) || null;
      }

      function GetCompensationType(employeeId) {
        const workItem = GetWorkItem(employeeId);
        if (!workItem) return "hourly";
        return workItem.compensationType === "daily" ? "daily" : "hourly";
      }

      //список отработанных часов
      function GetHours(employeeId) {
        const workHoursItem = GetWorkItem(employeeId);
        return workHoursItem?.hours ?? 0;
      }

      function GetRate(employeeId) {
        const workHoursItem = GetWorkItem(employeeId);
        return workHoursItem?.rate ?? 0;
      }

      function GetDayRate(employeeId) {
        const workHoursItem = GetWorkItem(employeeId);
        return workHoursItem?.dayRate ?? 0;
      }

      function GetSavePayload(employeeId) {
        const compensationType = GetCompensationType(employeeId);
        const dayRate = GetDayRate(employeeId);
        const hourlyRate = GetRate(employeeId);
        return {
          employeeId: employeeId,
          hours: GetHours(employeeId),
          rate: hourlyRate,
          dayRate: compensationType === "daily" ? (dayRate > 0 ? dayRate : hourlyRate) : dayRate,
          compensationType: compensationType,
          date: currentDate,
        };
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
            item.compensationType = "hourly";
          }else{
            const item2 = {
              rate: rate,
              hours: 8,
              date: currentDate,
              employeeId: employeeId,
              compensationType: "hourly",
              dayRate: 0,
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
            compensationType: "hourly",
            dayRate: 0,
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
            item.compensationType = "hourly";
          }else{
            const item2 = {
              rate: 0,
              hours: hours,
              date: currentDate,
              employeeId: employeeId,
              compensationType: "hourly",
              dayRate: 0,
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
            compensationType: "hourly",
            dayRate: 0,
          }]);
        }
      }

      function SetDayRate(employeeId, dayRate) {
        if(workHoursList && workHoursList.length > 0){
          let newList = workHoursList.slice();
          let item = newList.filter(item => item.employeeId === employeeId)[0];
          if(item){
            item.dayRate = dayRate;
            item.compensationType = "daily";
          }else{
            newList.push({
              rate: 0,
              hours: 0,
              dayRate: dayRate,
              date: currentDate,
              employeeId: employeeId,
              compensationType: "daily",
            });
          }
          setWorkHoursList(newList);
        } else {
          setWorkHoursList([{
            rate: 0,
            hours: 0,
            dayRate: dayRate,
            date: currentDate,
            employeeId: employeeId,
            compensationType: "daily",
          }]);
        }
      }

      function SetCompensationType(employeeId, compensationType) {
        const isDaily = compensationType === "daily";
        if(workHoursList && workHoursList.length > 0){
          let newList = workHoursList.slice();
          let item = newList.filter(i => i.employeeId === employeeId)[0];
          if(item){
            item.compensationType = compensationType;
            if (isDaily) {
              item.hours = 0;
              item.dayRate = item.dayRate > 0 ? item.dayRate : (item.rate ?? 0);
              item.rate = 0;
            } else {
              item.dayRate = 0;
              item.hours = item.hours ?? 8;
              item.rate = item.rate ?? 0;
            }
          } else {
            newList.push({
              employeeId: employeeId,
              date: currentDate,
              compensationType: compensationType,
              hours: isDaily ? 0 : 8,
              rate: 0,
              dayRate: 0,
            });
          }
          setWorkHoursList(newList);
          return;
        }

        setWorkHoursList([{
          employeeId: employeeId,
          date: currentDate,
          compensationType: compensationType,
          hours: isDaily ? 0 : 8,
          rate: 0,
          dayRate: 0,
        }]);
      }

    //обновляем таблицу с сотрудниками и фин операциями
    //финансовые операции загружаются только на выбранную дату
    function updateEmployeeListAndFinOperations(dateToCalc) {
      const date = dateToCalc ?? currentDate;
      setLoadingMainTable(true);

      const params = {
        date: date,
        objectId: Number(selectedObject),
        isInWorkShift: Number(isInWorkShift),
      }

      GetEmployeeWithFinOpListFromApi(params)
        .then((data) => {
            console.log(data);
            setLoadingMainTable(false);

            if(data.isSuccess){
              let empList = data.employeesList || [];
                setEmployeeList(empList);
                //теперь скачиваем отработанные часы
                updateWorkHours(date);
            }
        })
        .catch((error) => {
          setLoadingMainTable(false);
          console.error('Ошибка при получении данных сотрудников:', error);
        });
    }

    function updateWorkHours(date){

        GetWorkHoursList(date)
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

      const params = GetSavePayload(employeeId);

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

    function SaveAllWorkHoursItems() {
      const list = displayedEmployeeList || [];
      if (list.length === 0) {
        ToastShowAndHide({ show: true, msg: "Нет сотрудников для сохранения", variant: "warning" });
        return;
      }
      setSavingAllWorkHours(true);
      const promises = list.map((emp) =>
        SaveWorkHoursItemOnServer(GetSavePayload(emp.id)).then((r) => r.json()).then((data) => ({ id: emp.id, ok: data.isSuccess, msg: data.message }))
          .catch(() => ({ id: emp.id, ok: false, msg: "Ошибка сети" }))
      );
      Promise.all(promises).then((results) => {
        setSavingAllWorkHours(false);
        const okCount = results.filter((r) => r.ok).length;
        const failCount = results.filter((r) => !r.ok).length;
        if (failCount === 0) {
          ToastShowAndHide({ show: true, msg: `Сохранено для ${okCount} сотрудников`, variant: "success" });
        } else {
          ToastShowAndHide({ show: true, msg: `Сохранено: ${okCount}, ошибок: ${failCount}`, variant: "danger" });
        }
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

        //setCurrentDate(date);
        setTempDate(date);
        //updateWorkHours(date);
        //updateEmployeeListAndFinOperations();
      }
    }

    function onObjectChange(objId){
      setSelectedObject(Number(objId));
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

    const handleFocus = (e) => {
      e.target.select();
    };

    // Фильтрация по ФИО без повторного запроса к API
    const displayedEmployeeList = useMemo(() => {
      if (!fioToSearch || !fioToSearch.trim()) return employeeList || [];
      const q = fioToSearch.trim().toLowerCase();
      return (employeeList || []).filter(item => (item.fio || '').toLowerCase().includes(q));
    }, [employeeList, fioToSearch]);

    return (
        <Container expand="lg">
            <br/>
        {/* Заголовок страницы */}
            <div className="d-flex justify-content-between align-items-center mb-4">
              <div>
                <h2 className="mb-1">Учет времени</h2>
                <p className="text-muted mb-0"></p>
              </div>
            </div>

            

    <Row>
      <Col lg={12} className="mb-4">

      <Card className=" h-100">
                  <Card.Header className="bg-white border-0">
                    <Row className="align-items-end">
                      <Form.Group as={Col} sm={2}>
                        <Form.Label>Дата &nbsp;</Form.Label><br/>
                        <DatePicker style={{width: "100%"}} 
                        className='form-control'
                        //utcOffset={offset} 
                        locale={ru}
                        dateFormat="dd/MM/yyyy"
                        selected={tempDate} 
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
                      


                      <Form.Group as={Col} sm={2}>
                      <Form.Label>Поиск по имени &nbsp;</Form.Label>
                      <Form.Control 
                      value={fioToSearch}
                      onChange={(e) => setFioToSearch(e.target.value)}
                      type='text' 
                      placeholder='ФИО' />
                      </Form.Group>


                      <Form.Group as={Col} sm={2} style={{textAlign:"right"}}>
                        
                      <Button 
                      onClick={() => {
                        setCurrentDate(tempDate);
                        updateEmployeeListAndFinOperations(tempDate);
                      }}
                      variant="primary" 
                      className="">Показать</Button>
                      </Form.Group>

                      <Form.Group as={Col} sm={2} style={{textAlign:"right"}}>
                      <Button 
                      disabled={savingAllWorkHours || (displayedEmployeeList && displayedEmployeeList.length === 0)}
                      onClick={SaveAllWorkHoursItems}
                      variant="outline-success"
                      title="Сохранить часы и ставки для всех отображаемых сотрудников">
                      {savingAllWorkHours ? <><Spinner animation="border" size="sm" className="me-1"/> Сохранение...</> : <>Сохранить все</>}
                      </Button>
                      </Form.Group>

                    </Row>
                  </Card.Header>
                  <Card.Body>
                    <br/>
                    <div className="table-responsive" style={{minHeight:"100px"}}>
                        {
                    loadingMainTable ?
                    <div style={{textAlign:"center"}}>
                      <Spinner size='xl' />
                    </div>
                    :
                    employeeList ? 
                      <table className="table table-bordered table-hover">
                        <thead>
                          <tr>
                            <th rowSpan={2} width="5%"><strong>Id</strong></th>
                            <th rowSpan={2} width="30%"><strong>Фамилия Имя Отчество</strong></th>
                            <th rowSpan={2} width="5%"> На выплаты
                            <i title='На выплаты'>  <RussianRuble /></i>
                            
                            </th>
                            <th rowSpan={2} width="10%"><strong>Объект</strong></th>
                            <th colSpan={4} width="40%" className='text-center' style={{fontSize:"1.2rem"}}>{getDateFormat1(currentDate)}</th>
                            <th colSpan={2} rowSpan={2} width="25%" style={{verticalAlign:"middle"}} ><strong>Действия</strong><br/></th>
                          </tr>
                          <tr>
                            <th width="10%">Вахта</th>
                            <th width="10%">Тип оплаты</th>
                            <th width="10%">Отработанно часов</th>
                            <th width="10%">Ставка / день, руб.</th>
                            
                          </tr>
                        </thead>
                        <tbody>
                            {
                                displayedEmployeeList.map((employee) => {
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
                                          <td>
                                            <Form.Check
                                              title='На выплату'
                                              checked={!!employee.payout}
                                              onChange={(e) => {
                                                const checked = e.target.checked;
                                                SetEmployeePayout(employee.id, checked)
                                                  .then((data) => {
                                                    if (data.isSuccess) {
                                                      setEmployeeList(prev => prev.map(emp =>
                                                        emp.id === employee.id ? { ...emp, payout: checked } : emp
                                                      ));
                                                      ToastShowAndHide({ show: true, msg: data.message || 'Сохранено', variant: 'success' });
                                                    } else {
                                                      ToastShowAndHide({ show: true, msg: data.message || 'Ошибка', variant: 'danger' });
                                                    }
                                                  })
                                                  .catch(() => ToastShowAndHide({ show: true, msg: 'Ошибка при сохранении', variant: 'danger' }));
                                              }}
                                            />
                                          </td>
                                        <td>{employee.objectName}</td>
                                        <td>
                                        {
                                          employee.isInWorkShift ? "Да" : "Нет"
                                        }
                                        </td>
                                        <td>
                                          <Form.Select
                                            value={GetCompensationType(employee.id)}
                                            onChange={(e) => SetCompensationType(employee.id, e.target.value)}
                                          >
                                            <option value="hourly">Ставка в час + часы</option>
                                            <option value="daily">Стоимость за день</option>
                                          </Form.Select>
                                        </td>
                                          {
                                            //Столбец: Отработанно часов
                                          }
                                        <td>

                                        <Form.Control 
                                        type='number'
                                        disabled={GetCompensationType(employee.id) === "daily"}
                                        onFocus={(e) => handleFocus(e)}
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
                                        value={GetCompensationType(employee.id) === "daily" ? 0 : GetHours(employee.id)}
                                        
                                        />
                                        </td>
                                          {
                                            //Столбец: Ставка в час, руб.
                                          }
                                        <td>
                                        <Form.Control 
                                        onFocus={(e) => handleFocus(e)}
                                        //onChange={(e) => onChangeRate(e.target.value, employee.id)} 
                                        //value={workShiftsList[employee.id]?.rate}
                                        onChange={(e) => {
                                            //const value = e.target.value.replace(/[^\d ]/g, '');
                                            const value = e.target.value;
                                            //console.log("value", value);
                                            if(value >= 0 && value <= 10000){
                                              if (GetCompensationType(employee.id) === "daily") {
                                                SetDayRate(employee.id, value);
                                              } else {
                                                SetRate(employee.id, value);
                                              }
                                            }
                                            else{
                                                ToastShowAndHide({show: true, msg: "Ставка должна быть в диапазоне от 0 до 10000", variant: "danger"});
                                            }
                                        }}


                                        value={GetCompensationType(employee.id) === "daily" ? GetDayRate(employee.id) : GetRate(employee.id)}
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
                                              <td  colSpan="10">
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
                            {displayedEmployeeList.length === 0 && (
                              <tr>
                                <td colSpan={10} className="text-center text-muted py-4">
                                  {employeeList.length === 0 ? 'Сотрудников не найдено' : 'По заданным фильтрам сотрудников не найдено'}
                                </td>
                              </tr>
                            )}
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
              <td>Тип: {op.typeName}</td>
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
