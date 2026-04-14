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

import {
  GetEmployeeList,
  GetEmployeeFinancialReportApi
} from '../services/apiService';
import { getDateFormat2 } from '../services/commonService';


export function ReportForOneEmployeePage(){


    const [employeeList, setEmployeeList] = useState([]);
    const [objectsList, setObjectsList] = useState([]);
    const [selectedEmployeeId, setSelectedEmployeeId] = useState(null);
    const [reportItems, setReportItems] = useState([]);
    
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
                if (data.employeesList && data.employeesList.length > 0) {
                  setSelectedEmployeeId(data.employeesList[0].id);
                }
            }
        })
        .catch((error) => console.error('Ошибка при получении данных сотрудников:', error));
    }



    function onSelectObject(e){
      const value = e.target.value;
      if (value === "" || value === null) {
        setSelectedEmployeeId(null);
      } else {
        setSelectedEmployeeId(parseInt(value));
      }
    }


    async function updateReport(){
      if (!selectedEmployeeId) {
        return;
      }

      setUpdateReportAnimation(true);
      setReportItems([]);

      const params = {
        startDate: startDate.toISOString().substring(0, 10),
        endDate: endDate.toISOString().substring(0, 10),
        employeeId: selectedEmployeeId
      };

      try {
        const data = await GetEmployeeFinancialReportApi(params);
        console.log("data", data);
        
        if (data.isSuccess) {
          setReportItems(data.items || []);
        } else {
          console.error('Ошибка при построении отчета:', data.message);
        }
      } catch (error) {
        console.error('Ошибка при запросе отчета:', error);
      } finally {
        setUpdateReportAnimation(false);
      }
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
                            <Form.Select value={selectedEmployeeId || ""} onChange={(e) => onSelectObject(e)} >
                            {
                              employeeList.length === 0 && (
                                <option value="">Сотрудники не найдены</option>
                              )
                            }
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
                               <DatePicker
                                 locale="ru"
                                 selected={startDate}
                                 onChange={(date) => setStartDate(date)}
                                 dateFormat="dd/MM/yyyy"
                               />
                            </td>
                            <td  style={{textAlign:"right"}}>
                            по дату
                            </td>
                            <td>
                                <DatePicker
                                  locale="ru"
                                  selected={endDate}
                                  onChange={(date) => setEndDate(date)}
                                  dateFormat="dd/MM/yyyy"
                                />
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
                        <th>Описание</th>
                        <th>Сумма</th>
                        <th>Учет</th>
                    </tr>
                </thead>
                
                <tbody>
                {
                  reportItems.length === 0 ? (
                    <tr>
                      <td colSpan={4} style={{ textAlign: "center" }}>
                        Нет данных за выбранный период
                      </td>
                    </tr>
                  ) : (
                    reportItems.map((item, index) => {
                      const accountingText = item.accountingInfo || '--';
                      const payOffText = item.payed === true ? ' Выплата: да' : item.payed === false ? ' Выплата: нет' : '';
                      return (
                        <tr key={index}>
                          <td>{getDateFormat2(item.date)}</td>
                          <td>{item.description}</td>
                          <td>{item.amount} руб.</td>
                          <td>{accountingText}{payOffText}</td>
                        </tr>
                      );
                    })
                  )
                }
                </tbody>
                </Table>
                </div>
                
                }
                </Card.Body>
        </Card>

        </Container>
    )
}
