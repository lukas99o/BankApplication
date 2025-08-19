using BankApplication.Models;

namespace BankApplication.Services.IServices
{
    internal interface IAccountService
    {
        (User? user, DateTime? lockoutUntil, int tries, int failedAttempts) Login(DateTime? lockoutUntil, int tries, int failedAttempts);
        void Register();
    }
}
