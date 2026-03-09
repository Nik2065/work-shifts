import React, {useState, useEffect} from "react";
import { apiUrl } from "../services/const"; 

import { 
  Container, Row, Col, 
   Card, Button, 
  Spinner, Form, Table,
  FormGroup
} from 'react-bootstrap';

import DatePicker from "react-datepicker";

import { registerLocale, setDefaultLocale } from  "react-datepicker";
import { ru } from 'date-fns/locale/ru';
registerLocale('ru', ru)

import {GetEmployeeList
} from '../services/apiService';



export function ReportForOneEmployeePage(){


    const [employeeList, setEmployeeList] = useState([]);
    const [objectsList, setObjectsList] = useState([]);
    
    const [startDate, setStartDate] = useState(new Date());
    const [endDate, setEndDate] = useState(new Date());
    const [updateReportAnimation, setUpdateReportAnimation] = useState(false);


        useEffect(() => {
            updateEmployeeList();
            }
        , []);


    function updateEmployeeList(objectId) {
    console.log("updateEmployeeList");

      GetEmployeeList(objectId)

        .then((data) => {
            console.log(data);

            if(data.isSuccess){
                setEmployeeList(data.employeesList);
            }
        })
        .catch((error) => console.error('Ошибка при получении данных сотрудников:', error));
    }



    function updateReport(){

    }


    return (
        <Container expand="lg">
        <br/>
            {/* Заголовок страницы */}
            <div className="d-flex justify-content-between align-items-center mb-4">
              <div>
                <h2 className="mb-1">Финансовый отчет для одного сотрудника</h2>
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
                            <Form.Select onChange={(e) => onSelectObject(e)} >
                            {
                            employeeList.map((emp) => (
                                <option key={emp.id} value={emp.id}>{emp.fio}</option>
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
                                <Button onClick={updateReport} sm={1} 
                                variant="secondary" 
                                className="d-flex align-items-center">Построить отчет</Button>
                            </td>
                        </tr>

                        </tbody>
                    </Table>
                    <div className="table-responsive" style={{maxHeight:"200px"}}>

                    </div>
                </Card.Header>
                <br/>

                <div className="h3">Данные о рабочих часах</div>

                {
                
                updateReportAnimation ? 
                <div style={{textAlign:"center"}}>
                    <Spinner />
                    <h4>Загрузка данных...</h4>
                </div>
                  :
                <div className="table-responsive" style={{minHeight:"400px"}}>
                <br/>

                <Table bordered hover>
                <thead>
                    <tr>
                        <th>Дата</th>
                        <th></th>
                        <th>Сумма</th>
                        <th>Учет</th>
                    </tr>
                </thead>
                
                <tbody>
                <tr>
                    <td>1 марта</td>
                    <td>
                        Объект: Объект Ц. Рабочие часы: 8. Ставка в час: 300 руб.
                    </td>
                    <td>
                        2400 руб.
                    </td>
                    <td>Учтен в отчете #8</td>
                </tr>
                <tr>
                    <td>1 марта</td>
                    <td>
                        Начисление. Другое. 400 руб. 
                    </td>
                    <td>
                        400 руб.
                    </td>
                    <td>Учтен в отчете #8</td>
                </tr>
                <tr>
                    <td>2 марта</td>
                    <td>Списание. Штраф. 500. Комментарий:gkjhdfg </td>
                    <td>- 500 руб.</td>
                    <td>--</td>
                </tr>
                </tbody>
                </Table>
                </div>
                
                }
                </Card.Body>
        </Card>

        </Container>
    )
}
