namespace WorkShiftsApi
{
    public enum FinOperationTypeEnum
    {
        //списания
        Shtraf = 1, //штраф
        Forma = 2, //форта
        Ucho = 3, //Учо
        Other = 5, //другое списание
        AdvancePaymentPrevPeriod = 8, //был аванс в предыдущем периоде

        //начисления
        OtherPayroll = 6, //другие начисления 
        AdvancePayment = 7,//аванс

    }
}
