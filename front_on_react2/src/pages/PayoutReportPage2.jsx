import React, { useState, useEffect } from 'react';
import {
  Container, Row, Col, Table,
  Card,
  Spinner,
  Toast,
  ToastContainer,
  FormCheck
} from 'react-bootstrap';


import {
  GetMainReportNumbersList,
  GetMainReportFromPayoutMarks,
  GetPayOffStatusesForReport,
  SetPayOffForEmployeeInReport,
GetMainReportFromPayoutMarks2,
MarkPayoutRow
} from '../services/apiService';
import { getDateFormat2 } from '../services/commonService';





export function PayoutReportPage2(){

  const [updateReportAnimation, setUpdateReportAnimation] = useState(false);
  const [resultTable, setResultTable] = useState([]);
  const [reportNumbersList, setReportNumbersList] = useState([]);
  const [loadingReportNumbers, setLoadingReportNumbers] = useState(false);
  const [selectedReportNumber, setSelectedReportNumber] = useState(null);
  const [payOffMap, setPayOffMap] = useState({});
  const [showSavedToast, setShowSavedToast] = useState(false);


    useEffect(() => {
      loadReportNumbers();
    }, []);

    
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

    async function handleSelectSavedReport(reportNumber) {
        //alert(reportNumber);

        if (!reportNumber) {
          return;
        }

        setSelectedReportNumber(reportNumber);
        setUpdateReportAnimation(true);
        setResultTable([]);

        try{
            //todo: загрузка отчета
            GetMainReportFromPayoutMarks2(reportNumber)
            .then(data =>{
                console.log("data", data);

                setUpdateReportAnimation(false);
                if (data.isSuccess) {
                    setResultTable(data.items || []);
                }
            })
            .catch(() => setUpdateReportAnimation(false));
        }
        catch (error) {
            console.error('Ошибка при загрузке сохраненного отчета или статусов PayOff:', error);
            alert('Ошибка при загрузке сохраненного отчета');
        } finally {
            setUpdateReportAnimation(false);
        }

    }

    function MarkRow(state, reportNumber, employeeId){
        //console.log({state});

        const q = {
            ReportNumber: reportNumber,
            EmployeeId: employeeId,
            Checked: state
        }
        /*
        MarkPayoutRow(q)
        .then(data => {
            console.log("MarkRow", data);

            if(data.isSuccess){
                alert("Подтверждение сохранено");
            }
        })
        .catch((error) => console.log(error));
        */
    }


    return(
    <Container expand="lg">
        <ToastContainer position="top-end" className="p-3">
        <Toast
          show={showSavedToast}
          onClose={() => setShowSavedToast(false)}
          delay={3000}
          autohide
          bg="success"
        >
        <Toast.Header closeButton>
            <strong className="me-auto">Сохранено</strong>
        </Toast.Header>
        <Toast.Body>Изменения сохранены</Toast.Body>
        </Toast>
        </ToastContainer>
      <br />
      {/* Заголовок страницы */}
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 className="mb-1">Отчет о зарплате за период</h2>
          <p className="text-muted mb-0"></p>
        </div>
      </div>
            <Row>
            <Col lg={12} className="mb-4">
            <Card className="h-100">
              <Card.Body>
              <Card.Header>
                Сохраненные отчеты<br/>
                  <small>Нажмите на строку для отображения отчета</small>
              </Card.Header>
                <div className="table-responsive" style={{maxHeight:"200px"}}>
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
                            <tr
                                key={row.id}
                                onClick={() => handleSelectSavedReport(row.reportNumber)}
                                style={{ cursor: 'pointer' }}
                                className={row.reportNumber === selectedReportNumber ? 'table-active' : ''}
                            >
                                <td>{row.reportNumber}</td>
                                <td>
                                    {row.startDate ? getDateFormat2(row.startDate) : '—'}
                                    {' — '}
                                    {row.endDate ? getDateFormat2(row.endDate) : '—'}
                                </td>
                                <td>{row.createAuthor || '—'}</td>
                                <td>{row.created ? new Date(row.created).toLocaleString('ru-RU') : '—'}</td>
                                </tr>
                            ))
                            )}
                        </tbody>
                        </Table>
                    )}

                </div>

              <br/>
              <div className="h3 mb-3">Отчет о зарплате</div>
            {updateReportAnimation ? (
            <div style={{ textAlign: "center", padding: "40px" }}>
              <Spinner />
              <h4>Загрузка данных...</h4>
            </div>
                ) : (resultTable && resultTable.length > 0 && (
                      <div className="table-responsive" style={{ minHeight: "400px", marginTop: "20px" }}>
                        <Table bordered hover>
                            <thead>
                                <tr>
                                    <th>Id</th>
                                    <th>ФИО</th>
                                    <th>Есть аванс в расчетном периоде</th>
                                    <th>Есть аванс в предыдущем периоде</th>
                                    <th>Итоговая сумма к расчету, руб.</th>
                                    <th>Подтвердить</th>
                                </tr>
                            </thead>
                          <tbody>
                            {resultTable.map((row, i) => {

                                return (
                                    <tr>
                                        <td>
                                            {row.employeeId}
                                        </td>
                                        <td>
                                            {row.employeeFio}
                                        </td>
                                        <td>
                                            {
                                                row.hasAdvancePayment==true ? "Да" : "Нет"
                                            }
                                        </td>
                                        <td>
                                            {
                                                row.hasAdvancePaymentInPrevPeriod==true ? "Да" : "Нет"
                                            }
                                        </td>
                                        <td>
                                            {row.totalSum}
                                        </td>
                                        <td>
                                            <FormCheck onClick={(e) => {
                                                //console.log(e.target.checked);

                                                MarkRow(e.target.checked, selectedReportNumber, row.employeeId)
                                            }}>

                                            </FormCheck>
                                        </td>
                                    </tr>
                                )
                            })

                            }
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