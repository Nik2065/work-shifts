

export function getDateFormat1(date) {
    
    if (date == null) {
        return "";
    }

    let result = "";
    
    try {
        let date1 = new Date(date);
        let year = date1.getFullYear();
        let month = GetMonthName(date1.getMonth() + 1);
        let day = date1.getDate();
        result = day + "-" + month + "-" + year;
    } catch (e) {
        result = "";
    }

    return result;
}

function GetMonthName(month) {
    let monthName = "";
    switch (month) {
        case 1:
            monthName = "Янв";
            break;
        case 2:
            monthName = "Фев";
            break;
        case 3:
            monthName = "Март";
            break;
        case 4:
            monthName = "Апр";
            break;
        case 5:
            monthName = "Май";
            break;
        case 6:
            monthName = "Июнь";
            break;
        case 7:
            monthName = "Июль";
            break;
        case 8:
            monthName = "Авг";
            break;
        case 9:
            monthName = "Сент";
            break;
        case 10:
            monthName = "Окт";
            break;
        case 11:
            monthName = "Ноя";
            break;
        case 12:
            monthName = "Дек";
            break;
        default:
            monthName = "";
            break;
    }
    return monthName;
}

export function getRoleName(roleCode) {
    if(roleCode == "admin") {
        return "Администратор";
    } else if(roleCode == "object_manager") {
        return "Начальник объекта";
    } else if(roleCode == "buh") {
    return "Бухгалтерия";
    } else {
        return "Неизвестно";
    }
}