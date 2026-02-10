import React, { useState, useEffect } from 'react';
import { Modal, Button, Form, Alert } from 'react-bootstrap';
import { SaveObjectFromApi } from '../../services/apiService';

export function ModalForObject({ show, onHide, objectItem, onSaved }) {
  const [name, setName] = useState('');
  const [address, setAddress] = useState('');
  const [saving, setSaving] = useState(false);
  const [alertData, setAlertData] = useState({ show: false, message: '', variant: 'success' });

  useEffect(() => {
    if (objectItem) {
      setName(objectItem.name ?? '');
      setAddress(objectItem.address ?? '');
    }
    setAlertData({ show: false, message: '', variant: 'success' });
  }, [objectItem, show]);

  function handleSubmit(e) {
    e.preventDefault();
    setAlertData({ show: false, message: '', variant: 'success' });
    if (!name || name.trim().length < 2) {
      setAlertData({ show: true, message: 'Наименование должно быть не менее 2 символов', variant: 'danger' });
      return;
    }
    setSaving(true);
    SaveObjectFromApi({
      id: objectItem.id,
      name: name.trim(),
      address: (address || '').trim(),
    })
      .then((data) => {
        if (data.isSuccess) {
          setAlertData({ show: true, message: data.message, variant: 'success' });
          onSaved?.();
          setTimeout(() => onHide(), 500);
        } else {
          setAlertData({ show: true, message: data.message || 'Ошибка сохранения', variant: 'danger' });
        }
      })
      .catch((err) => {
        setAlertData({ show: true, message: err.message || 'Ошибка сохранения', variant: 'danger' });
      })
      .finally(() => setSaving(false));
  }

  return (
    <Modal show={show} onHide={onHide}>
      <Modal.Header closeButton>
        <Modal.Title>Редактировать объект</Modal.Title>
      </Modal.Header>
      <Form onSubmit={handleSubmit}>
        <Modal.Body>
          <Form.Group className="mb-3">
            <Form.Label>Наименование</Form.Label>
            <Form.Control
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Введите наименование объекта"
            />
          </Form.Group>
          <Form.Group className="mb-3">
            <Form.Label>Адрес</Form.Label>
            <Form.Control
              type="text"
              value={address}
              onChange={(e) => setAddress(e.target.value)}
              placeholder="Введите адрес"
            />
          </Form.Group>
          {alertData.show && (
            <Alert variant={alertData.variant} onClose={() => setAlertData((a) => ({ ...a, show: false }))} dismissible>
              {alertData.message}
            </Alert>
          )}
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={onHide} type="button">
            Отмена
          </Button>
          <Button variant="primary" type="submit" disabled={saving}>
            {saving ? 'Сохранение…' : 'Сохранить'}
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  );
}
