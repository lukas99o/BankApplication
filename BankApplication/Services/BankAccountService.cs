using BankApplication.Data;
using BankApplication.Models;
using BankApplication.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BankApplication.Services
{
    internal class BankAccountService : IBankAccountService
    {
        private readonly ApplicationDbContext _context;

        public BankAccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void CreateAccount(int userId)
        {
            Console.Clear();
            string? title;

            while (true)
            {
                Console.Write("Ange kontots titel: ");
                title = Console.ReadLine();

                if (string.IsNullOrEmpty(title))
                {
                    Console.WriteLine("Titeln kan inte vara tom. Försök igen.");
                    continue;
                }

                if (title.Length > 50)
                {
                    Console.WriteLine("Titeln får inte vara längre än 50 tecken. Försök igen.");
                    continue;
                }

                break;
            }

            var account = new BankAccount
            {
                UserId = userId,
                Balance = 0,
                Title = title
            };

            _context.BankAccounts.Add(account);
            _context.SaveChanges();
            Console.WriteLine($"Ditt konto '{title}' har skapats med ID: {account.Id}.");
            Console.WriteLine("\nTryck på valfri tangent för att återgå till menyn...");
            Console.ReadKey();
        }

        public void Deposit(int userId)
        {
            Console.Clear();
            decimal amount;
            BankAccount? account = null;

            while (true)
            {
                Console.Write("Ange kontots titel för insättning: ");
                string? title = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(title))
                {
                    account = _context.Users
                        .Include(u => u.Accounts)
                        .SingleOrDefault(u => u.Id == userId)?
                        .Accounts
                        .SingleOrDefault(a => a.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

                    if (account != null)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Inget konto med den titeln hittades. Försök igen.");
                    }
                }
                else
                {
                    Console.WriteLine("Titeln kan inte vara tom. Försök igen.");
                }
            }

            while (true)
            {
                Console.Write("Ange insättningsbelopp: ");
                string? input = Console.ReadLine();

                if (decimal.TryParse(input, out amount) && amount > 0)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Beloppet måste vara ett positivt nummer. Försök igen.");
                }
            }

            account.Balance += amount;
            _context.SaveChanges();

            Console.WriteLine($"\nInsättning genomförd! {amount:C} har lagts till på kontot \"{account.Title}\".");
            Console.WriteLine($"Nytt saldo: {account.Balance:C}");
            Console.WriteLine("\nTryck på valfri tangent för att återgå till menyn...");
            Console.ReadKey();
        }

        public void Transfer(decimal amount, int targetAccountId)
        {
            throw new NotImplementedException();
        }

        public void ViewBalance(int userId)
        {
            Console.Clear();
            BankAccount? account = null;


            while (true)
            {
                Console.Write("Ange kontots titel för att se saldot: ");
                string? title = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(title))
                {
                    account = _context.Users
                        .Include(u => u.Accounts)
                        .SingleOrDefault(u => u.Id == userId)?
                        .Accounts
                        .SingleOrDefault(a => a.Title.Equals(title, StringComparison.OrdinalIgnoreCase));


                    if (account != null)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Inget konto med den titeln hittades. Försök igen.");
                    }
                }
                else
                {
                    Console.WriteLine("Titeln kan inte vara tom. Försök igen.");
                }
            }

            Console.WriteLine($"\nSaldo för kontot \"{account.Title}\": {account.Balance:C}");
            Console.WriteLine("\nTryck på valfri tangent för att återgå till menyn...");
            Console.ReadKey();
        }

        public void ViewTransactionHistory()
        {
            throw new NotImplementedException();
        }

        public void Withdraw(decimal amount)
        {
            throw new NotImplementedException();
        }
    }
}
