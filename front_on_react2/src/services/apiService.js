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

export async function GetSiteUser(siteUserId) {
    const url = apiUrl + '/api/user/getuser/?userId=' + siteUserId
    return fetch(url, {
            method: 'GET',
            headers: GetSeqHeaders()
    })

};

export async function GetAllObjects() {
    const url = apiUrl + '/api/user/GetAllObjects';
    return fetch(url, {
            method: 'GET',
            headers: GetSeqHeaders()
    })

};

export async function CreateSiteUserFromApi(params) {
    const url = apiUrl + '/api/user/CreateUser/';
    return fetch(url, {
            method: 'POST',
            headers: GetSeqHeaders(),
            body: JSON.stringify(params)
    })

};

function GetEmployeeMock() {

    return mockFetch({});
}

async function GetEmployeeFromApi(employeeId) {
    const url = apiUrl + '/api/employee/getemployee/?employeeId=' + employeeId;
    //console.log(localStorage.getItem('token'));
    return fetch(url, {
            method: 'GET',
            headers: GetSeqHeaders()
        })
}

async function GetEmployeeListFromApi() {
    const url = apiUrl + '/api/employee/getEmployeeList';

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
    const url = apiUrl + '/api/user/GetUsersList';
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

export async function GetWorkHoursForPeriodApi(params) {
    console.log(params);
    const url = apiUrl + '/api/report/GetWorkHoursForPeriod';

    return fetch(url, {
            method: 'POST',
            headers: GetSeqHeaders(),
            body: JSON.stringify(params)
        }
    );
}
