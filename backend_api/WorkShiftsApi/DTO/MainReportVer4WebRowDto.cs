using System.Text.Json.Serialization;

namespace WorkShiftsApi.DTO
{
    /// <summary>
    /// Строка отчёта Ver4 для отображения на сайте (структура совпадает с Excel GetMainReportVer4AsXls).
    /// </summary>
    public class MainReportVer4WebRowDto
    {
        [JsonPropertyName("kind")]
        public string Kind { get; set; } = "data";

        /// <summary>Всегда 14 ячеек, как столбцы A–N в Excel.</summary>
        [JsonPropertyName("cells")]
        public string[] Cells { get; set; } = new string[14];
    }
}
