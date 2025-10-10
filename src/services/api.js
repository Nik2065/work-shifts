const baseUrl = ""


export const getEmployees = async () => {
    //const response = await fetch(`${baseUrl}/employee/getEmployees`);
    //return response.json();
    //return MyFetchMockSuccess(getEmployeesResponse);
    return mockFetch(getEmployeesResponse);
}

const getEmployeesResponse = {
    "isError": false,
    "message": "",
    "employees":[
    {
        id: 1,
        fio: "Иванов Иван Иванович",
        object: "Объект В",
        age: "32",
        dateAdd: "2025-01-01",
        status: "Активен",
    },
    {
        id: 2,
        fio: "Смирнов Александр Васильевич",
        object: "Объект А",
        age: "28",
        dateAdd: "2025-01-01",
        status: "Активен",
    },
    {
        id: 3,
        fio: "Новиков Алексей Дмитриевич",
        object: "Объект Б",
        age: "30",
        dateAdd: "2025-01-01",
        status: "Уволен",
    },
    {
        id: 4,
        fio: "Волков Андрей Владимирович",
        object: "Объект В",
        age: "30",
        dateAdd: "2025-01-01",
        status: "Активен",
    },
    {
        id: 5,
        fio: "Кузнецов Дмитрий Алексеевич",
        object: "Объект В",
        age: "49",
        dateAdd: "2025-01-01",
        status: "Активен",
    },
    {
        id: 6,
        fio: "Петров Евгений Викторович",
        object: "Объект В",
        age: "23",
        dateAdd: "2025-01-01",
        status: "Активен",
    }
    ]
}


function MyFetchMockSuccess(data){


    return new Promise((resolve, reject) => {
        setTimeout(() => {
            const o = {
                ok: true,
                status: 200,
                json:() => Promise.resolve( JSON.stringify( data))
            };

            resolve(o);
        }, 2000);
    });
}


// Создаем заглушку, которая возвращает промис с объектом, похожим на Response
function mockFetch(mockData) {
  return Promise.resolve({
    ok: true,
    status: 200,
    json: () => Promise.resolve(mockData),
    // можно добавить другие методы, если они используются: text, blob и т.д.
  });
}





// Заглушка для fetch
const mockApiFetch = (url) => {
  const mockData = {
    '/api/users': [
      { id: 1, name: 'John Doe', email: 'john@example.com' },
      { id: 2, name: 'Jane Smith', email: 'jane@example.com' }
    ],
    '/api/products': [
      { id: 1, title: 'Product 1', price: 100 },
      { id: 2, title: 'Product 2', price: 200 }
    ],
    '/api/user/1': { id: 1, name: 'John Doe', role: 'admin' }
  };

  return Promise.resolve({
    ok: true,
    status: 200,
    json: () => Promise.resolve(mockData[url] || { error: 'Not found' }),
    text: () => Promise.resolve(JSON.stringify(mockData[url] || { error: 'Not found' }))
  });
};