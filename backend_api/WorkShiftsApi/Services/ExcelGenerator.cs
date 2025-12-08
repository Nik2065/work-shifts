using System.Data;
using ClosedXML.Excel;


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
                            worksheet.Cell(rowNumber, col + 1).Value = dataTable.Rows[row][col]?.ToString() ?? string.Empty;
                        }
                    }


                }


                // Автоширина столбцов
                worksheet.Columns().AdjustToContents();





                // Заголовки столбцов
                /*for (int i = 0; i < dataTable.Columns.Count; i++)
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
                }*/


                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }


    }

    public class TableDataDto
    {
        public DataTable DataTable { get; set; }

        public string Title { get; set; }
    }
}
