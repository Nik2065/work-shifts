namespace WorkShiftsApi.Services
{
    public interface IAuthService
    {
        Task<SiteUserDb?> AuthenticateAsync(string username, string password);
        //Task<SiteUserDb> RegisterAsync(string username, string password, string? email = null);
        Task<bool> UserExistsAsync(string username);
        string GenerateJwtToken(SiteUserDb user);
    }
}
