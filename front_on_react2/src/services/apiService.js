import { apiUrl } from "./const"; 
import { Navigate } from "react-router-dom";

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


/*function ResponseParser(response) {
        if (response.ok) {
        try {
            return response.json();
        } catch (error) {
            console.error('Error parsing JSON:', error);
            return null;
        }
    }
    else if(response.status === 401){
        //токен умер - переходим на страницу логина 
        Navigate('/login');
    }
    else 
        return null;
}*/

//переадресация на страницу логина
const redirectToLogin = () => {
  localStorage.removeItem('token'); // Очистка токена
  window.location.href = '/login';  // Принудительный редирект
};

// Универсальная обёртка для fetch с обработкой 401
export async function authenticatedFetch(url, options = {}) {
  const response = await fetch(url, {
    ...options,
    headers: GetSeqHeaders(),
  });

  if (response.status === 401) {
    redirectToLogin();
    throw new Error('Unauthorized access - redirected to login');
  }

  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }

  return response;
}


// Парсер JSON, который можно вызывать вне fetch
export async function parseJSON(response) {
  try {
    return await response.json();
  } catch (error) {
    console.error('Error parsing JSON:', error);
    throw error;
  }
}




export async function GetEmployee(employeeId) {

    return GetEmployeeFromApi(employeeId);
    //return GetEmployeeMock(employeeId);

};

export async function GetEmployeeList() {

    return GetEmployeeListFromApi();
    //return GetEmployeeMock();

};

/*export async function GetSiteUser(siteUserId) {
    const url = apiUrl + '/api/user/getuser/?userId=' + siteUserId
    return fetch(url, {
            method: 'GET',
            headers: GetSeqHeaders()
    })

};*/


export async function GetSiteUser(siteUserId) {
    const url = apiUrl + '/api/user/getuser/?userId=' + siteUserId
    const response = await authenticatedFetch(url);
    return parseJSON(response);
};




export async function GetAllObjects() {
    const url = apiUrl + '/api/user/GetAllObjects';
    const response = await authenticatedFetch(url);
    return parseJSON(response);
};

export async function CreateSiteUserFromApi(params) {
    const url = apiUrl + '/api/user/CreateUser/';
    const response = await authenticatedFetch(url, {
            method: 'POST',
            body: JSON.stringify(params)
    });
    return parseJSON(response);
};

export async function SaveSiteUserFromApi(params) {

    if(params.password == null || params.password == undefined)
        params.password = "";

    //console.log("params", params);
    const url = apiUrl + '/api/user/SaveUser/';
    const response = await authenticatedFetch(url, {
            method: 'POST',
            body: JSON.stringify(params)
    });
    return parseJSON(response);
};


export async function SaveEmployeeFromApi(params) {
    const url = apiUrl + '/api/Employee/saveemployee'
    const response = await authenticatedFetch(url, {
            method: 'POST',
            body: JSON.stringify(params)
    });
    return parseJSON(response);
}



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
    const response = await authenticatedFetch(url);
    return parseJSON(response);
}

export async function GetEmployeeWithFinOpListFromApi(params) {
    const url = apiUrl + '/api/employee/GetEmployeeWithFinOpList?date=' + params.date.toISOString()
    + "&objectId=" + params.objectId;
    const response = await authenticatedFetch(url);
    return parseJSON(response);
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

    const response = await authenticatedFetch(url, {
            method: 'POST',
            body: JSON.stringify(params)
        });
    return parseJSON(response);
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


export async function CreateWorkShiftFromApi(params) {
    const url = apiUrl + '/api/Employee/CreateWorkShift'
    const response = await authenticatedFetch(url, {
            method: 'POST',
            body: JSON.stringify(params)
    });
    return parseJSON(response);
}

export async function DeteleWorkShiftFromApi(params) {
    const url = apiUrl + '/api/Employee/DeleteWorkShift'
    const response = await authenticatedFetch(url, {
            method: 'POST',
            body: JSON.stringify(params)
    });
    return parseJSON(response);
}



export async function CreateFinOperationFromApi(params) {
    const url = apiUrl + '/api/Employee/CreateFinOperation'
    const response = await authenticatedFetch(url, {
            method: 'POST',
            body: JSON.stringify(params)
    });
    return parseJSON(response);
}


export async function DeleteFinOperationFromApi(params) {
    const url = apiUrl + '/api/Employee/DeleteFinOperation'
    const response = await authenticatedFetch(url, {
            method: 'POST',
            body: JSON.stringify(params)
    });
    return parseJSON(response);
}


export async function GetFinOperationsFromApi(date) {
    const url = apiUrl + '/api/employee/GetWorkHours?date=' + new Date(date).toISOString();
    const response = await authenticatedFetch(url);
    return parseJSON(response);
}