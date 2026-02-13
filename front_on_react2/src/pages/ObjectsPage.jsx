import React, { useState, useEffect } from 'react';
import {
  Container, Table, Card, Spinner, Pagination, Button, Modal
} from 'react-bootstrap';
import { GetAllObjects, DeleteObjectFromApi } from '../services/apiService';
import { ModalForObject } from '../components/modal/ModalForObject';
import { Pencil, Trash2, Plus } from 'lucide-react';

const PAGE_SIZE = 50;

export function ObjectsPage() {
  const [objectsList, setObjectsList] = useState([]);
  const [loading, setLoading] = useState(true);
  const [currentPage, setCurrentPage] = useState(1);
  const [editModalShow, setEditModalShow] = useState(false);
  const [editingObject, setEditingObject] = useState(null);
  const [deleteConfirm, setDeleteConfirm] = useState(null);

  useEffect(() => {
    loadObjects();
  }, []);

  function loadObjects() {
    setLoading(true);
    GetAllObjects()
      .then((data) => {
        if (data.isSuccess) {
          setObjectsList(data.objects || []);
          setCurrentPage(1);
        }
      })
      .catch((error) => console.error('Ошибка при получении списка объектов:', error))
      .finally(() => setLoading(false));
  }

  const totalPages = Math.ceil(objectsList.length / PAGE_SIZE);
  const startIndex = (currentPage - 1) * PAGE_SIZE;
  const paginatedList = objectsList.slice(startIndex, startIndex + PAGE_SIZE);

  function openEditModal(item) {
    setEditingObject(item ? { id: item.id, name: item.name, address: item.address } : null);
    setEditModalShow(true);
  }

  function handleDeleteConfirm(obj) {
    setDeleteConfirm(obj);
  }

  function handleDeleteExecute() {
    if (!deleteConfirm) return;
    DeleteObjectFromApi(deleteConfirm.id)
      .then((data) => {
        if (data.isSuccess) {
          loadObjects();
          setDeleteConfirm(null);
        } else {
          alert(data.message || 'Ошибка удаления');
        }
      })
      .catch((err) => {
        alert(err.message || 'Ошибка удаления');
      });
  }

  return (
    <Container expand="lg">
      <br />
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 className="mb-1">Объекты</h2>
          <p className="text-muted mb-0">
            Всего: {objectsList.length}
          </p>
        </div>
        <Button variant="primary" className="d-flex align-items-center" onClick={() => openEditModal(null)}>
          <Plus className="me-2" size={18} />
          Добавить объект
        </Button>
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
                      <th>Наименование</th>
                      <th>Адрес</th>
                      <th width="140px">Действия</th>
                    </tr>
                  </thead>
                  <tbody>
                    {paginatedList && paginatedList.length > 0 ? (
                      paginatedList.map((item) => (
                        <tr key={item.id}>
                          <td width="5%">{item.id}</td>
                          <td width="30%">{item.name ?? '—'}</td>
                          <td width="">{item.address ?? '—'}</td>
                          <td width="">
                            <Button title='Редактировать'
                              variant="outline-primary"
                              size="sm"
                              className="me-1"
                              onClick={() => openEditModal(item)}
                            >
                              <Pencil /> 
                            </Button>
                            <Button
                              title='Удалить'
                              variant="outline-danger"
                              size="sm"
                              onClick={() => handleDeleteConfirm(item)}
                            > <Trash2 />
                              
                            </Button>
                          </td>
                        </tr>
                      ))
                    ) : (
                      <tr>
                        <td colSpan={4} className="text-center text-muted py-4">
                          Нет данных об объектах
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
                    {' '}({startIndex + 1}–{Math.min(startIndex + PAGE_SIZE, objectsList.length)} из {objectsList.length})
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

      <ModalForObject
        show={editModalShow}
        onHide={() => { setEditModalShow(false); setEditingObject(null); }}
        objectItem={editingObject}
        onSaved={loadObjects}
      />

      <Modal show={!!deleteConfirm} onHide={() => setDeleteConfirm(null)}>
        <Modal.Header closeButton>
          <Modal.Title>Удалить объект?</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          {deleteConfirm && (
            <>
              Вы уверены, что хотите удалить объект &laquo;{deleteConfirm.name}&raquo;?
            </>
          )}
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setDeleteConfirm(null)}>
            Отмена
          </Button>
          <Button variant="danger" onClick={handleDeleteExecute}>
            Удалить
          </Button>
        </Modal.Footer>
      </Modal>
    </Container>
  );
}
