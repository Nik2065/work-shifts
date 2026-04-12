using System.Globalization;
using WorkShiftsApi.Controllers;
using WorkShiftsApi.DTO;

namespace WorkShiftsApi.Services
{
    /// <summary>
    /// Собирает строки отчёта Ver4 — та же разметка, что в <see cref="ExcelGenerator.CreateExcelFromMainReportVer4Table"/>.
    /// </summary>
    public static class MainReportVer4TableBuilder
    {
        private static readonly CultureInfo Ru = CultureInfo.GetCultureInfo("ru-RU");

        public static List<MainReportVer4WebRowDto> Build(MainReportDto reportData)
        {
            var rows = new List<MainReportVer4WebRowDto>();

            var title = string.Format("Отчет за период c {0} по {1} (включительно)",
                reportData.StartDate.ToString("dd/MM/yyyy", Ru),
                reportData.EndDate.AddDays(-1).ToString("dd/MM/yyyy", Ru));
            rows.Add(new MainReportVer4WebRowDto
            {
                Kind = "title",
                Cells = Row14(title)
            });

            rows.Add(new MainReportVer4WebRowDto
            {
                Kind = "sectionBanner",
                Cells = Row14("Расчет по ведомости")
            });

            rows.Add(new MainReportVer4WebRowDto { Kind = "spacer", Cells = Row14() });

            rows.Add(ColumnHeaderRow());

            var vedEmployees = reportData.Employees.Where(x => x.EmplOptions == EmplOptionEnums.Vedomost);
            var notVedEmployees = reportData.Employees.Where(x => x.EmplOptions != EmplOptionEnums.Vedomost);

            var skipFirstBankTitle = false;

            if (!vedEmployees.Any())
            {
                var r = Row14();
                r[0] = "Нет сотрудников для расчета";
                rows.Add(new MainReportVer4WebRowDto { Kind = "noEmployees", Cells = r });
            }
            else
            {
                decimal vedomostSum = 0;

                foreach (var emp in vedEmployees)
                {
                    var finData = reportData.EmployeeFinDatas.FirstOrDefault(x => x.EmployeeId == emp.Id);
                    if (finData == null)
                        continue;
                    decimal employeeSum = 0;

                    if (finData.AdvancePaymentInPeriod)
                    {
                        var avans = finData.FinOperations.FirstOrDefault(x =>
                            x.TypeId == (int)FinOperationTypeEnum.AdvancePayment);
                        var line = Row14();
                        line[0] = emp.Fio ?? "";
                        line[11] = Num(avans?.Sum ?? 0);
                        employeeSum += avans?.Sum ?? 0;
                        rows.Add(new MainReportVer4WebRowDto { Kind = "data", Cells = line });
                    }
                    else
                    {
                        var firstLine = true;
                        foreach (var item in finData.WorkHours)
                        {
                            var line = Row14();
                            if (firstLine)
                            {
                                line[0] = emp.Fio ?? "";
                                firstLine = false;
                            }

                            line[3] = Num(item.Hours);
                            line[4] = Num(item.Rate);
                            var sub = item.Hours * item.Rate;
                            line[12] = Num(sub);
                            employeeSum += sub;
                            rows.Add(new MainReportVer4WebRowDto { Kind = "data", Cells = line });
                        }

                        foreach (var item in finData.WorkDays)
                        {
                            var line = Row14();
                            if (firstLine)
                            {
                                line[0] = emp.Fio ?? "";
                                firstLine = false;
                            }

                            line[1] = Num(item.WorkDaysCount);
                            line[2] = Num(item.Rate);
                            var sub = item.WorkDaysCount * item.Rate;
                            line[12] = Num(sub);
                            employeeSum += item.WorkDaysCount * item.Rate;
                            rows.Add(new MainReportVer4WebRowDto { Kind = "data", Cells = line });
                        }

                        var shtraf = finData.FinOperations.FirstOrDefault(x =>
                            x.TypeId == (int)FinOperationTypeEnum.Shtraf);
                        var forma = finData.FinOperations.FirstOrDefault(x =>
                            x.TypeId == (int)FinOperationTypeEnum.Forma);
                        var ucho = finData.FinOperations.FirstOrDefault(x =>
                            x.TypeId == (int)FinOperationTypeEnum.Ucho);
                        var spisaniaOther = finData.FinOperations.FirstOrDefault(x =>
                            x.TypeId == (int)FinOperationTypeEnum.Other);
                        var avansPrev = finData.FinOperations.FirstOrDefault(x =>
                            x.TypeId == (int)FinOperationTypeEnum.AdvancePaymentPrevPeriod);
                        var payrollOther = finData.FinOperations.FirstOrDefault(x =>
                            x.TypeId == (int)FinOperationTypeEnum.OtherPayroll);

                        var ded = Row14();
                        if (firstLine)
                            ded[0] = emp.Fio ?? "";
                        ded[5] = Num(shtraf?.Sum ?? 0);
                        ded[6] = Num(forma?.Sum ?? 0);
                        ded[7] = Num(ucho?.Sum ?? 0);
                        ded[8] = Num(spisaniaOther?.Sum ?? 0);
                        ded[9] = Num(avansPrev?.Sum ?? 0);
                        ded[10] = Num(payrollOther?.Sum ?? 0);
                        rows.Add(new MainReportVer4WebRowDto { Kind = "data", Cells = ded });

                        employeeSum -= shtraf?.Sum ?? 0;
                        employeeSum -= forma?.Sum ?? 0;
                        employeeSum -= ucho?.Sum ?? 0;
                        employeeSum -= spisaniaOther?.Sum ?? 0;
                        employeeSum -= avansPrev?.Sum ?? 0;
                        employeeSum += payrollOther?.Sum ?? 0;
                    }

                    var subtotal = Row14();
                    subtotal[11] = "Итог по сотруднику:";
                    subtotal[12] = Num(employeeSum);
                    rows.Add(new MainReportVer4WebRowDto { Kind = "employeeTotal", Cells = subtotal });
                    vedomostSum += employeeSum;

                    rows.Add(new MainReportVer4WebRowDto { Kind = "spacer", Cells = Row14() });
                }

                if (notVedEmployees.Any())
                {
                    var firstBank = notVedEmployees.Select(x => x.Bank).Distinct().First();
                    var combined = Row14();
                    combined[0] = "Расчет для карт банка: " + (firstBank?.BankName ?? "");
                    combined[11] = "Общий итог:";
                    combined[12] = Num(vedomostSum);
                    rows.Add(new MainReportVer4WebRowDto { Kind = "grandTotalWithBankTitle", Cells = combined });
                    skipFirstBankTitle = true;
                }
                else
                {
                    var vedGrand = Row14();
                    vedGrand[11] = "Общий итог:";
                    vedGrand[12] = Num(vedomostSum);
                    rows.Add(new MainReportVer4WebRowDto { Kind = "grandTotal", Cells = vedGrand });
                }
            }

            if (notVedEmployees.Any())
            {
                var banksToCount = notVedEmployees.Select(x => x.Bank).Distinct().ToList();

                for (var bi = 0; bi < banksToCount.Count; bi++)
                {
                    var bank = banksToCount[bi];
                    var notVedEmployeesForBank = notVedEmployees.Where(x => x.BankId == bank.Id);

                    if (!(skipFirstBankTitle && bi == 0))
                    {
                        rows.Add(new MainReportVer4WebRowDto
                        {
                            Kind = "bankTitle",
                            Cells = Row14("Расчет для карт банка: " + (bank?.BankName ?? ""))
                        });
                    }

                    rows.Add(ColumnHeaderRow());

                    decimal bankItog = 0;

                    foreach (var emp in notVedEmployeesForBank)
                    {
                        var finData = reportData.EmployeeFinDatas.FirstOrDefault(x => x.EmployeeId == emp.Id);
                        if (finData == null)
                            continue;
                        decimal employeeSum = 0;

                        if (finData.AdvancePaymentInPeriod)
                        {
                            var avans = finData.FinOperations.FirstOrDefault(x =>
                                x.TypeId == (int)FinOperationTypeEnum.AdvancePayment);
                            var line = Row14();
                            line[0] = emp.Fio ?? "";
                            line[11] = Num(avans?.Sum ?? 0);
                            employeeSum += avans?.Sum ?? 0;
                            rows.Add(new MainReportVer4WebRowDto { Kind = "data", Cells = line });
                        }
                        else
                        {
                            var firstLine = true;
                            foreach (var item in finData.WorkHours)
                            {
                                var line = Row14();
                                if (firstLine)
                                {
                                    line[0] = emp.Fio ?? "";
                                    firstLine = false;
                                }

                                line[3] = Num(item.Hours);
                                line[4] = Num(item.Rate);
                                var sub = item.Hours * item.Rate;
                                line[12] = Num(sub);
                                employeeSum += sub;
                                rows.Add(new MainReportVer4WebRowDto { Kind = "data", Cells = line });
                            }

                            foreach (var item in finData.WorkDays)
                            {
                                var line = Row14();
                                if (firstLine)
                                {
                                    line[0] = emp.Fio ?? "";
                                    firstLine = false;
                                }

                                line[1] = Num(item.WorkDaysCount);
                                line[2] = Num(item.Rate);
                                var sub = item.WorkDaysCount * item.Rate;
                                line[12] = Num(sub);
                                employeeSum += item.WorkDaysCount * item.Rate;
                                rows.Add(new MainReportVer4WebRowDto { Kind = "data", Cells = line });
                            }

                            var shtraf = finData.FinOperations.FirstOrDefault(x =>
                                x.TypeId == (int)FinOperationTypeEnum.Shtraf);
                            var forma = finData.FinOperations.FirstOrDefault(x =>
                                x.TypeId == (int)FinOperationTypeEnum.Forma);
                            var ucho = finData.FinOperations.FirstOrDefault(x =>
                                x.TypeId == (int)FinOperationTypeEnum.Ucho);
                            var spisaniaOther = finData.FinOperations.FirstOrDefault(x =>
                                x.TypeId == (int)FinOperationTypeEnum.Other);
                            var avansPrev = finData.FinOperations.FirstOrDefault(x =>
                                x.TypeId == (int)FinOperationTypeEnum.AdvancePaymentPrevPeriod);
                            var payrollOther = finData.FinOperations.FirstOrDefault(x =>
                                x.TypeId == (int)FinOperationTypeEnum.OtherPayroll);

                            var ded = Row14();
                            if (firstLine)
                                ded[0] = emp.Fio ?? "";
                            ded[5] = Num(shtraf?.Sum ?? 0);
                            ded[6] = Num(forma?.Sum ?? 0);
                            ded[7] = Num(ucho?.Sum ?? 0);
                            ded[8] = Num(spisaniaOther?.Sum ?? 0);
                            ded[9] = Num(avansPrev?.Sum ?? 0);
                            ded[10] = Num(payrollOther?.Sum ?? 0);
                            rows.Add(new MainReportVer4WebRowDto { Kind = "data", Cells = ded });

                            employeeSum -= shtraf?.Sum ?? 0;
                            employeeSum -= forma?.Sum ?? 0;
                            employeeSum -= ucho?.Sum ?? 0;
                            employeeSum -= spisaniaOther?.Sum ?? 0;
                            employeeSum -= avansPrev?.Sum ?? 0;
                            employeeSum += payrollOther?.Sum ?? 0;
                        }

                        var subtotal = Row14();
                        subtotal[11] = "Итог по сотруднику:";
                        subtotal[12] = Num(employeeSum);
                        rows.Add(new MainReportVer4WebRowDto { Kind = "employeeTotal", Cells = subtotal });
                        bankItog += employeeSum;

                        rows.Add(new MainReportVer4WebRowDto { Kind = "spacer", Cells = Row14() });
                    }

                    var bankGrand = Row14();
                    bankGrand[11] = "Общий итог:";
                    bankGrand[12] = Num(bankItog);
                    rows.Add(new MainReportVer4WebRowDto { Kind = "grandTotal", Cells = bankGrand });

                    rows.Add(new MainReportVer4WebRowDto { Kind = "spacer", Cells = Row14() });
                }
            }

            rows.Add(new MainReportVer4WebRowDto { Kind = "spacer", Cells = Row14() });
            rows.Add(new MainReportVer4WebRowDto { Kind = "spacer", Cells = Row14() });

            return rows;
        }

        private static MainReportVer4WebRowDto ColumnHeaderRow()
        {
            var c = Row14();
            c[0] = "ФИО";
            c[1] = "Дней";
            c[2] = "Ставка в день";
            c[3] = "Часов";
            c[4] = "Ставка в час";
            c[5] = "Штрафы";
            c[6] = "Форма";
            c[7] = "УЧО";
            c[8] = "Списания:Другое";
            c[9] = "Аванс в предыдущем периоде";
            c[10] = "Начисления:Другое";
            c[11] = "Аванс";
            c[12] = "Итого";
            c[13] = "Подпись сотрудника";
            return new MainReportVer4WebRowDto { Kind = "columnHeader", Cells = c };
        }

        private static string[] Row14(string? onlyFirst = null)
        {
            var r = new string[14];
            for (var i = 0; i < 14; i++)
                r[i] = "";
            if (onlyFirst != null)
                r[0] = onlyFirst;
            return r;
        }

        private static string Num(decimal v) => v.ToString("0.##", Ru);

        private static string Num(int v) => v.ToString(Ru);
    }
}
