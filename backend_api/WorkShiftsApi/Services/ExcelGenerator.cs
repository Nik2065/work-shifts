using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data;
using WorkShiftsApi.Controllers;
using WorkShiftsApi.DTO;


namespace WorkShiftsApi.Services
{
    public class ExcelGenerator
    {
        public byte[] CreateExcelFromDataTable(DataTable dataTable, string sheetName = "Sheet1")
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(sheetName);

                // Заголовки столбцов
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = dataTable.Columns[i].ColumnName;
                    cell.Style.Font.SetBold(true);
                    cell.Style.Fill.SetBackgroundColor(XLColor.LightBlue);
                }

                // Данные
                for (int row = 0; row < dataTable.Rows.Count; row++)
                {
                    for (int col = 0; col < dataTable.Columns.Count; col++)
                    {
                        worksheet.Cell(row + 2, col + 1).Value = dataTable.Rows[row][col]?.ToString() ?? string.Empty;
                    }
                }

                // Автоширина столбцов
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        //Создаем файл из нескольких таблиц 
        public byte[] CreateExcelFromDataTables(List<TableDataDto> tables, string sheetName = "Sheet1")
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(sheetName);


                var allTablesSum = 0;

                int rowNumber = 1;
                foreach (var tData in tables)
                {
                    //заголовок таблицы
                    {
                        var cell = worksheet.Cell(rowNumber, 1);
                        cell.Value = tData.Title;
                        cell.Style.Font.SetBold(true);
                        cell.Style.Fill.SetBackgroundColor(XLColor.Yellow);
                        //todo как объединить ячейки?
                    }

                    var dataTable = tData.DataTable;

                    // Заголовки столбцов
                    rowNumber += 1;
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        var cell = worksheet.Cell(rowNumber, i + 1);
                        cell.Value = dataTable.Columns[i].ColumnName;
                        cell.Style.Font.SetBold(true);
                        cell.Style.Fill.SetBackgroundColor(XLColor.LightBlue);
                    }

                    // Данные
                    for (int row = 0; row < dataTable.Rows.Count; row++)
                    {
                        rowNumber += 1;

                        for (int col = 0; col < dataTable.Columns.Count; col++)
                        {
                            var value = dataTable.Rows[row][col]?.ToString();
                            worksheet.Cell(rowNumber, col + 1).Value = value ?? string.Empty;
                            //считаем итоги. Итоги хранятся в колонке 8
                            /*if (col == 8 && value!= null) {
                                if (int.TryParse(value, out int r1))
                                {
                                    allTablesSum = allTablesSum + r1;
                                }
                            }*/
                        }
                    }

                    allTablesSum += tData.TotalSum;

                    rowNumber += 1;
                }

                //итоговая строка по всем таблицам
                rowNumber += 1;
                {
                    var cellTotal = worksheet.Cell(rowNumber, 8);
                    cellTotal.Value = "Итого (все банки):";
                    cellTotal.Style.Font.SetBold(true);
                    cellTotal.Style.Fill.SetBackgroundColor(XLColor.LightBlue);

                    var cellTotal2 = worksheet.Cell(rowNumber, 9);
                    cellTotal2.Value = allTablesSum;
                    cellTotal2.Style.Font.SetBold(true);
                    cellTotal2.Style.Fill.SetBackgroundColor(XLColor.LightBlue);
                }

                // Автоширина столбцов
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        /// <summary>
        /// Экспорт таблицы отчёта ver.3 (шаг 3 пайплайна: данные → DataTable → файл).
        /// Лист «Отчёт ver 3»: шапка, период, строка заголовков колонок, данные, общий итог.
        /// </summary>
        public byte[] CreateExcelFromMainReportVer3Table(DataTable table)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Отчёт ver 3");

            const int metaRows = 3;
            var totalColName = "Списания: Другое";
            var hasTotalCol = table.Columns.Contains(totalColName);

            for (var row = 0; row < table.Rows.Count; row++)
            {
                var isTotalLabelRow = hasTotalCol
                    && row == table.Rows.Count - 1
                    && table.Rows[row][totalColName]?.ToString() == "Общий итог:";

                for (var col = 0; col < table.Columns.Count; col++)
                {
                    var cell = worksheet.Cell(row + 1, col + 1);
                    var val = table.Rows[row][col];
                    if (val is int i)
                        cell.Value = i;
                    else if (val != null && val != DBNull.Value && int.TryParse(val.ToString(), out var n))
                        cell.Value = n;
                    else
                        cell.Value = val?.ToString() ?? string.Empty;

                    if (row == 0 && col == 0)
                    {
                        cell.Style.Font.SetBold(true);
                        cell.Style.Font.SetFontSize(12);
                    }
                    else if (row == 1 && col == 0)
                        cell.Style.Font.SetItalic(true);
                    else if (row == metaRows)
                    {
                        cell.Style.Font.SetBold(true);
                        cell.Style.Fill.SetBackgroundColor(XLColor.LightBlue);
                    }
                    else if (isTotalLabelRow && col >= 6)
                    {
                        cell.Style.Font.SetBold(true);
                        cell.Style.Fill.SetBackgroundColor(XLColor.LightBlue);
                    }
                }
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }


        public byte[] CreateExcelFromMainReportVer4Table(MainReportDto reportData)
        {
            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Лист 1");

            {
                var cell11 = worksheet.Cell(1, 1);
                cell11.Value = String.Format("Отчет за период c {0} по {1} (включительно)",
                    reportData.StartDate.ToString("dd/MM/yyyy"),
                    reportData.EndDate.AddDays(-1).ToString("dd/MM/yyyy"));//чтобы было включительно - вычитаем 

                cell11.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                cell11.Style.Font.FontSize = 14;
                worksheet.Range("A1:N1").Merge();
            }
            //------
            //заголовок для ведомости
            {
                var cell = worksheet.Cell(2, 1);
                cell.Value = "Расчет по ведомости";

                cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                cell.Style.Font.FontSize = 14;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.SetBackgroundColor(XLColor.Yellow);
                worksheet.Range("A2:N2").Merge();

            }
            //если пользователей по ведомости нет то выводим надпись 
            var vedEmployees = reportData.Employees.Where(x => x.EmplOptions == EmplOptionEnums.Vedomost);
            var notVedEmployees = reportData.Employees.Where(x => x.EmplOptions != EmplOptionEnums.Vedomost);
            var currentRow = 4;

            {
                for (int i = 1; i <= 14; i++)
                {
                    worksheet.Cell(currentRow, i).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, i).Style.Fill.SetBackgroundColor(XLColor.Yellow);
                }


                worksheet.Cell(currentRow, 1).Value = "ФИО";
                worksheet.Cell(currentRow, 2).Value = "Дней";
                worksheet.Cell(currentRow, 3).Value = "Ставка в день";
                worksheet.Cell(currentRow, 4).Value = "Часов";
                worksheet.Cell(currentRow, 5).Value = "Ставка в час";
                worksheet.Cell(currentRow, 6).Value = "Штрафы";
                worksheet.Cell(currentRow, 7).Value = "Форма";
                worksheet.Cell(currentRow, 8).Value = "УЧО";
                worksheet.Cell(currentRow, 9).Value = "Списания:Другое";
                worksheet.Cell(currentRow, 10).Value = "Аванс в предыдущем периоде";
                worksheet.Cell(currentRow, 11).Value = "Начисления:Другое";
                worksheet.Cell(currentRow, 12).Value = "Аванс";
                worksheet.Cell(currentRow, 13).Value = "Итого";
                worksheet.Cell(currentRow, 14).Value = "Подпись сотрудника";
            }

            currentRow += 1;
            if (!vedEmployees.Any())
            {
                //todo: объединить ячейки
                worksheet.Cell(currentRow, 1).Value = "Нет сотрудников для расчета";
                worksheet.Range(currentRow, 1, currentRow, 14).Merge();
            }
            else
            {
                decimal vedomostSum = 0;

                foreach (var emp in vedEmployees)
                {
                    worksheet.Cell(currentRow, 1).Value = emp.Fio;

                    
                    var finData = reportData.EmployeeFinDatas.FirstOrDefault(x => x.EmployeeId == emp.Id);
                    decimal employeeSum = 0;


                    if (finData.AdvancePaymentInPeriod)
                    {
                        //! для аванса отдельная логика
                        //! АВАНС
                        var avans = finData.NotPayedFinOperations.FirstOrDefault(x => x.TypeId == (int)FinOperationTypeEnum.AdvancePayment);
                        worksheet.Cell(currentRow, 12).Value = avans?.Sum ?? 0;
                        employeeSum += avans?.Sum ?? 0;
                    }
                    else
                    {

                        //var days = finData?.NotPayedWorkDays;
                        //worksheet.Cell(currentRow + 1, 2).Value = days?.ToString();

                        //.............................................................................................
                        //тут может быть несколько ставок в час т.е. будет несколько строк для одного сотрудника
                        //теперь добавляем несколько строк со ставками
                        foreach (var item in finData.NotPayedWorkHours)
                        {
                            worksheet.Cell(currentRow, 4).Value = item.Hours;
                            worksheet.Cell(currentRow, 5).Value = item.Rate;
                            worksheet.Cell(currentRow, 13).Value = item.Hours * item.Rate;
                            employeeSum += item.Hours * item.Rate;
                            currentRow += 1;
                        }
                        //.............................................................................................

                        //теперь добавляем несколько строк со ставками смен
                        foreach (var item in finData.NotPayedWorkDays)
                        {
                            worksheet.Cell(currentRow, 2).Value = item.WorkDaysCount;
                            worksheet.Cell(currentRow, 3).Value = item.Rate;
                            worksheet.Cell(currentRow, 13).Value = item.WorkDaysCount * item.Rate;
                            employeeSum += item.WorkDaysCount * item.Rate;
                            currentRow += 1;
                        }

                        //списания.....................................................................................
                        //штраф
                        var shtraf = finData.NotPayedFinOperations.FirstOrDefault(x => x.TypeId == (int)FinOperationTypeEnum.Shtraf);
                        worksheet.Cell(currentRow, 6).Value = shtraf?.Sum ?? 0;
                        //форма
                        var forma = finData.NotPayedFinOperations.FirstOrDefault(x => x.TypeId == (int)FinOperationTypeEnum.Forma);
                        worksheet.Cell(currentRow, 7).Value = forma?.Sum ?? 0;
                        var ucho = finData.NotPayedFinOperations.FirstOrDefault(x => x.TypeId == (int)FinOperationTypeEnum.Ucho);
                        worksheet.Cell(currentRow, 8).Value = ucho?.Sum ?? 0;
                        var spisaniaOther = finData.NotPayedFinOperations.FirstOrDefault(x => x.TypeId == (int)FinOperationTypeEnum.Other);
                        worksheet.Cell(currentRow, 9).Value = spisaniaOther?.Sum ?? 0;
                        //
                        var avansPrev = finData.NotPayedFinOperations.FirstOrDefault(x => x.TypeId == (int)FinOperationTypeEnum.AdvancePaymentPrevPeriod);
                        worksheet.Cell(currentRow, 10).Value = avansPrev?.Sum ?? 0;


                        //начисления
                        var payrollOther = finData.NotPayedFinOperations.FirstOrDefault(x => x.TypeId == (int)FinOperationTypeEnum.OtherPayroll);
                        worksheet.Cell(currentRow, 11).Value = payrollOther?.Sum ?? 0;
                        //вычитаем
                        employeeSum -= shtraf?.Sum ?? 0;
                        employeeSum -= forma?.Sum ?? 0;
                        employeeSum -= ucho?.Sum ?? 0;
                        employeeSum -= spisaniaOther?.Sum ?? 0;
                        employeeSum -= avansPrev?.Sum ?? 0;
                        //суммируем
                        employeeSum += payrollOther?.Sum ?? 0;
                    }


                    //итоги по сотруднику
                    currentRow += 1;
                    worksheet.Cell(currentRow, 12).Value = "Итог по сотруднику:";
                    worksheet.Cell(currentRow, 13).Value = employeeSum;
                    vedomostSum += employeeSum;


                    //новый сотрудник = доп увеличиваем счетчик строк
                    currentRow += 1;
                }

                //итог по ведомости
                worksheet.Cell(currentRow, 12).Value = "Общий итог:";
                worksheet.Cell(currentRow, 12).Style.Fill.SetBackgroundColor(XLColor.BlueGray);
                worksheet.Cell(currentRow, 12).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 13).Value = vedomostSum;

            }

            //отчет по банкам
            if (notVedEmployees.Any())
            {
                var tmp1 = notVedEmployees.ToList();

                //поазываем только банки по которым есть расчет
                var banksToCount = notVedEmployees.Select(x => x.Bank).Distinct();
                decimal bankItog = 0;

                foreach (var bank in banksToCount)
                {
                    //выбираем сотрудников только по этому банку
                    var notVedEmployeesForBank = notVedEmployees.Where(x => x.BankId == bank.Id);

                    //шапка расчета
                    worksheet.Cell(currentRow, 1).Value = "Расчет для карт банка: " + bank?.BankName ?? "";
                    worksheet.Cell(currentRow, 1).Style.Font.FontSize = 14;
                    worksheet.Cell(currentRow, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                    currentRow += 1;

                    for (int i = 1; i <= 14; i++)
                    {
                        worksheet.Cell(currentRow, i).Style.Font.Bold = true;
                        worksheet.Cell(currentRow, i).Style.Fill.SetBackgroundColor(XLColor.Yellow);
                    }
                    worksheet.Cell(currentRow, 1).Value = "ФИО";
                    worksheet.Cell(currentRow, 2).Value = "Дней";
                    worksheet.Cell(currentRow, 3).Value = "Ставка в день";
                    worksheet.Cell(currentRow, 4).Value = "Часов";
                    worksheet.Cell(currentRow, 5).Value = "Ставка в час";
                    worksheet.Cell(currentRow, 6).Value = "Штрафы";
                    worksheet.Cell(currentRow, 7).Value = "Форма";
                    worksheet.Cell(currentRow, 8).Value = "УЧО";
                    worksheet.Cell(currentRow, 9).Value = "Списания:Другое";
                    worksheet.Cell(currentRow, 10).Value = "Аванс в предыдущем периоде";
                    worksheet.Cell(currentRow, 11).Value = "Начисления:Другое";
                    worksheet.Cell(currentRow, 12).Value = "Аванс";
                    worksheet.Cell(currentRow, 13).Value = "Итого";
                    worksheet.Cell(currentRow, 14).Value = "Подпись сотрудника";
                    //завершили оформление шапки
                    currentRow += 1;


                    foreach (var emp in notVedEmployeesForBank)
                    {
                        worksheet.Cell(currentRow, 1).Value = emp.Fio;
                        var finData = reportData.EmployeeFinDatas.FirstOrDefault(x => x.EmployeeId == emp.Id);
                        decimal employeeSum = 0;

                        if (finData.AdvancePaymentInPeriod)
                        {
                            var avans = finData.NotPayedFinOperations.FirstOrDefault(x => x.TypeId == (int)FinOperationTypeEnum.AdvancePayment);
                            worksheet.Cell(currentRow, 12).Value = avans?.Sum ?? 0;
                            employeeSum += avans?.Sum ?? 0;
                        }
                        else
                        {
                            //из таблицы о часах работы
                            foreach (var item in finData.NotPayedWorkHours)
                            {
                                worksheet.Cell(currentRow, 4).Value = item.Hours;
                                worksheet.Cell(currentRow, 5).Value = item.Rate;
                                worksheet.Cell(currentRow, 13).Value = item.Hours * item.Rate;
                                employeeSum += item.Hours * item.Rate;
                                currentRow += 1;
                            }
                            //.............................................................................................
                            //из таблицы о сменах
                            foreach (var item in finData.NotPayedWorkDays)
                            {
                                worksheet.Cell(currentRow, 2).Value = item.WorkDaysCount;
                                worksheet.Cell(currentRow, 3).Value = item.Rate;
                                worksheet.Cell(currentRow, 13).Value = item.WorkDaysCount * item.Rate;
                                employeeSum += item.WorkDaysCount * item.Rate;
                                currentRow += 1;
                            }
                            //списания.....................................................................................
                            //штраф
                            var shtraf = finData.NotPayedFinOperations.FirstOrDefault(x => x.TypeId == (int)FinOperationTypeEnum.Shtraf);
                            worksheet.Cell(currentRow, 6).Value = shtraf?.Sum ?? 0;
                            //форма
                            var forma = finData.NotPayedFinOperations.FirstOrDefault(x => x.TypeId == (int)FinOperationTypeEnum.Forma);
                            worksheet.Cell(currentRow, 7).Value = forma?.Sum ?? 0;
                            var ucho = finData.NotPayedFinOperations.FirstOrDefault(x => x.TypeId == (int)FinOperationTypeEnum.Ucho);
                            worksheet.Cell(currentRow, 8).Value = ucho?.Sum ?? 0;
                            var spisaniaOther = finData.NotPayedFinOperations.FirstOrDefault(x => x.TypeId == (int)FinOperationTypeEnum.Other);
                            worksheet.Cell(currentRow, 9).Value = spisaniaOther?.Sum ?? 0;
                            //
                            var avansPrev = finData.NotPayedFinOperations.FirstOrDefault(x => x.TypeId == (int)FinOperationTypeEnum.AdvancePaymentPrevPeriod);
                            worksheet.Cell(currentRow, 10).Value = avansPrev?.Sum ?? 0;

                            //начисления
                            var payrollOther = finData.NotPayedFinOperations.FirstOrDefault(x => x.TypeId == (int)FinOperationTypeEnum.OtherPayroll);
                            worksheet.Cell(currentRow, 11).Value = payrollOther?.Sum ?? 0;


                            //вычитаем
                            employeeSum -= shtraf?.Sum ?? 0;
                            employeeSum -= forma?.Sum ?? 0;
                            employeeSum -= ucho?.Sum ?? 0;
                            employeeSum -= spisaniaOther?.Sum ?? 0;
                            employeeSum -= avansPrev?.Sum ?? 0;
                            //суммируем
                            employeeSum += payrollOther?.Sum ?? 0;
                        }

                        //итоги по сотруднику
                        currentRow += 1;
                        worksheet.Cell(currentRow, 12).Value = "Итог по сотруднику:";
                        worksheet.Cell(currentRow, 13).Value = employeeSum;
                        bankItog += employeeSum;

                        //новый сотрудник = доп увеличиваем счетчик строк
                        currentRow += 1;
                    }

                    //итог по банку
                    worksheet.Cell(currentRow, 12).Value = "Общий итог:";
                    worksheet.Cell(currentRow, 12).Style.Fill.SetBackgroundColor(XLColor.BlueGray);
                    worksheet.Cell(currentRow, 12).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, 13).Value = bankItog;


                    currentRow += 1;
                }
            }


            //пишем общий итог
            currentRow += 1;
            currentRow += 1;

           

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class TableDataDto
    {
        public DataTable DataTable { get; set; }

        public string Title { get; set; }
        public int TotalSum { get; set; }
    }
}
