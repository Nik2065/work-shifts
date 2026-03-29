import React, { useState, useEffect } from 'react';
import {
  Container, Row, Col, Table,
  Card,
  Spinner,
  Toast,
  ToastContainer
} from 'react-bootstrap';
import {
  GetMainReportNumbersList,
  GetMainReportFromPayoutMarks,
  GetPayOffStatusesForReport,
  SetPayOffForEmployeeInReport
} from '../services/apiService';
import { getDateFormat2 } from '../services/commonService';

export function PayoutReportPage() {
  const [updateReportAnimation, setUpdateReportAnimation] = useState(false);
  const [resultTable, setResultTable] = useState([]);
  const [reportNumbersList, setReportNumbersList] = useState([]);
  const [loadingReportNumbers, setLoadingReportNumbers] = useState(false);
  const [selectedReportNumber, setSelectedReportNumber] = useState(null);
  const [payOffMap, setPayOffMap] = useState({});
  const [showSavedToast, setShowSavedToast] = useState(false);

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
    loadReportNumbers();
  }, []);

  async function handleSelectSavedReport(row) {
    if (!row || !row.reportNumber) {
      return;
    }

    setSelectedReportNumber(row.reportNumber);
    setUpdateReportAnimation(true);
    setResultTable([]);
    setPayOffMap({});

    try {
      const [reportData, payOffData] = await Promise.all([
        GetMainReportFromPayoutMarks(row.reportNumber),
        GetPayOffStatusesForReport(row.reportNumber),
      ]);

      if (reportData.isSuccess) {
        setResultTable(reportData.mainReportTable?.items || []);
      } else {
        alert(reportData.message || 'Ошибка при загрузке сохраненного отчета');
      }

      if (payOffData.isSuccess && payOffData.items) {
        const map = {};
        payOffData.items.forEach((item) => {
          if (item.fio) {
            map[item.fio] = item.payOff;
          }
        });
        setPayOffMap(map);
      }
    } catch (error) {
      console.error('Ошибка при загрузке сохраненного отчета или статусов PayOff:', error);
      alert('Ошибка при загрузке сохраненного отчета');
    } finally {
      setUpdateReportAnimation(false);
    }
  }

  function handleTogglePayOff(fio, currentValue) {
    if (!selectedReportNumber || !fio) {
      return;
    }

    const newValue = !currentValue;

    setPayOffMap((prev) => ({
      ...prev,
      [fio]: newValue,
    }));

    SetPayOffForEmployeeInReport({
      reportNumber: selectedReportNumber,
      fio: fio,
      payOff: newValue,
    })
      .then((data) => {
        if (data.isSuccess) {
          setShowSavedToast(true);
        } else {
          alert(data.message || 'Ошибка при сохранении статуса выплаты');
        }
      })
      .catch((error) => {
        console.error('Ошибка при сохранении статуса выплаты:', error);
        alert('Ошибка при сохранении статуса выплаты');
      });
  }

  return (
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
                        onClick={() => handleSelectSavedReport(row)}
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
          ) : (
            resultTable && resultTable.length > 0 && (
              <div className="table-responsive" style={{ minHeight: "400px", marginTop: "20px" }}>
                <Table bordered hover>
                  <tbody>
                    {resultTable.map((row, rowIndex) => {
                      const fio = row[0];
                      const isEmployeeRow =
                        fio &&
                        fio !== "ФИО" &&
                        fio !== "Общий итог:" &&
                        !fio.startsWith("Расчет ");
                      const checked = !!payOffMap[fio];

                      const isHead = fio &&  fio === "ФИО";

                      return (
                        <tr key={rowIndex}>
                          {row.map((cell, cellIndex) => (
                            <td key={cellIndex}>{cell}</td>
                          ))}
                          <td style={{ textAlign: "center" }}>
                            {isEmployeeRow ? (
                              <input
                                type="checkbox"
                                checked={checked}
                                onChange={() => handleTogglePayOff(fio, checked)}
                              />
                            ) : null}
                            { isHead ? "Подпись сотрудника"
                            : null
                            }


                          </td>
                        </tr>
                      );
                    })}
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