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
                    reportData.EndDate.ToString("dd/MM/yyyy"));

                cell11.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                cell11.Style.Font.FontSize = 14;
                worksheet.Range("A1:K1").Merge();
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
                worksheet.Range("A2:L2").Merge();

            }
            //если пользователей по ведомости нет то выводим надпись 
            var vedEmployees = reportData.Employees.Where(x => x.EmplOptions == EmplOptionEnums.Vedomost);
            var notVedEmployees = reportData.Employees.Where(x => x.EmplOptions != EmplOptionEnums.Vedomost);
            var currentRow = 4;
            
            {
                for (int i = 1; i <= 12; i++)
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
                worksheet.Cell(currentRow, 10).Value = "Начисления:Другое";
                worksheet.Cell(currentRow, 11).Value = "Итого";
                worksheet.Cell(currentRow, 12).Value = "Подпись сотрудника";

            }

            currentRow += 1;
            if (!vedEmployees.Any())
            {
                //todo: объединить ячейки
                worksheet.Cell(currentRow, 1).Value = "Нет сотрудников для расчета";
                worksheet.Range(currentRow, 1, currentRow, 11).Merge();
            }
            else
            {
                foreach (var emp in vedEmployees)
                {
                    worksheet.Cell(currentRow, 1).Value = emp.Fio;
                    //количество дней смотрим по одной таблице
                    var finData = reportData.EmployeeFinDatas.FirstOrDefault(x => x.EmployeeId == emp.Id);
                    //var days = finData?.NotPayedWorkDays;
                    //worksheet.Cell(currentRow + 1, 2).Value = days?.ToString();

                    //.............................................................................................
                    //тут может быть несколько ставок в час т.е. будет несколько строк для одного сотрудника
                    //теперь добавляем несколько строк со ставками
                    foreach (var item in finData.NotPayedWorkHours)
                    {
                        worksheet.Cell(currentRow, 4).Value = item.Hours;
                        worksheet.Cell(currentRow, 5).Value = item.Rate;
                        worksheet.Cell(currentRow, 11).Value = item.Hours * item.Rate;
                        currentRow += 1;
                    }
                    //.............................................................................................

                    //теперь добавляем несколько строк со ставками
                    foreach (var item in finData.NotPayedWorkDays)
                    {
                        worksheet.Cell(currentRow, 2).Value = item.WorkDaysCount;
                        worksheet.Cell(currentRow, 3).Value = item.Rate;
                        worksheet.Cell(currentRow, 11).Value = item.WorkDaysCount * item.Rate;
                        currentRow += 1;
                    }


                    //новый сотрудник = доп увеличиваем счетчик строк
                    currentRow += 1;
                }

            }

            //отчет по банкам
            if (notVedEmployees.Any())
            {
                var tmp1 = notVedEmployees.ToList();

                //поазываем только банки по которым есть расчет
                var banksToCount = notVedEmployees.Select(x => x.Bank).Distinct();
                
                foreach (var bank in banksToCount) 
                {
                    //шапка расчета
                    

                    worksheet.Cell(currentRow, 1).Value = "Расчет для карт банка " + bank?.BankName ?? "";
                    worksheet.Cell(currentRow, 1).Style.Font.FontSize = 14;
                    worksheet.Cell(currentRow, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                    currentRow += 1;

                    for (int i = 1; i <= 12; i++)
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
                    worksheet.Cell(currentRow, 10).Value = "Начисления:Другое";
                    worksheet.Cell(currentRow, 11).Value = "Итого";
                    worksheet.Cell(currentRow, 12).Value = "Подпись сотрудника";
                    //завершили оформление шапки
                    currentRow += 1;

                }



            }


            
            
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
