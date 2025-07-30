using BankApplication.Data;
using BankApplication.Models;
using BankApplication.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BankApplication.Services
{
    internal class FunService : IFunService
    {
        private ApplicationDbContext _context;

        public FunService(ApplicationDbContext context) 
        {
            _context = context;
        }

        public void Gamble(int userId)
        {
            var userAccounts = _context.BankAccounts
                .Where(a => a.UserId == userId)
                .ToList();

            if (!userAccounts.Any())
            {
                Console.Clear();
                Console.WriteLine("Du har inga konton att spela med.");
                Console.Write("\nTryck på valfri tangent för att återgå till menyn... ");
                Console.ReadKey();
                return;
            }

            Console.Clear();
            Console.WriteLine("\nVälj ett konto att spela från:");
            foreach (var acc in userAccounts)
            {
                Console.WriteLine($"- {acc.Title} (Saldo: {acc.Balance:C})");
            }

            BankAccount? chosenAccount = null;
            while (chosenAccount == null)
            {
                Console.Write("Ange kontonamn: ");
                var input = Console.ReadLine();
                chosenAccount = userAccounts.SingleOrDefault(a => a.Title.Equals(input, StringComparison.OrdinalIgnoreCase));

                if (chosenAccount == null)
                    Console.WriteLine("Kontot hittades inte. Försök igen.");
            }

            var random = new Random();

            bool keepPlaying = true;
            while (keepPlaying)
            {
                if (chosenAccount.Balance <= 0)
                {
                    Console.WriteLine("❌ Du har inte tillräckligt med saldo för att spela.");
                    break;
                }

                decimal amount;
                while (true)
                {
                    Console.Write("\nAnge summa att spela med: ");
                    if (decimal.TryParse(Console.ReadLine(), out amount) && amount > 0 && amount <= chosenAccount.Balance)
                        break;

                    Console.WriteLine("Ogiltigt belopp. Beloppet måste vara positivt och inte överstiga saldot.");
                }

                Console.Write($"Du spelar {amount:C}. Vill du fortsätta? (true/false): ");
                if (!bool.TryParse(Console.ReadLine(), out bool confirm) || !confirm)
                {
                    Console.WriteLine("🎲 Spelet avbröts.");
                    continue;
                }

                var odds = new List<(string Outcome, double Multiplier, int Chance)>
                {
                    ("JACKPOT", 10.0, 3),
                    ("TRIPLE", 3.0, 11),
                    ("DOUBLE", 2.0, 27),
                    ("HALVE", 0.5, 50),
                    ("ZERO", 0.0, 9)
                };

                int roll = random.Next(1, 101); 
                int cumulative = 0;
                string result = "ZERO";
                double multiplier = 0;

                foreach (var option in odds)
                {
                    cumulative += option.Chance;
                    if (roll <= cumulative)
                    {
                        result = option.Outcome;
                        multiplier = option.Multiplier;
                        break;
                    }
                }

                decimal resultAmount = amount * (decimal)multiplier;
                decimal balanceChange = resultAmount - amount;

                chosenAccount.Balance += balanceChange;

                chosenAccount.Transactions.Add(new Transaction
                {
                    Amount = Math.Abs(balanceChange),
                    Date = DateTime.Now,
                    Sender = balanceChange < 0,
                    BankAccountTitle = chosenAccount.Title,
                    Type = Transaction.TransactionType.Transfer,
                    OtherBankAccountTitle = $"🎰 {result}",
                    BankAccountId = chosenAccount.Id
                });

                _context.SaveChanges();

                Console.WriteLine($"\n🎲 Resultat: {result} ({multiplier}x)");

                if (balanceChange > 0)
                    Console.WriteLine($"🎉 Du vann {balanceChange:C}! Nytt saldo: {chosenAccount.Balance:C}");
                else if (balanceChange < 0)
                    Console.WriteLine($"😢 Du förlorade {-balanceChange:C}. Nytt saldo: {chosenAccount.Balance:C}");
                else
                    Console.WriteLine($"😐 Din insats var värdelös. Du förlorade hela beloppet. Nytt saldo: {chosenAccount.Balance:C}");

                Console.Write("\n🎰 Vill du spela igen? (true/false): ");
                if (!bool.TryParse(Console.ReadLine(), out keepPlaying) || !keepPlaying)
                {
                    Console.WriteLine("🎮 Tack för att du spelade!");
                    break;
                }
            }

            Console.WriteLine("\nTryck på valfri tangent för att återgå till menyn...");
            Console.ReadKey();
        }

        public void Leaderboard(int userId)
        {
            var usersWithAccounts = _context.Users
                .Include(u => u.Accounts)
                .ToList();

            var userBalances = usersWithAccounts
                .Select(user => new
                {
                    user.Username,
                    TotalBalance = user.Accounts.Sum(account => (double)account.Balance) 
                })
                .ToList();

            if (!userBalances.Any())
            {
                Console.WriteLine("Inga användare med konton hittades.");
                return;
            }

            Console.WriteLine("Vill du sortera från högst till lägst? (true/false): ");
            var input = Console.ReadLine();

            bool descending = true;
            if (bool.TryParse(input, out bool result))
            {
                descending = result;
            }

            var sortedUsers = descending
                ? userBalances.OrderByDescending(u => u.TotalBalance)
                : userBalances.OrderBy(u => u.TotalBalance);

            Console.WriteLine($"\nLeaderboard ({(descending ? "Högst till Lägst" : "Lägst till Högst")}):");
            foreach (var user in sortedUsers)
            {
                Console.WriteLine($"{user.Username}: {user.TotalBalance:C}");
            }

            Console.WriteLine("\nTryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }
    }
}
