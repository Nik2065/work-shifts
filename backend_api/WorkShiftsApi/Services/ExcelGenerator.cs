using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data;
using System.Globalization;
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
            var rows = MainReportVer4TableBuilder.Build(reportData);
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Лист 1");
            var ru = CultureInfo.GetCultureInfo("ru-RU");
            var r = 1;

            foreach (var row in rows)
            {
                switch (row.Kind)
                {
                    case "title":
                        worksheet.Cell(r, 1).Value = row.Cells[0];
                        worksheet.Cell(r, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                        worksheet.Cell(r, 1).Style.Font.FontSize = 14;
                        worksheet.Range(r, 1, r, 14).Merge();
                        r++;
                        break;

                    case "sectionBanner":
                        var banner = worksheet.Cell(r, 1);
                        banner.Value = row.Cells[0];
                        banner.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                        banner.Style.Font.FontSize = 14;
                        banner.Style.Font.Bold = true;
                        banner.Style.Fill.SetBackgroundColor(XLColor.Yellow);
                        worksheet.Range(r, 1, r, 14).Merge();
                        r++;
                        break;

                    case "spacer":
                        r++;
                        break;

                    case "columnHeader":
                        for (var i = 1; i <= 14; i++)
                        {
                            worksheet.Cell(r, i).Style.Font.Bold = true;
                            worksheet.Cell(r, i).Style.Fill.SetBackgroundColor(XLColor.Yellow);
                            worksheet.Cell(r, i).Value = row.Cells[i - 1];
                        }

                        r++;
                        break;

                    case "noEmployees":
                        worksheet.Cell(r, 1).Value = row.Cells[0];
                        worksheet.Range(r, 1, r, 14).Merge();
                        r++;
                        break;

                    case "bankTitle":
                        worksheet.Cell(r, 1).Value = row.Cells[0];
                        worksheet.Cell(r, 1).Style.Font.FontSize = 14;
                        worksheet.Cell(r, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                        r++;
                        break;

                    case "data":
                    case "employeeTotal":
                        for (var i = 1; i <= 14; i++)
                        {
                            var txt = row.Cells[i - 1];
                            if (string.IsNullOrEmpty(txt))
                                continue;
                            var cell = worksheet.Cell(r, i);
                            if (decimal.TryParse(txt, NumberStyles.Number, ru, out var decVal))
                                cell.Value = decVal;
                            else if (int.TryParse(txt, NumberStyles.Integer, ru, out var intVal))
                                cell.Value = intVal;
                            else
                                cell.Value = txt;
                        }

                        r++;
                        break;

                    case "grandTotalWithBankTitle":
                        worksheet.Cell(r, 1).Value = row.Cells[0];
                        worksheet.Cell(r, 1).Style.Font.FontSize = 14;
                        worksheet.Cell(r, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                        worksheet.Cell(r, 12).Value = row.Cells[11];
                        worksheet.Cell(r, 12).Style.Fill.SetBackgroundColor(XLColor.BlueGray);
                        worksheet.Cell(r, 12).Style.Font.Bold = true;
                        if (decimal.TryParse(row.Cells[12], NumberStyles.Number, ru, out var gtotB))
                            worksheet.Cell(r, 13).Value = gtotB;
                        else
                            worksheet.Cell(r, 13).Value = row.Cells[12];
                        r++;
                        break;

                    case "grandTotal":
                        worksheet.Cell(r, 12).Value = row.Cells[11];
                        worksheet.Cell(r, 12).Style.Fill.SetBackgroundColor(XLColor.BlueGray);
                        worksheet.Cell(r, 12).Style.Font.Bold = true;
                        if (decimal.TryParse(row.Cells[12], NumberStyles.Number, ru, out var gtot))
                            worksheet.Cell(r, 13).Value = gtot;
                        else
                            worksheet.Cell(r, 13).Value = row.Cells[12];
                        r++;
                        break;

                    default:
                        for (var i = 1; i <= 14; i++)
                        {
                            var txt = row.Cells[i - 1];
                            if (string.IsNullOrEmpty(txt))
                                continue;
                            worksheet.Cell(r, i).Value = txt;
                        }

                        r++;
                        break;
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
