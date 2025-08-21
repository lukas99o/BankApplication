using BankApplication.Data;
using BankApplication.Models;
using BankApplication.Services.IServices;
using Microsoft.EntityFrameworkCore;
using BankApplication.Helpers;

namespace BankApplication
{
    internal class App
    {
        private readonly ApplicationDbContext _db;
        private readonly IAccountService _accountService;
        private readonly IBankAccountService _bankAccountService;
        private readonly IFunService _funService;
        private User? user;
        private DateTime? lockoutUntil;
        private int tries = 3;
        private int failedAttempts = 0;
        private List<BankAccount> userBankAccounts = new();

        public App(ApplicationDbContext db, IAccountService accountService, IBankAccountService bankAccountService, IFunService funService)
        {
            _db = db;
            _accountService = accountService;
            _bankAccountService = bankAccountService;
            _funService = funService;
        }

        public void Run()
        {
            _db.Database.Migrate();
            LoginMenu();

            userBankAccounts = _db.BankAccounts
                    .Where(b => b.UserId == user!.Id)
                    .ToList();

            while (true)
            {
                if (userBankAccounts.Count() == 0)
                {
                    bool account = _bankAccountService.CreateAccount(user!.Id);

                    if (account)
                    {
                        MainMenu();
                    }
                    else
                    {
                        LoginMenu();
                    }
                }
                else
                {
                    MainMenu();
                    LoginMenu();
                }
            }
        }

        public void LoginMenu()
        {
            while(user == null)
            {
                int choice = MenuSystem.MenuInput(
                    new string[] { "VÄLKOMMEN TILL RETROBANK 3000!", "Välj ett av alternativen:" }, 
                    new string[] { "Logga in", "Registrera", "Avsluta" }, 
                    null
                );

                switch (choice)
                {
                    case 0:
                        (user, lockoutUntil, tries, failedAttempts) = _accountService.Login(lockoutUntil, tries, failedAttempts);
                        break;

                    case 1:
                        _accountService.Register();
                        break;

                    case 2:
                        MenuSystem.ExitApplication();
                        break;
                }
            }
        }
        
        public void MainMenu()
        {
            while (user != null)
            {
                int choice = MenuSystem.MenuInput(
                    new[] { "RETRO BANK 3000", $"Inloggad som: {user.Username}", "Välj ett av alternativen:" },
                    new[] { "Visa saldo", "Insättning", "Utdrag", "Överför pengar mellan dina konton", "Överför pengar till någon annans konto",
                            "Visa transaktioner", "Skapa nytt konto", "Visa leaderboard", "Gamble", "Logga ut"
                          },
                    null
                );

                switch (choice)
                {
                    case 0:
                        _bankAccountService.ViewBalance(user.Id);
                        break;
                    case 1:
                        _bankAccountService.Deposit(user.Id);
                        break;
                    case 2:
                        _bankAccountService.Withdraw(user.Id);
                        break;
                    case 3:
                        _bankAccountService.Transfer(user.Id);
                        break;
                    case 4:
                        _bankAccountService.TransferToExternalAccount(user.Id);
                        break;
                    case 5:
                        _bankAccountService.ViewTransactionHistory(user.Id);
                        break;
                    case 6:
                        _bankAccountService.CreateAccount(user.Id);
                        break;
                    case 7:
                        _funService.Leaderboard(user.Id);
                        break;
                    case 8:
                        _funService.Gamble(user.Id);
                        break;
                    case 9:
                        user = MenuSystem.LogOut(user);
                        break;
                }
            }
        }
    }
}