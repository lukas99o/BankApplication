using BankApplication.Data;
using BankApplication.Helpers;
using BankApplication.Models;
using BankApplication.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BankApplication.Services
{
    internal class FunService : IFunService
    {
        private ApplicationDbContext _context;
        private static readonly Random _rng = new Random();
        public FunService(ApplicationDbContext context) 
        {
            _context = context;
        }

        public void Gamble(int userId)
        {
            while (true)
            {
                var userAccounts = _context.BankAccounts
                .Where(a => a.UserId == userId)
                .ToList();

                int choice = MenuSystem.MenuInput(
                    new[] { "🎰 Välkommen till Spelautomaten! 🎰" },
                    new[] { "Spela", "Avbryt" },
                    null
                );
                if (choice == 1) return;

                string[] accountStrings = userAccounts
                    .Select(a => $"Titel: [{a.Title}] Kontosaldo: [{a.Balance:C}]")
                    .Concat(new[] { "Avbryt" })
                    .ToArray();

                choice = MenuSystem.MenuInput(
                    new[] { "🎰 Välj ett konto att spela från: 🎰" },
                    accountStrings,
                    null
                );

                if (choice == accountStrings.Length - 1)
                {
                    MenuSystem.MenuInput(
                        new[] { "🎰 Avbröt spelautomaten 🎰" },
                        new[] { "Meny" },
                        null
                    );
                    return;
                }

                var chosenAccount = userAccounts[choice];
                if (chosenAccount.Balance <= 0)
                {
                    MenuSystem.MenuInput(
                        new[] { "❌ Du har inte tillräckligt med saldo för att spela." },
                        new[] { "Meny" },
                        null
                    );
                    return;
                }

                var random = new Random();
                bool keepPlaying = true;

                while (keepPlaying)
                {
                    choice = MenuSystem.MenuInput(
                        new[] { $"Titel: [{chosenAccount.Title}] Kontosaldo: [{chosenAccount.Balance:C}]", "Välj insättningsbelopp" },
                        new[] { "100 kr", "200 kr", "500 kr", "1 000 kr", "2 000 kr",
                                "5 000 kr", "10 000 kr", "Ange manuellt", "Avbryt" },
                        null
                    );

                    if (choice == 8)
                    {
                        MenuSystem.MenuInput(
                            new[] { "🎰 Avbröt spelautomaten 🎰" },
                            new[] { "Meny" },
                            null
                        );
                        return;
                    }

                    decimal amount = choice switch
                    {
                        0 => 100m,
                        1 => 200m,
                        2 => 500m,
                        3 => 1000m,
                        4 => 2000m,
                        5 => 5000m,
                        6 => 10000m,
                        _ => 0m
                    };

                    if (choice == 7)
                    {
                        while (true)
                        {
                            MenuSystem.CenterY(8);
                            MenuSystem.Header();
                            MenuSystem.WriteCenteredXForeground("Tryck på [ESC] för att avbryta.", ConsoleColor.Green, false);
                            MenuSystem.WriteCenteredXForeground("Ange insättningsbelopp att spela med: ", ConsoleColor.Yellow, true);
                            string? input = MenuSystem.ReadNumberWithEscape();

                            if (input == null)
                            {
                                MenuSystem.MenuInput(
                                    new[] { "Insättning avbröts." },
                                    new[] { "Meny" },
                                    null
                                );
                                return;
                            }

                            if (input == "")
                            {
                                choice = MenuSystem.MenuInput(
                                    new[] { "Beloppet kan inte vara tomt. Försök igen." },
                                    new[] { "Försök igen", "Meny" },
                                    ConsoleColor.Red
                                );

                                if (choice == 0) continue;
                                return;
                            }

                            if (!decimal.TryParse(input, out amount) || amount <= 0)
                            {
                                choice = MenuSystem.MenuInput(
                                    new[] { "Ogiltigt belopp. Försök igen." },
                                    new[] { "Försök igen", "Meny" },
                                    ConsoleColor.Red
                                );
                                if (choice == 0) continue;
                                return;
                            }

                            break;
                        }
                    }

                    if (amount > chosenAccount.Balance)
                    {
                        choice = MenuSystem.MenuInput(
                            new[] { "❌ Du har inte tillräckligt med saldo för att spela." },
                            new[] { "Försök igen", "Meny" },
                            ConsoleColor.Red
                        );
                        if (choice == 1) return;
                        continue;
                    }

                    choice = MenuSystem.MenuInput(
                        new[] { $"🎰 Du har valt att spela med {amount:C} på kontot: {chosenAccount.Title} 🎰" },
                        new[] { "Spela", "Avbryt" },
                        null
                    );

                    if (choice == 1)
                    {
                        choice = MenuSystem.MenuInput(
                            new[] { "🎰 Avbröt spelautomaten 🎰" },
                            new[] { "Försök igen", "Meny" },
                            null
                        );
                        if (choice == 0) continue;
                        return;
                    }

                    var odds = new List<(string Outcome, decimal Multiplier, int Chance)>
                    {
                        ("JACKPOT", 10.0m, 3),
                        ("TRIPLE", 3.0m, 11),
                        ("DOUBLE", 2.0m, 27),
                        ("HALVE", 0.5m, 50),
                        ("ZERO", 0.0m, 9)
                    };

                    int total = odds.Sum(o => o.Chance);
                    int roll = _rng.Next(0, total); 

                    int acc = 0;
                    string result = "ZERO";
                    decimal multiplier = 0m;

                    foreach (var o in odds)
                    {
                        acc += o.Chance;
                        if (roll < acc)
                        {
                            result = o.Outcome;
                            multiplier = o.Multiplier;
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
                    
                    string resultString = multiplier switch
                    {
                        10.0m => $"JACKPOT! 🎉 Du vann {balanceChange:C}! Nytt saldo: {chosenAccount.Balance:C}",
                        3.0m => $"TRIPLE! 🎉 Du vann {balanceChange:C}! Nytt saldo: {chosenAccount.Balance:C}",
                        2.0m => $"DOUBLE! 🎉 Du vann {balanceChange:C}! Nytt saldo: {chosenAccount.Balance:C}",
                        0.5m => $"HALVE! 😢 Du förlorade hälften: {balanceChange:C}. Nytt saldo: {chosenAccount.Balance:C}",
                        _ => $"ZERO! 😐 Du förlorade allt: {balanceChange:C}. Nytt saldo: {chosenAccount.Balance:C}"
                    };

                    choice = MenuSystem.MenuInput(
                        new[] { $"Resultat: [{result}] [{multiplier}x]", resultString, "🎰 Vill du spela igen?" },
                        new[] { "Ja", "Nej" },
                        null
                    );
                    if (choice == 1) keepPlaying = false;
                }

                MenuSystem.MenuInput(
                    new[] { "🎰 Tack för att du spelade! 🎰" },
                    new[] { "Meny" },
                    null
                );
                return;
            }
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
                .OrderByDescending(u => u.TotalBalance)
                .ToList();

            bool keepViewing = true;
            bool ascending = false;

            while (keepViewing)
            {
                var leaderboardStrings = new List<string>
                {
                    "🏆 Leaderboard 🏆",
                    "Här är användarnas kontosaldon:"
                };

                int rank = 1;
                foreach (var user in userBalances)
                {
                    leaderboardStrings.Add($"{rank}. {user.Username} – {user.TotalBalance:C}");
                    rank++;
                }

                int choice = MenuSystem.MenuInput(
                    leaderboardStrings.ToArray(),
                    new[] { "Sortera", "Meny" },
                    null
                );

                if (choice == 0)
                {
                    ascending = !ascending;
                    userBalances = ascending
                        ? userBalances.OrderBy(u => u.TotalBalance).ToList()
                        : userBalances.OrderByDescending(u => u.TotalBalance).ToList();
                }
                else
                {
                    keepViewing = false;
                }
            }
        }

    }
}
