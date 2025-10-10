import React, {useState, useEffect} from "react";
import {Card, Container, Table} from "react-bootstrap";
import {getEmployees} from "../services/api";

 export function EmployeesPage() {

  const[employees, setEmployees] = useState([]);

  useEffect(() => {

    getEmployees()
      .then(response => response.json())
      .then(data => {
        console.log('Employees fetched from API:', data);

        if(!data.isError) {
          setEmployees(data.employees);
        }
        else {
          console.error('Ошибка при получении данных сотрудников:', data.message);
        }
      })
      .catch(error => {
        console.error('Ошибка:', error);
      });

  }, []);

  return (
    

     
    <Card>
      <Card.Body>
        <Card.Title>
        <h4>Список сотрудников</h4>
        </Card.Title>
        
        {
          employees.length > 0 ? (
            <Table bordered hover>
              <thead>
                <tr>
                  <th>Id</th>
                  <th>ФИО</th>
                  <th>Объект</th>
                  <th>Возраст</th>
                  <th>Дата добавления</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {employees.map((employee) => (
                  <tr key={employee.id}>
                    <td>{employee.id}</td>
                    <td>{employee.fio}</td>
                    <td>{employee.object}</td>
                    <td>{employee.age}</td>
                    <td>{employee.dateAdd}</td>
                    <td>{employee.status}</td>
                  </tr>
                ))}
              </tbody>
            </Table>
          ) : 
            <EmptyTable />
        }







      </Card.Body>
    </Card>

    
  );
};


const EmptyTable = () => {
  return (
  <Table bordered hover>
          <thead>
            <tr>
              <th>Id</th>
              <th>ФИО</th>
              <th>Объект</th>
              <th>Возраст</th>
              <th>Дата добавления</th>
              <th></th>
            </tr>
          </thead>
          <tbody>

          </tbody>
        </Table>)
}


export default EmployeesPage;