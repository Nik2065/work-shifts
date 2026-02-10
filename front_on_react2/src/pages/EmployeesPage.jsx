import React, { useState, useEffect } from 'react';
import {
  Container, Table, Card, Spinner, Pagination
} from 'react-bootstrap';
import { GetEmployeeList } from '../services/apiService';

const PAGE_SIZE = 50;

export function EmployeesPage() {
  const [employeeList, setEmployeeList] = useState([]);
  const [loading, setLoading] = useState(true);
  const [currentPage, setCurrentPage] = useState(1);

  useEffect(() => {
    loadEmployees();
  }, []);

  function loadEmployees() {
    setLoading(true);
    GetEmployeeList()
      .then((data) => {
        if (data.isSuccess) {
          setEmployeeList(data.employeesList || []);
          setCurrentPage(1);
        }
      })
      .catch((error) => console.error('Ошибка при получении данных сотрудников:', error))
      .finally(() => setLoading(false));
  }

  const totalPages = Math.ceil(employeeList.length / PAGE_SIZE);
  const startIndex = (currentPage - 1) * PAGE_SIZE;
  const paginatedList = employeeList.slice(startIndex, startIndex + PAGE_SIZE);

  return (
    <Container expand="lg">
      <br />
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 className="mb-1">Список сотрудников</h2>
          <p className="text-muted mb-0">
            Всего: {employeeList.length}
          </p>
        </div>
      </div>

      <Card>
        <Card.Body>
          {loading ? (
            <div className="text-center py-5">
              <Spinner animation="border" />
              <p className="mt-2 mb-0">Загрузка...</p>
            </div>
          ) : (
            <>
              <div className="table-responsive">
                <Table bordered hover>
                  <thead>
                    <tr>
                      <th width="8%">Id</th>
                      <th>ФИО</th>
                      <th width="15%">Дата рождения</th>
                      <th>Объект</th>
                      <th width="20%">Форма оплаты</th>
                      <th>Банк</th>
                    </tr>
                  </thead>
                  <tbody>
                    {paginatedList && paginatedList.length > 0 ? (
                      paginatedList.map((item) => (
                        <tr key={item.id}>
                          <td>{item.id}</td>
                          <td>{item.fio}</td>
                          <td>{item.dateOfBirth ? new Date(item.dateOfBirth).toLocaleDateString('ru-RU') : '—'}</td>
                          <td>{item.objectName ?? item.object ?? '—'}</td>
                          <td>{item.emplOptions ?? '—'}</td>
                          <td>{item.bankName ?? '—'}</td>
                        </tr>
                      ))
                    ) : (
                      <tr>
                        <td colSpan={6} className="text-center text-muted py-4">
                          Нет данных о сотрудниках
                        </td>
                      </tr>
                    )}
                  </tbody>
                </Table>
              </div>

              {totalPages > 1 && (
                <div className="d-flex justify-content-between align-items-center mt-3">
                  <span className="text-muted">
                    Страница {currentPage} из {totalPages}
                    {' '}({startIndex + 1}–{Math.min(startIndex + PAGE_SIZE, employeeList.length)} из {employeeList.length})
                  </span>
                  <Pagination>
                    <Pagination.First
                      onClick={() => setCurrentPage(1)}
                      disabled={currentPage === 1}
                    />
                    <Pagination.Prev
                      onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                      disabled={currentPage === 1}
                    />
                    {Array.from({ length: totalPages }, (_, i) => i + 1)
                      .filter((p) => {
                        if (totalPages <= 7) return true;
                        return Math.abs(p - currentPage) <= 2 || p === 1 || p === totalPages;
                      })
                      .map((p, idx, arr) => (
                        <React.Fragment key={p}>
                          {idx > 0 && arr[idx - 1] !== p - 1 && (
                            <Pagination.Ellipsis disabled />
                          )}
                          <Pagination.Item
                            active={p === currentPage}
                            onClick={() => setCurrentPage(p)}
                          >
                            {p}
                          </Pagination.Item>
                        </React.Fragment>
                      ))}
                    <Pagination.Next
                      onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
                      disabled={currentPage === totalPages}
                    />
                    <Pagination.Last
                      onClick={() => setCurrentPage(totalPages)}
                      disabled={currentPage === totalPages}
                    />
                  </Pagination>
                </div>
              )}
            </>
          )}
        </Card.Body>
      </Card>
    </Container>
  );
}
