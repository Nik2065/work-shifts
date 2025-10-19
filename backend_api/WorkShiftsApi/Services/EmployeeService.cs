namespace WorkShiftsApi.Services
{
    public class EmployeeService
    {

        public EmployeeService(IConfiguration config) 
        {
            _config = config;

        }

        private readonly IConfiguration _config;



    }
}
