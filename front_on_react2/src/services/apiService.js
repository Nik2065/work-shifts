import { apiUrl } from "./const"; 
import { Navigate } from "react-router-dom";

import {converDateToIsoStringWithTimeZone} from './commonService.js'; 

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

export async function GetEmployeeList(objectId) {

    return GetEmployeeListFromApi(objectId);
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
}

export async function SaveObjectFromApi(params) {
    const url = apiUrl + '/api/user/SaveObject';
    const response = await authenticatedFetch(url, {
        method: 'POST',
        body: JSON.stringify(params),
    });
    return parseJSON(response);
}

export async function DeleteObjectFromApi(id) {
    const url = apiUrl + '/api/user/DeleteObject';
    const response = await authenticatedFetch(url, {
        method: 'POST',
        body: JSON.stringify({ id }),
    });
    return parseJSON(response);
}

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
    const response = await authenticatedFetch(url);
    return parseJSON(response);
}

async function GetEmployeeListFromApi(objectId) {
    let url = apiUrl + '/api/employee/getEmployeeList?objectId=';
    if(objectId)
        url = url + objectId;
    const response = await authenticatedFetch(url);
    return parseJSON(response);
}

export async function GetEmployeeWithFinOpListFromApi(params) {
    const url = apiUrl + '/api/employee/GetEmployeeWithFinOpList?date=' 
    + converDateToIsoStringWithTimeZone(params.date) + "&objectId=" 
    + params.objectId 
    + "&isInWorkShift=" + params.isInWorkShift;
    const response = await authenticatedFetch(url);
    return parseJSON(response);
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

    let url = apiUrl + '/api/employee/GetWorkHours?date=';
    if(date)
     url = url + new Date(date).toISOString();
    
    const response = await authenticatedFetch(url);
    return parseJSON(response);
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

//отчет для одного сотрудника
export async function GetWorkHoursForPeriodApi(params) {
    console.log(params);
    const url = apiUrl + '/api/report/GetWorkHoursForPeriod';
    const response = await authenticatedFetch(url, {
            method: 'POST',
            body: JSON.stringify(params)
    });
    return parseJSON(response);
}

//отчет для списка сотрудников для отображения на странице
export async function GetMainReportForPeriodAsTable(params) {
    
    console.log(params);
    const url = apiUrl + '/api/report/GetMainReportForPeriodAsTable?startDate=' 
    + params.startDate 
    + '&endDate=' + params.endDate + '&employees=' + params.employees;
    const response = await authenticatedFetch(url);
    return parseJSON(response);

}

//отчет для списка сотрудников для отображения на странице. версия 2
export async function GetMainReportForPeriodAsTableWithBanks(params) {
    
    console.log(params);
    const url = apiUrl + '/api/report/GetMainReportForPeriodAsTableWithBanks?startDate=' 
    + params.startDate 
    + '&endDate=' + params.endDate + '&employees=' + params.employees;
    const response = await authenticatedFetch(url);
    return parseJSON(response);

}


 export async function DownloadFileWithAuth(url, filename = 'file') {
        
    try {
            console.log(url);
            const response = await authenticatedFetch(url);
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            const blob = await response.blob();
            const downloadUrl = window.URL.createObjectURL(blob);
            
            // Создаем временную ссылку для скачивания
            const link = document.createElement('a');
            link.href = downloadUrl;
            link.download = filename;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            
            // Освобождаем память
            window.URL.revokeObjectURL(downloadUrl);
    } catch (error) {
        console.error('Download failed:', error);
    }
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

export async function GetWorkShifts(params){
    
    try {
        const url = apiUrl + '/api/employee/GetWorkShifts?employeeId=' + params.employeeId;
        const response = await authenticatedFetch(url, "GET");
        return parseJSON(response);
    } catch (error) {
        console.error('Error fetching work shifts:', error);
        return [];
    }
}


export async function GetFinOperationTypesFromApi() {
    const url = apiUrl + '/api/employee/GetFinOperationTypes';
    const response = await authenticatedFetch(url);
    return parseJSON(response);
}