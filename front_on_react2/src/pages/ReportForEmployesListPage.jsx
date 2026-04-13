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
    GetMainReportVer4AsTable,
    GetAllObjects, 
    DownloadFileWithAuth,
    SavePayoutMarks, SavePayoutMarks2
} from '../services/apiService';

import { registerLocale, setDefaultLocale } from  "react-datepicker";
import { ru } from 'date-fns/locale/ru';
registerLocale('ru', ru)

function normalizeVer4Cells(cells) {
    const c = cells && Array.isArray(cells) ? [...cells] : [];
    while (c.length < 14) c.push('');
    return c.slice(0, 14);
}

function renderMainReportVer4Row(row, idx) {
    const cells = normalizeVer4Cells(row.cells);
    const td = (i, extra = {}) => (
        <td key={i} {...extra}>{cells[i] !== '' ? cells[i] : ''}</td>
    );

    switch (row.kind) {
        case 'title':
            return (
                <tr key={idx}>
                    <td colSpan={14} style={{ fontSize: '1.05rem', fontWeight: 500, borderBottom: 'none' }}>{cells[0]}</td>
                </tr>
            );
        case 'sectionBanner':
            return (
                <tr key={idx} style={{ backgroundColor: '#fff3cd' }}>
                    <td colSpan={14} style={{ fontSize: '1.05rem', fontWeight: 700 }}>{cells[0]}</td>
                </tr>
            );
        case 'spacer':
            return (
                <tr key={idx} style={{ height: 8 }}>
                    <td colSpan={14} style={{ padding: 0, border: 'none', height: 8 }} />
                </tr>
            );
        case 'columnHeader':
            return (
                <tr key={idx} style={{ backgroundColor: '#fff3cd' }}>
                    {cells.map((v, i) => (
                        <th key={i} style={{ fontWeight: 700, fontSize: '0.82rem', whiteSpace: 'nowrap' }}>{v}</th>
                    ))}
                </tr>
            );
        case 'noEmployees':
            return (
                <tr key={idx}>
                    <td colSpan={14}>{cells[0]}</td>
                </tr>
            );
        case 'bankTitle':
            return (
                <tr key={idx}>
                    <td colSpan={14} style={{ fontSize: '1.05rem', fontWeight: 500, paddingTop: '0.75rem' }}>{cells[0]}</td>
                </tr>
            );
        case 'grandTotalWithBankTitle':
            return (
                <tr key={idx}>
                    <td colSpan={10} style={{ fontSize: '1.05rem', verticalAlign: 'middle' }}>{cells[0]}</td>
                    <td>{cells[10]}</td>
                    <td style={{ backgroundColor: '#a7b0b9', fontWeight: 700 }}>{cells[11]}</td>
                    <td style={{ backgroundColor: '#a7b0b9', fontWeight: 700 }}>{cells[12]}</td>
                    <td>{cells[13]}</td>
                </tr>
            );
        case 'grandTotal':
            return (
                <tr key={idx}>
                    {Array.from({ length: 14 }, (_, i) => {
                        if (i === 11) return <td key={i} style={{ backgroundColor: '#a7b0b9', fontWeight: 700 }}>{cells[i]}</td>;
                        if (i === 12) return <td key={i} style={{ backgroundColor: '#a7b0b9', fontWeight: 700 }}>{cells[i]}</td>;
                        return td(i);
                    })}
                </tr>
            );
        case 'data':
        case 'employeeTotal':
        default:
            return (
                <tr key={idx}>
                    {cells.map((v, i) => (
                        <td key={i} style={{ whiteSpace: 'nowrap' }}>{v}</td>
                    ))}
                </tr>
            );
    }
}

export function ReportForEmployesListPage() {

    const [employeeList, setEmployeeList] = useState([]);
    const [startDate, setStartDate] = useState(new Date());
    const [endDate, setEndDate] = useState(new Date());
    const [updateReportAnimation, setUpdateReportAnimation] = useState(false);
    const [selectedEmployesList, setSelectedEmployesList] = useState([]);
    const [objectsList, setObjectsList] = useState([]);
    const [reportVer4Rows, setReportVer4Rows] = useState([]);
    const [savingMarks, setSavingMarks] = useState(false);

    useEffect(() => {
        updateObjects();
        //updateEmployeeList();
        }
    , []);


    //загружаем сотрудников только для выделеного объекта
    function updateEmployeeList(objectId) {
    console.log("updateEmployeeList");

      GetEmployeeList(objectId)

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
              //загружаем пользователей для объекта [0]
              if(data.objects && data.objects.length>0){
                updateEmployeeList(data.objects[0].id);
              }

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



    function updateReport2() {

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
        GetMainReportVer4AsTable(params)
        .then((data) => {
            setUpdateReportAnimation(false);
            if(data.isSuccess){
                setReportVer4Rows(data.rows || []);
            }
            else {
                setReportVer4Rows([]);
                alert(data.message || "Не удалось построить отчёт");
            }
        })
        .catch((error) => {
            setUpdateReportAnimation(false);
            setReportVer4Rows([]);
            console.error('Ошибка при получении данных отчета:', error);
            alert("Ошибка при получении данных отчета");
        });
    }

   
    function onSelectObject(e){
        //console.log(e.target.value);
        const objId = e.target.value;
        updateEmployeeList(objId);
    }

    function handleCreateReportWithMarks() {
        if (selectedEmployesList.length === 0) {
            alert("Выберите хотя бы одного сотрудника");
            return;
        }

        const params = {
            employees: selectedEmployesList.join(","),
            startDate: startDate.toISOString(),
            endDate: endDate.toISOString()
        };

        setSavingMarks(true);
        SavePayoutMarks(params)
            .then((data) => {
                setSavingMarks(false);
                if (data.isSuccess) {
                    alert(data.message || "Отчет с отметками создан. Дальнейшие отметки о выдаче зарплаты — на странице «Зарплата».");
                } else {
                    alert(data.message || "Ошибка при создании отчета");
                }
            })
            .catch((error) => {
                setSavingMarks(false);
                console.error("Ошибка при создании отчета с отметками:", error);
                alert("Ошибка при создании отчета с отметками");
            });
    }


    function handleTest(){
        //создаем отчет с отметками
        if (selectedEmployesList.length === 0) {
            alert("Выберите хотя бы одного сотрудника");
            return;
        }

        const params = {
            employees: selectedEmployesList.join(","),
            startDate: startDate.toISOString(),
            endDate: endDate.toISOString()
        };

        setSavingMarks(true);
        SavePayoutMarks2(params)            
        .then((data) => {
                setSavingMarks(false);
                if (data.isSuccess) {
                    alert(data.message || "Отчет с отметками создан. Дальнейшие отметки о выдаче зарплаты — на странице «Зарплата».");
                } else {
                    alert(data.message || "Ошибка при создании отчета");
                }
        })
        .catch((error) => {
          setSavingMarks(false);
            console.error("Ошибка при создании отчета с отметками:", error);
            alert("Ошибка при создании отчета с отметками");
        });


    }

    return (
    
        <Container expand="lg">
        <br/>
        {/* Заголовок страницы */}
            <div className="d-flex justify-content-between align-items-center mb-4">
              <div>
                <h2 className="mb-1">Отчет по списку сотрудников</h2>
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
                                <Button onClick={updateReport2} sm={1} 
                                variant="secondary" 
                                className="d-flex align-items-center">Построить отчет</Button>
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
                                            <Form.Check  key={item.id}
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
                <div className="table-responsive" style={{minHeight:"400px"}}>
                <br/>
                <div className="h3">Данные о рабочих часах</div>
                <p className="text-muted small mb-2">Таблица совпадает с выгрузкой «Скачать отчёт» (версия с разбивкой по банкам).</p>
                {reportVer4Rows.length === 0 ? (
                    <p className="text-muted">Нажмите «Построить отчёт», чтобы сформировать таблицу.</p>
                ) : (
                    <Table bordered hover className="mt-2" style={{ fontSize: '0.88rem' }}>
                        <tbody>
                            {reportVer4Rows.map((row, i) => renderMainReportVer4Row(row, i))}
                        </tbody>
                    </Table>
                )}
                <br/>

                </div>
                }

                {    /*
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
                 */   }
                 { /*
                <FormGroup  className="m-3" style={{textAlign:"right"}}>
                        <Form.Label>Скачать отчет с разбивкой по банкам</Form.Label>
                         &nbsp; &nbsp;
                        <Button type="button" variant="primary" onClick={() => {
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
                </FormGroup> */ }

                <Table bordered={true}>
                    <tbody>
                        <tr>
                            <td>
                                Скачать отчет c разбивкой по банкам 
                            </td>
                            <td>
                                <Button type="button" variant="primary" onClick={() => {
                                //проверяем выбран ли хотя бы один сотрудник
                                if(selectedEmployesList.length > 0){

                                //const url = apiUrl + '/api/report/GetMainReportForPeriodAsXlsWithBanks?startDate=' 
                                const url = apiUrl + '/api/report/GetMainReportVer4AsXls?startDate=' 
                                    + startDate.toISOString() 
                                    + '&endDate=' + endDate.toISOString() + '&employees=' + selectedEmployesList.join(",");

                                DownloadFileWithAuth(url, "Отчет с разбивкой по банкам.xlsx");
                                }
                                else{
                                    alert("Выберите хотя бы одного сотрудника");
                                }
                        }}>
                        Скачать отчет
                        </Button>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Скачать отчет с разбивкой по банкам 
                                и заполнить данные о выплатах <br/>
                                <small>Дальнейшие отметки о выдаче зарплаты на странице "Зарплата"</small>
                                <br/>
                                <Button variant="link" href="/reportForPayout2" >Перейти к заполнению отметок об оплате</Button>
                            </td>
                            <td>
                                {  /*
                                    <Button
                                        variant="primary"
                                        onClick={handleCreateReportWithMarks}
                                        disabled={savingMarks || selectedEmployesList.length === 0}
                                    >
                                        {savingMarks ? (
                                            <>
                                                <Spinner animation="border" size="sm" className="me-2" />
                                                Создание...
                                            </>
                                        ) : (
                                            "Создать отчет с отметками"
                                        )}
                                    </Button>
                                    */ }

                                    <Button variant="primary" onClick={handleTest}>
                                                                                {savingMarks ? (
                                            <>
                                                <Spinner animation="border" size="sm" className="me-2" />
                                                Создание...
                                            </>
                                        ) : (
                                            "Создать отчет с отметками"
                                        )}
                                    </Button>
                            </td>
                        </tr>
                    </tbody>
                </Table>
                {
                    /*
                <FormGroup className="m-3" style={{textAlign:"right"}}>
                        <Form.Label>Отметить платежи в отчете исполненными </Form.Label>
                        &nbsp; &nbsp;
                        <Button disabled type="button" variant="primary" onClick={() => {}}>
                        Поставить отметку
                        </Button>
                </FormGroup>
                */
                }
                <br/>
                <br/>

                </Card.Body>
            </Card>
        </Container>
    )
}