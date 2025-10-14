import { baseUrl } from "./const"; 
import { mockFetch } from "./api";


export async function GetWorkShiftsByDate(date) {

    if (date == new Date(2025, 9, 10)) {
        return mockFetch(GetWorkShiftsByDateResponse1);
    }
    return mockFetch(GetWorkShiftsByDateResponse1);

}

const GetWorkShiftsByDateResponse1 = {    
    "isError": false,
    "message": "",
    "workShifts":[
        {
            "Id": 1,
            "date": "2023-05-01",
            "hours": 5.5,
            "employee": {
                id: 1,
                fio: "Иванов Иван Иванович",
            }
        }
    ]
};

