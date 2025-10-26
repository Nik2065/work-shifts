import { apiUrl } from "./const"; 

const GetHeaders =() => {
    return {
    'Content-Type': 'application/json' };
};

const GetSeqHeaders =() => {
    return {
    'Content-Type': 'application/json',
    'Authorization': 'Bearer ' + localStorage.getItem('token')
    };
};


export function mockFetch(mockData) {
  return Promise.resolve({
    ok: true,
    status: 200,
    json: () => Promise.resolve(mockData),
    // можно добавить другие методы, если они используются: text, blob и т.д.
  });
}



export async function GetEmployee(employeeId) {

    return GetEmployeeFromApi(employeeId);
    //return GetEmployeeMock(employeeId);

};

export async function GetEmployeeList() {

    return GetEmployeeListFromApi();
    //return GetEmployeeMock();

};



function GetEmployeeMock() {

    return mockFetch({});
}

/*async function GetEmployeeFromApi(employeeId) {
    try {
        const response = await fetch(`${apiUrl}/api/employee/?employeeId=${employeeId}`);
        if (!response.ok) {
            throw new Error(`Ошибка: ${response.status}`);
        }
        return response.json();
    } catch (error) {
        console.error('Ошибка при получении данных сотрудника:', error);
        throw error;
    }
}*/

async function GetEmployeeFromApi(employeeId) {
    const url = apiUrl + '/api/employee/getemployee/?employeeId=' + employeeId;
    //console.log(localStorage.getItem('token'));
    return fetch(url, {
            method: 'GET',
            headers: GetSeqHeaders()
        })
}

async function GetEmployeeListFromApi() {
    /*try {
        const response = await fetch(`${apiUrl}/api/employee/getEmployeeList}`);
        if (!response.ok) {
            throw new Error(`Ошибка: ${response.status}`);
        }
        return response.json();
    } catch (error) {
        console.error('Ошибка при получении данных сотрудников:', error);
        throw error;
    }*/

    const url = apiUrl + '/api/employee/getEmployeeList';

    //console.log(localStorage.getItem('token'));

    return fetch(url, {
            method: 'GET',
            headers: GetSeqHeaders(),
        }
    );
}


export async function GetEmployeeWorkShifts(employeeId) {

    const url = apiUrl + '/api/employee/getEmployeeWorkShifts?employeeId=' + employeeId;
    //console.log(localStorage.getItem('token'));
    return fetch(url, {
            method: 'GET',
            headers: GetSeqHeaders(),
        }
    );

}


export async function CreateEmployee(params) {
    console.log(params);
    const url = apiUrl + '/api/employee/createEmployee';

    return fetch(url, {
            method: 'POST',
            headers: GetSeqHeaders(),
            body: JSON.stringify(params)
        }
    );
}

export async function GetSiteUsersList() {
    const url = apiUrl + '/api/auth/GetUsersList';
        return fetch(url, {
            method: 'GET',
            headers: GetSeqHeaders(),
        }
    );
}

export async function GetWorkHoursList(date) {
    const url = apiUrl + '/api/employee/GetWorkHours?date=' + new Date(date).toISOString();
        return fetch(url, {
            method: 'GET',
            headers: GetSeqHeaders(),
        }
    );
}

export async function SaveWorkHoursItemOnServer(params) {
    console.log(params);
    const url = apiUrl + '/api/employee/SaveWorkHoursItem';

    return fetch(url, {
            method: 'POST',
            headers: GetSeqHeaders(),
            body: JSON.stringify(params)
        }
    );
}

