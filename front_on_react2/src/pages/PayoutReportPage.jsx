import React, { useState, useEffect } from 'react';
import {
  Container, Row, Col, Toast, Table, ToastContainer,
  Card, Button,
  Form, Spinner
} from 'react-bootstrap';
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import { registerLocale, setDefaultLocale } from "react-datepicker";
import { ru } from 'date-fns/locale/ru';
import { GetEmployeeList, GetAllObjects, GetMainReportForPeriodAsTableWithBanks, SavePayoutMarks, GetMainReportNumbersList } from '../services/apiService';

registerLocale('ru', ru)

export function PayoutReportPage() {
  const [employeeList, setEmployeeList] = useState([]);
  const [startDate, setStartDate] = useState(new Date());
  const [endDate, setEndDate] = useState(new Date());
  const [updateReportAnimation, setUpdateReportAnimation] = useState(false);
  const [selectedEmployesList, setSelectedEmployesList] = useState([]);
  const [objectsList, setObjectsList] = useState([]);
  const [selectedObject, setSelectedObject] = useState(-1);
  const [resultTable, setResultTable] = useState([]);
  const [savingMarks, setSavingMarks] = useState(false);
  const [reportNumbersList, setReportNumbersList] = useState([]);
  const [loadingReportNumbers, setLoadingReportNumbers] = useState(false);

  function loadReportNumbers() {
    setLoadingReportNumbers(true);
    GetMainReportNumbersList()
      .then((data) => {
        setLoadingReportNumbers(false);
        if (data.isSuccess) {
          setReportNumbersList(data.items || []);
        }
      })
      .catch(() => setLoadingReportNumbers(false));
  }

  useEffect(() => {
    updateObjects();
    loadReportNumbers();
  }, []);

  //загружаем сотрудников для выделенного объекта
  function updateEmployeeList(objectId) {
    console.log("updateEmployeeList");
    GetEmployeeList(objectId === -1 ? null : objectId)
      .then((data) => {
        if (data.isSuccess) {
          setEmployeeList(data.employeesList || []);
          // Сбрасываем выбранных сотрудников при смене объекта
          setSelectedEmployesList([]);
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
          setObjectsList(data.objects || []);
          // Загружаем всех сотрудников при первой загрузке
          updateEmployeeList(-1);
        }
      })
      .catch(error => console.log(error));
  }

  function onSelectObject(e) {
    const objId = parseInt(e.target.value);
    setSelectedObject(objId);
    updateEmployeeList(objId);
  }

  function updateReport() {
    if (selectedEmployesList.length == 0) {
      alert("Выберите хотя бы одного сотрудника");
      return;
    }

    const list = selectedEmployesList.join(",");

    const params = {
      employees: list,
      startDate: startDate.toISOString(),
      endDate: endDate.toISOString()
    };

    setUpdateReportAnimation(true);
    GetMainReportForPeriodAsTableWithBanks(params)
      .then((data) => {
        setUpdateReportAnimation(false);
        console.log(data);
        if (data.isSuccess) {
          setResultTable(data.mainReportTable?.items || []);
        } else {
          alert(data.message || 'Ошибка при получении отчета');
        }
      })
      .catch((error) => {
        setUpdateReportAnimation(false);
        console.error('Ошибка при получении данных отчета:', error);
        alert('Ошибка при получении отчета');
      });
  }

  function handleSavePayoutMarks() {
    if (selectedEmployesList.length == 0) {
      alert("Выберите хотя бы одного сотрудника");
      return;
    }

    if (resultTable.length == 0) {
      alert("Сначала постройте отчет");
      return;
    }

    const list = selectedEmployesList.join(",");

    const params = {
      employees: list,
      startDate: startDate.toISOString(),
      endDate: endDate.toISOString()
    };

    setSavingMarks(true);
    SavePayoutMarks(params)
      .then((data) => {
        setSavingMarks(false);
        console.log(data);
        if (data.isSuccess) {
          alert(data.message || 'Отметки об оплате успешно сохранены');
          loadReportNumbers();
        } else {
          alert(data.message || 'Ошибка при сохранении отметок');
        }
      })
      .catch((error) => {
        setSavingMarks(false);
        console.error('Ошибка при сохранении отметок:', error);
        alert('Ошибка при сохранении отметок');
      });
  }

  return (
    <Container expand="lg">
      <br />
      {/* Заголовок страницы */}
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 className="mb-1">Отчет о зарплате за период</h2>
          <p className="text-muted mb-0"></p>
        </div>
      </div>

      <Row>
        <Col lg={4} className="mb-4">
          <Card>
            <Card.Header>Сохраненные отчеты</Card.Header>
            <Card.Body className="p-0">
              {loadingReportNumbers ? (
                <div className="text-center py-4">
                  <Spinner animation="border" size="sm" />
                </div>
              ) : (
                <Table bordered hover size="sm" className="mb-0">
                  <thead>
                    <tr>
                      <th>№ отчета</th>
                      <th>Период</th>
                      <th>Автор</th>
                      <th>Дата создания</th>
                    </tr>
                  </thead>
                  <tbody>
                    {reportNumbersList.length === 0 ? (
                      <tr>
                        <td colSpan={4} className="text-center text-muted py-3">
                          Нет сохраненных отчетов
                        </td>
                      </tr>
                    ) : (
                      reportNumbersList.map((row) => (
                        <tr key={row.id}>
                          <td>{row.reportNumber}</td>
                          <td>
                            {row.startDate ? new Date(row.startDate).toLocaleDateString('ru-RU') : '—'}
                            {' — '}
                            {row.endDate ? new Date(row.endDate).toLocaleDateString('ru-RU') : '—'}
                          </td>
                          <td>{row.createAuthor || '—'}</td>
                          <td>{row.created ? new Date(row.created).toLocaleString('ru-RU') : '—'}</td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </Table>
              )}
            </Card.Body>
          </Card>
        </Col>
        <Col lg={8} className="mb-4">
      <Card className="h-100">
        <Card.Body>
          <Card.Header className="bg-white border-0">
            <Table>
              <tbody>
                <tr>
                  <td width="25%">
                    <Form.Label>Объект</Form.Label>
                    <Form.Select value={selectedObject} onChange={(e) => onSelectObject(e)}>
                      <option value={-1}>Все объекты</option>
                      {objectsList.map((obj) => (
                        <option key={obj.id} value={obj.id}>{obj.name}</option>
                      ))}
                    </Form.Select>
                  </td>
                  <td width="15%" style={{ textAlign: "right" }}>
                    <Form.Label>Дата начала</Form.Label>
                  </td>
                  <td width="20%">
                    <DatePicker
                      locale="ru"
                      selected={startDate}
                      onChange={(date) => setStartDate(date)}
                      className="form-control"
                    />
                  </td>
                  <td width="15%" style={{ textAlign: "right" }}>
                    <Form.Label>Дата конца</Form.Label>
                  </td>
                  <td width="20%">
                    <DatePicker
                      locale="ru"
                      selected={endDate}
                      onChange={(date) => setEndDate(date)}
                      className="form-control"
                    />
                  </td>
                  <td width="5%">
                    <Button
                      onClick={updateReport}
                      variant="primary"
                      className="d-flex align-items-center"
                      disabled={updateReportAnimation}
                    >
                      {updateReportAnimation ? (
                        <>
                          <Spinner animation="border" size="sm" className="me-2" />
                          Загрузка...
                        </>
                      ) : (
                        'Построить отчет'
                      )}
                    </Button>
                  </td>
                </tr>
              </tbody>
            </Table>

            <div className="table-responsive" style={{ maxHeight: "400px", marginTop: "20px" }}>
              <Table bordered hover>
                <thead>
                  <tr>
                    <th width="5%">Id</th>
                    <th width="40%">ФИО</th>
                    <th width="30%">Объект</th>
                    <th width="10%">В отчет</th>
                  </tr>
                </thead>
                <tbody>
                  {employeeList && employeeList.length > 0 ? (
                    employeeList.map((item) => (
                      <tr key={item.id}>
                        <td>{item.id}</td>
                        <td>{item.fio}</td>
                        <td>{item.objectName || '—'}</td>
                        <td>
                          <Form.Check
                            key={item.id}
                            checked={selectedEmployesList.includes(item.id)}
                            onChange={(e) => {
                              if (e.target.checked) {
                                setSelectedEmployesList([...selectedEmployesList, item.id]);
                              } else {
                                setSelectedEmployesList(selectedEmployesList.filter(id => id !== item.id));
                              }
                            }}
                            type="checkbox"
                          />
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={4} className="text-center text-muted py-4">
                        Нет данных о сотрудниках
                      </td>
                    </tr>
                  )}
                </tbody>
              </Table>
            </div>
          </Card.Header>

            {updateReportAnimation ? (
            <div style={{ textAlign: "center", padding: "40px" }}>
              <Spinner />
              <h4>Загрузка данных...</h4>
            </div>
          ) : (
            resultTable && resultTable.length > 0 && (
              <div className="table-responsive" style={{ minHeight: "400px", marginTop: "20px" }}>
                <br />
                <div className="d-flex justify-content-between align-items-center mb-3">
                  <div className="h3">Отчет о зарплате</div>
                  <Button
                    onClick={handleSavePayoutMarks}
                    variant="success"
                    disabled={savingMarks || selectedEmployesList.length === 0}
                  >
                    {savingMarks ? (
                      <>
                        <Spinner animation="border" size="sm" className="me-2" />
                        Сохранение...
                      </>
                    ) : (
                      'Сохранить с отметками об оплате'
                    )}
                  </Button>
                </div>
                <Table bordered hover>
                  <tbody>
                    {resultTable.map((item, index) => (
                      <tr key={index}>
                        {item.map((cell, cellIndex) => (
                          <td key={cellIndex}>{cell}</td>
                        ))}
                      </tr>
                    ))}
                  </tbody>
                </Table>
                <br />
              </div>
            )
          )}
        </Card.Body>
      </Card>
        </Col>
      </Row>
    </Container>
  )
}