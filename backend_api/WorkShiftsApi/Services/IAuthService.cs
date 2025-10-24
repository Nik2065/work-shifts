using Microsoft.AspNetCore.Mvc;
using WorkShiftsApi.DTO;

namespace WorkShiftsApi.Services
{
    public interface IAuthService
    {
        Task<SiteUserDb?> AuthenticateAsync(string username, string password);
        Task<SiteUserDb> RegisterAsync(string username, string password);
        Task<bool> UserExistsAsync(string username);
        string GenerateJwtToken(SiteUserDb user);

        //
        List<SiteUserDto> GetSiteUsersList();
    }


}
