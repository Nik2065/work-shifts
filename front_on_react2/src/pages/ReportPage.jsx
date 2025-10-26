import React, {useState, useEffect} from "react";

import { 
  Container, Row, Col, 
   Card, Button, 
  Dropdown, Form, Table
} from 'react-bootstrap';
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import {GetEmployeeList, GetWorkHoursForPeriodApi} from '../services/apiService';
import { FileText } from 'lucide-react';

import { registerLocale, setDefaultLocale } from  "react-datepicker";
import { ru } from 'date-fns/locale/ru';
registerLocale('ru', ru)

export function ReportPage() {

    const [employeeList, setEmployeeList] = useState([]);
    const [startDate, setStartDate] = useState(new Date());
    const [endDate, setEndDate] = useState(new Date());
    const [updateReportAnimation, setUpdateReportAnimation] = useState(false);
    const [workHoursList, setWorkHoursList] = useState(null);
    const [totalData, setTotalData] = useState({
                    totalHours: 0,
                    itemsCount: 0,
                    totalSalary: 0});
    const [employeeId, setEmployeeId] = useState(null);


    useEffect(() => {
        updateEmployeeList();
        }
    , []);

    function updateEmployeeList() {
    //console.log("updateEmployeeList");

      GetEmployeeList()
        .then((response) => response.json())
        .then((data) => {
            console.log(data);

            if(data.isSuccess){
                setEmployeeList(data.employeesList);
                //теперь скачиваем отработанные часы
                //updateWorkHours(currentDate);
                if(data.employeesList.length > 0){
                    setEmployeeId(data.employeesList[0].id);
                }
            }
        })
        .catch((error) => console.error('Ошибка при получении данных сотрудников:', error));
    }


    function updateReport() {
        const params = {
            employeeId: employeeId,
            startDate: startDate.toISOString(),
            endDate: endDate.toISOString()
        };

        setUpdateReportAnimation(true);
        GetWorkHoursForPeriodApi(params)
        .then((response) => response.json())
        .then((data) => {
            setUpdateReportAnimation(false);
            console.log(data);
            if(data.isSuccess){
                setWorkHoursList(data.workHoursList);
                setTotalData({
                    totalHours: data.totalHours,
                    itemsCount: data.ItemsCount,
                    totalSalary: data.totalSalary
                });
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
                <h2 className="mb-1">Отчеты</h2>
                <p className="text-muted mb-0"></p>
              </div>

            </div>
            <Card className=" h-100">
                <Card.Body>
                <Card.Header className="bg-white border-0">

                    <Table>
                        <tbody>
                        <tr>
                            <td>
                            <Form.Select >
                            {
                            employeeList.map((employee) => (
                                <option key={employee.id} value={employee.id}>{employee.fio}</option>
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
                </Card.Header>
                
                {
                    setWorkHoursList == null ?
                <div style={{textAlign:"center"}}>
                    <FileText size={100} />
                    <h4>Не выбраны даты отчета</h4>
                </div>
                : 
                <div className="table-responsive" style={{height:"400px"}}>
                <Table bordered hover>
                    <thead>
                        <tr>
                            <th>Id</th>
                            <th>Дата</th>
                            <th>Часы</th>
                            <th>Сумма</th>
                            <th>Тип</th>
                        </tr>
                    </thead>
                    <tbody>
                        {   workHoursList ?
                            workHoursList.map((item) => (<tr key={item.id}>
                                    <td>{item.id}</td>
                                    <td>{item.date}</td>
                                    <td>{item.hours}</td>
                                    <td>{item.itemSalary}</td>
                                    </tr>)
                            )
                            :
                            <></>
                        }
                    </tbody>
                </Table>
                </div>
                }

                {
                    <Table bordered>
                        <thead>
                            <tr>
                                <th>Количество записей</th>
                                <th>Количество часов</th>
                                <th>Сумма</th>
                            </tr>
                            <tr>
                                <td>{totalData.itemsCount}</td>
                                <td>{totalData.totalHours}</td>
                                <td>{totalData.totalSalary}</td>
                            </tr>
                        </thead>
                    </Table>
                }
                </Card.Body>
            </Card>
        </Container>
    )
}