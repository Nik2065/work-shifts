import React, {useState, useEffect} from "react";
import { useAuth } from '../context/AuthContext';
import { 
  Container, Row, Col, 
   Card, Button, 
  Spinner, Table
} from 'react-bootstrap';
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import {GetSiteUser} from '../services/apiService';


export function ProfilePage() {
  
    const { user } = useAuth();
    const [currentUser, setCurrentUser] = useState({
        created: '',
        id: '',
        login: '',
        roleName:''
    });
    const[currentUserLoading, setCurrentUserLoading] = useState(false);

    //console.log("user", user);

    useEffect(() => {
        setCurrentUserLoading(true);

        GetSiteUser(user.Id)
        .then(data => {
            console.log("data", data);

            setCurrentUserLoading(false);
            
            if (data.isSuccess) {
                setCurrentUser(data.user);
            } else {}
        })
        .catch(err => {
            setCurrentUserLoading(false);
            console.log(err);
        });
        }
    , []);

    return (
        <Container expand="lg">
        <br/>
            <div className="d-flex justify-content-between align-items-center mb-4">
              <div>
                <h2 className="mb-1">Данные пользователя</h2>
                <p className="text-muted mb-0"></p>
              </div>
            </div>

            <Card className=" h-100">
                <Card.Body>
                    {
                        
                    currentUserLoading ?
                    <div style={{height:"100px", textAlign:"center"}}>
                        <Spinner/>
                    </div>
                    :
                    <Row>
                    <Col xs={12} sm={12} lg={8} xxl={6}>
                    
                    <Table bordered hover>
                        <tbody>
                        <tr>
                            <td width="50%">Логин</td>
                            <td>{currentUser.login}</td>
                        </tr>
                        <tr>
                            <td >Роль</td>
                            <td>{currentUser.roleName}</td>
                        </tr>
                        <tr>
                            <td >Добавлен</td>
                            <td>{currentUser.created}</td>
                        </tr>
                        <tr>
                            <td >Объекты</td>
                            <td>{
                            currentUser.objectsList ?
                            currentUser.objectsList.map((object) => {
                                return object.name;
                            }).join(', ') : ''
                            }</td>
                        </tr>
                        </tbody>
                    </Table>
                    
                    </Col>
                    <Col></Col>
                    </Row>
                    }
                </Card.Body>
                </Card>
        </Container>
    )



}