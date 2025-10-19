import { baseUrl } from "./const"; 

export function mockFetch(mockData) {
  return Promise.resolve({
    ok: true,
    status: 200,
    json: () => Promise.resolve(mockData),
    // можно добавить другие методы, если они используются: text, blob и т.д.
  });
}



export async function GetEmployee(employeeId) {

    //return GetEmployeeServer(employeeId);
    return GetEmployeeMock(employeeId);

};



function GetEmployeeMock() {

    const  token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbkBjb21wYW55LmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6ImFkbWluIiwianRpIjoiYTk3NWM1YjItN2UwNS00MjcyLWI3YjUtMjM3YjZjNzE5NDMyIiwiZXhwIjoxNzYwODc3NjAxLCJpc3MiOiJteWFwaS5jb20iLCJhdWQiOiJteWFwaS51c2VycyJ9.iRQw0r4_s8r-owSakai9iwl0QKJbd9Q1WP0zaZTg5Oc";
    localStorage.setItem('token', token);
    setToken(token);

    // Декодируем токен и сохраняем данные пользователя
    const userData = JSON.parse(atob(token.split('.')[1]));
    setUser(userData);
    
    return true;
}