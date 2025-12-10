import React, {useState, useEffect} from "react";
import { apiUrl } from "../services/const"; 

import { 
  Container, Row, Col, 
   Card, Button, 
  Spinner, Form, Table,
  FormGroup
} from 'react-bootstrap';
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import {GetEmployeeList, 
    GetMainReportForPeriodAsTable, 
    GetAllObjects, 
    DownloadFileWithAuth} from '../services/apiService';

import { registerLocale, setDefaultLocale } from  "react-datepicker";
import { ru } from 'date-fns/locale/ru';
registerLocale('ru', ru)

export function ReportForEmployesListPage() {

    const [employeeList, setEmployeeList] = useState([]);
    const [startDate, setStartDate] = useState(new Date());
    const [endDate, setEndDate] = useState(new Date());
    const [updateReportAnimation, setUpdateReportAnimation] = useState(false);
    const [selectedEmployesList, setSelectedEmployesList] = useState([]);
    const [objectsList, setObjectsList] = useState([]);
    const [resultTable, setResultTable] = useState([]);


    useEffect(() => {
        updateObjects();
        updateEmployeeList();
        }
    , []);

    function updateEmployeeList() {
    //console.log("updateEmployeeList");

      GetEmployeeList()
        .then((data) => {
            //console.log(data);

            if(data.isSuccess){
                setEmployeeList(data.employeesList);
            }
        })
        .catch((error) => console.error('Ошибка при получении данных сотрудников:', error));
    }

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



    function updateReport() {

        if(selectedEmployesList.length == 0){
            alert("Список сотрудников пуст");
            return;
        }

        const list = selectedEmployesList.join(",");

        const params = {
            employees: list,
            startDate: startDate.toISOString(),
            endDate: endDate.toISOString()
        };

        setUpdateReportAnimation(true);
        GetMainReportForPeriodAsTable(params)
        .then((data) => {
            setUpdateReportAnimation(false);
            console.log(data);
            if(data.isSuccess){

                setResultTable(
                    data.mainReportTable.items
                );

            }
            else {
                //alert
            }
        })
        .catch((error) => {
            setUpdateReportAnimation(false);
            console.error('Ошибка при получении данных отчета:', error)
        });
    }


   



    return (
    
        <Container expand="lg">
        <br/>
        {/* Заголовок страницы */}
            <div className="d-flex justify-content-between align-items-center mb-4">
              <div>
                <h2 className="mb-1">Отчет</h2>
                <p className="text-muted mb-0"></p>
              </div>

            </div>
            <Card className=" h-100">
                <Card.Body>
                <Card.Header className="bg-white border-0">
                
                    <Table>
                        <tbody>
                        <tr>
                            <td width="35%">
                            <Form.Select >
                            {
                            objectsList.map((obj) => (
                                <option key={obj.id} value={obj.id}>{obj.name}</option>
                            ))
                            }
                            </Form.Select>
                            </td>
                            <td style={{textAlign:"right"}}>
                                Выгрузка расчетов с даты 
                            </td>
                            <td>
                               <DatePicker locale="ru" selected={startDate} onChange={(date) => setStartDate(date)} />
                            </td>
                            <td  style={{textAlign:"right"}}>
                            по дату
                            </td>
                            <td>
                                <DatePicker locale="ru" selected={endDate} onChange={(date) => setEndDate(date)} />
                            </td>
                            <td>
                                <Button onClick={updateReport} sm={1} variant="secondary" className="d-flex align-items-center">Построить отчет</Button>
                            </td>
                        </tr>

                        </tbody>
                    </Table>
                    <div className="table-responsive" style={{maxHeight:"200px"}}>
                        <Table bordered hover >
                            <thead>
                                <tr>
                                    <th width="1%">Id</th>
                                    <th width="30%">ФИО</th>
                                    <th width="10%">В отчет</th>
                                </tr>
                            </thead>
                            <tbody>
                                {
                                    employeeList ? 
                                    employeeList.map((item) => (<tr key={item.id}>
                                        <td>{item.id}</td>
                                        <td>{item.fio}</td>
                                        <td>
                                            <Form.Check 
                                            checked={
                                                selectedEmployesList.includes(item.id) ? true : false
                                                } 

                                                onClick={(e) => {
                                                    if(e.target.checked){
                                                        setSelectedEmployesList([...selectedEmployesList, item.id]);
                                                    }
                                                    else{
                                                        setSelectedEmployesList(selectedEmployesList.filter(id => id !== item.id));
                                                    }
                                                    //console.log(selectedEmployesList);
                                                }}
                                                type="checkbox" />
                                        </td>
                                        </tr>
                                        ))
                                    :
                                    <></>
                                }
                            </tbody>
                        </Table>
                    </div>
                </Card.Header>
                
                {

                
                updateReportAnimation ? 
                <div style={{textAlign:"center"}}>
                    <Spinner />
                    <h4>Загрузка данных...</h4>
                </div>
                  :
                <div className="table-responsive" style={{height:"400px"}}>
                <br/>
                <div className="h3">Данные о рабочих часах</div>
                
                <Table bordered hover>
                    <thead>
                        <tr>
                            <th width="30%">ФИО</th>
                            <th width="15%">Дней</th>
                            <th width="15%">Сумма за работу</th>
                            <th width="12%">Начисления</th>
                            <th width="12%">Списания</th>
                            <th width="15%">Итого</th>
                        </tr>
                    </thead>
                    <tbody>
                        {   resultTable ?
                            resultTable.map((item) => (<tr key={item[0]}>
                                    <td>{item[0]}</td>
                                    <td>{item[1]}</td>
                                    <td>{item[2]}</td>
                                    <td>{item[3]}</td>
                                    <td>{item[4]}</td>
                                    <td>{item[5]}</td>
                                    </tr>)
                            )
                            :
                            <></>
                        }
                    </tbody>
                </Table>
                <br/>

                </div>
                }


                <FormGroup  className="m-3" style={{textAlign:"right"}}>
                        <Form.Label>Скачать отчет без разбивки по банкам</Form.Label>
                         &nbsp; &nbsp;
                        <Button type="button" variant="primary" onClick={() => {
                            //проверяем выбран ли хотя бы один сотрудник
                            if(selectedEmployesList.length > 0){
                                
                                const url = apiUrl + '/api/report/GetMainReportForPeriodAsXls?startDate=' 
                                    + startDate.toISOString() 
                                    + '&endDate=' + endDate.toISOString() + '&employees=' + selectedEmployesList.join(",");                   
                                DownloadFileWithAuth(url, "Отчет без разбивки по банкам.xlsx");
                            }
                            else{
                                alert("Выберите хотя бы одного сотрудника");
                            }
                        }}>
                        Скачать
                        </Button>
                </FormGroup>
                <FormGroup  className="m-3" style={{textAlign:"right"}}>
                        <Form.Label>Скачать отчет с разбивкой по банкам</Form.Label>
                         &nbsp; &nbsp;
                        <Button disabled type="button" variant="primary" onClick={() => {
                            //проверяем выбран ли хотя бы один сотрудник
                            if(selectedEmployesList.length > 0){

                                const url = apiUrl + '/api/report/GetMainReportForPeriodAsXlsWithBanks?startDate=' 
                                    + startDate.toISOString() 
                                    + '&endDate=' + endDate.toISOString() + '&employees=' + selectedEmployesList.join(",");

                                DownloadFileWithAuth(url, "Отчет с разбивкой по банкам.xlsx");
                            }
                            else{
                                alert("Выберите хотя бы одного сотрудника");
                            }
                        }}>
                        Скачать
                        </Button>
                </FormGroup>
                <FormGroup className="m-3" style={{textAlign:"right"}}>
                        <Form.Label>Отметить платежи в отчете исполненными </Form.Label>
                        &nbsp; &nbsp;
                        <Button disabled type="button" variant="primary" onClick={() => {}}>
                        Поставить отметку
                        </Button>
                </FormGroup>

                <br/>
                <br/>

                </Card.Body>
            </Card>
        </Container>
    )
}