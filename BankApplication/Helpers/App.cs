using BankApplication.Data;
using BankApplication.Models;
using BankApplication.Services;
using BankApplication.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BankApplication.Helpers
{
    internal class App
    {
        private readonly ApplicationDbContext _db;
        private readonly IAccountService _accountService;
        private readonly IBankAccountService _bankAccountService;
        private User user;
        private List<BankAccount> userBankAccounts = new();

        public App(ApplicationDbContext db, IAccountService accountService, IBankAccountService bankAccountService)
        {
            _db = db;
            _accountService = accountService;
            _bankAccountService = bankAccountService;
        }

        public void Run()
        {
            _db.Database.Migrate();

            Console.WriteLine("Välkommen till RetroBank 3000!");
            LoginMenu();

            userBankAccounts = _db.BankAccounts
                .Where(b => b.UserId == user.Id)
                .ToList();

            if (userBankAccounts.Count() == 0)
            {
                _bankAccountService.CreateAccount(user.Id);
            }

            MainMenu();
        }

        public void LoginMenu()
        {
            while(true)
            {
                Console.Clear();
                Console.WriteLine("Välj ett alternativ:");
                Console.WriteLine("1. Logga in");
                Console.WriteLine("2. Registrera");
                Console.WriteLine("3. Avsluta");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        user = _accountService.Login()!;            
                        break;
                    case "2":
                        _accountService.Register();
                        break;
                    case "3":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.Write("Ogiltigt val, försök igen. Tryck på valfri tangent för att återgå till menyn... ");
                        Console.ReadKey();
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
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Välkommen till RetroBank 3000!");
                Console.WriteLine($"Inloggad som: {user.Username}");
                Console.WriteLine("\nVälj ett alternativ:");
                Console.WriteLine("1. Visa saldo");
                Console.WriteLine("2. Sätt in pengar");
                Console.WriteLine("3. Ta ut pengar");
                Console.WriteLine("4. Visa transaktioner");
                Console.WriteLine("5. Skapa nytt konto");
                Console.WriteLine("5. Logga ut");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        _bankAccountService.ViewBalance(user.Id);
                        break;
                    case "2":
                        _bankAccountService.Deposit(user.Id);
                        break;
                    case "5":
                        _bankAccountService.CreateAccount(user.Id);
                        break;
                    default:
                        Console.WriteLine("Ogiltigt val, försök igen. Tryck på valfri tangent för att återgå till menyn... ");
                        Console.ReadKey();
                        break;
                }
            }
            
        }
    }
}
