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

            if (userBankAccounts.Count() == 0)
            {
                _bankAccountService.CreateAccount(user!.Id);
            }

            while (true)
            {
                MainMenu();
                LoginMenu();
            }
        }

        public void LoginMenu()
        {
            while(true)
            {
                Console.Clear();
                int choice = Menu.ShowArrowMenu(new string[] { "Välkommen till RetroBank 3000!", "Välj ett av alternativen:" }, new string[] { "Logga in", "Registrera", "Avsluta" });

                switch (choice)
                {
                    case 0:
                        user = _accountService.Login()!;
                        break;
                    case 1:
                        _accountService.Register();
                        break;
                    case 2:
                        Console.Clear();
                        Console.WriteLine("Tack för att du använde RetroBank 3000! Hejdå!");
                        Environment.Exit(0);
                        break;
                }

                if (user != null)
                {
                    break;
                }
            }
        }
        
        public void MainMenu()
        {
            while (user != null)
            {
                Console.Clear();
                Console.WriteLine("Välkommen till RetroBank 3000!");
                Console.WriteLine($"Inloggad som: {user.Username}");
                Console.WriteLine("\nVälj ett alternativ:");
                Console.WriteLine("1. Visa saldo");
                Console.WriteLine("2. Sätt in pengar");
                Console.WriteLine("3. Ta ut pengar");
                Console.WriteLine("4. Överför pengar mellan dina konton");
                Console.WriteLine("5. Överför pengar till någon annans konto");
                Console.WriteLine("6. Visa transaktioner");
                Console.WriteLine("7. Skapa nytt konto");
                Console.WriteLine("8. Visa leaderboard");
                Console.WriteLine("9. Gamble");
                Console.WriteLine("10. Logga ut");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        _bankAccountService.ViewBalance(user.Id);
                        break;
                    case "2":
                        _bankAccountService.Deposit(user.Id);
                        break;
                    case "3":
                        _bankAccountService.Withdraw(user.Id);
                        break;
                    case "4":
                        _bankAccountService.Transfer(user.Id);
                        break;
                    case "5":
                        _bankAccountService.TransferToExternalAccount(user.Id);
                        break;
                    case "6":
                        _bankAccountService.ViewTransactionHistory(user.Id);
                        break;
                    case "7":
                        _bankAccountService.CreateAccount(user.Id);
                        break;
                    case "8":
                        _funService.Leaderboard(user.Id);
                        break;
                    case "9":
                        _funService.Gamble(user.Id);
                        break;
                    case "10":
                        user = null;
                        Console.WriteLine("Du har loggats ut. Tryck på valfri tangent för att återgå till inloggningsmenyn...");
                        Console.ReadKey();
                        return;
                    default:
                        Console.WriteLine("Ogiltigt val, försök igen. Tryck på valfri tangent för att återgå till menyn... ");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }
}
