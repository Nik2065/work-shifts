import { baseUrl } from "./const"; 



export async function Login(credentials) {

    return Login(credentials);
    //return LoginMock(credentials);

};

async function Login(credentials) {
        try {
          // Отправляем запрос к серверу для авторизации
          const response = await fetch(baseUrl + '/api/auth/login', {
          method: 'POST',
          headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(credentials),
      });

      if (response.ok) {
        const { token } = await response.json();
        
        // Сохраняем токен в localStorage и состоянии
        localStorage.setItem('token', token);
        setToken(token);
        
        // Декодируем токен и сохраняем данные пользователя
        const userData = JSON.parse(atob(token.split('.')[1]));
        setUser(userData);
        
        return true;
      }
      return false;
    } catch (error) {
      console.error('Login error:', error);
      return false;
    }
}


function LoginMock() {

    const  token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbkBjb21wYW55LmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6ImFkbWluIiwianRpIjoiYTk3NWM1YjItN2UwNS00MjcyLWI3YjUtMjM3YjZjNzE5NDMyIiwiZXhwIjoxNzYwODc3NjAxLCJpc3MiOiJteWFwaS5jb20iLCJhdWQiOiJteWFwaS51c2VycyJ9.iRQw0r4_s8r-owSakai9iwl0QKJbd9Q1WP0zaZTg5Oc";
    localStorage.setItem('token', token);
    setToken(token);

    // Декодируем токен и сохраняем данные пользователя
    const userData = JSON.parse(atob(token.split('.')[1]));
    setUser(userData);
    
    return true;
}