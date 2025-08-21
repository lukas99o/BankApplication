using BankApplication.Data;
using BankApplication.Helpers;
using BankApplication.Models;
using BankApplication.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using System.Security.Principal;

namespace BankApplication.Services
{
    internal class BankAccountService : IBankAccountService
    {
        private readonly ApplicationDbContext _context;

        public BankAccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void ViewBalance(int userId)
        {
            var bankAccounts = _context.BankAccounts
                .Where(b => b.UserId == userId)
                .ToList();

            var accountStrings = bankAccounts
                .Select(a => $"Titel: [{a.Title}] Kontosaldo: [{a.Balance} kr]")
                .ToArray();

            MenuSystem.MenuInput(
                accountStrings,
                new[] { "Meny" },
                null
            );
        }

        public bool CreateAccount(int userId)
        {
            var user = _context.Users
                .Include(u => u.Accounts)
                .SingleOrDefault(u => u.Id == userId);

            if (user?.Accounts == null || !user.Accounts.Any())
            {
                int choice = MenuSystem.MenuInput(
                    new[] { "👋 VÄLKOMMEN TILL BANKEN!", "Inga konton hittades.", "Vänligen skapa ditt första konto." },
                    new[] { "Skapa ett nytt konto", "Meny" },
                    null
                );

                if (choice == 1) return false; 
            }

            while (true)
            {
                MenuSystem.CenterY(2);
                MenuSystem.WriteAllCenteredXForeground(
                    new[] { "SKAPA KONTO", "Tryck på [ESC] för att avbryta.", "" },
                    ConsoleColor.Green
                );

                MenuSystem.WriteCenteredXForeground("Ange det nya kontots titel: ", ConsoleColor.Yellow, true);
                string? title = MenuSystem.ReadLineWithEscape();

                if (title == null)
                {
                    MenuSystem.MenuInput(
                        new[] { "Konto skapande avbröts." },
                        new[] { "Meny" },
                        null
                    );

                    return false;
                }

                if (title == "")
                {
                    int choice = MenuSystem.MenuInput(
                        new[] { "Titeln kan inte vara tom. Försök igen." },
                        new[] { "Försök igen", "Meny" },
                        ConsoleColor.Red
                    );
                    if (choice == 0) continue;

                    return false;
                }

                if (title.Length > 50)
                {
                    int choice = MenuSystem.MenuInput(
                        new[] { "Titeln får inte vara längre än 50 tecken. Försök igen." },
                        new[] { "Försök igen", "Meny" },
                        ConsoleColor.Red
                    ); 
                    if (choice == 0) continue;

                    return false;
                }

                var account = new BankAccount
                {
                    UserId = userId,
                    Balance = 0,
                    Title = title
                };

                _context.BankAccounts.Add(account);
                _context.SaveChanges();

                MenuSystem.MenuInput(
                    new[] { $"✅ Ditt konto '{title}' har skapats med ID: {account.Id}." },
                    new[] { "Meny" },
                    null
                );

                return true;
            }
        }

        public void Deposit(int userId)
        {
            while (true)
            {
                var userAccounts = _context.BankAccounts
                    .Where(b => b.UserId == userId)
                    .ToList();

                string[] accountStrings = userAccounts
                    .Select(a => $"Titel: [{a.Title}] Kontosaldo: [{a.Balance} kr]")
                    .Concat(new[] { "Avbryt" })
                    .ToArray();

                int choice = MenuSystem.MenuInput(
                    new[] { "INSÄTTNING", "Välj ett konto för insättning" },
                    accountStrings,
                    null
                );

                if (choice == accountStrings.Length - 1) 
                {
                    MenuSystem.MenuInput(
                        new[] { "Insättning avbruten." },
                        new[] { "Meny" },
                        null
                    );
                    return;
                }
                var chosenAccount = userAccounts[choice];

                choice = MenuSystem.MenuInput(
                        new[] { "Välj insättningsbelopp" },
                        new[] { "100 kr", "200 kr", "500 kr", "1 000 kr", "2 000 kr",
                                "5 000 kr", "10 000 kr", "Ange manuellt", "Avbryt" },
                        null
                );

                if (choice == 8) 
                {
                    choice = MenuSystem.MenuInput(
                        new[] { "Insättning avbröts." },
                        new[] { "Försök igen", "Meny" },
                        null
                    );

                    if (choice == 0) continue;
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
                        MenuSystem.CenterY(2);
                        MenuSystem.WriteCenteredXForeground("Tryck på [ESC] för att avbryta.", ConsoleColor.Green, false);
                        MenuSystem.WriteCenteredXForeground("Ange insättningsbelopp: ", ConsoleColor.Yellow, true);
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

                choice = MenuSystem.MenuInput(
                        new[] { $"Du skrev in {amount} kr", $"Vill du bekräfta insättningen till '{chosenAccount.Title}'?" },
                        new[] { "Ja", "Nej" },
                        null
                    );

                if (choice == 0)
                {
                    chosenAccount.Balance += amount;

                    var transaction = new Transaction
                    {
                        Amount = amount,
                        Date = DateTime.UtcNow,
                        Type = Transaction.TransactionType.Deposit,
                        BankAccountTitle = chosenAccount.Title,
                        BankAccountId = chosenAccount.Id
                    };

                    chosenAccount.Transactions.Add(transaction);
                    _context.SaveChanges();

                    MenuSystem.MenuInput(
                        new[] { $"✅ Insättningen lyckades! {amount:C} har lagts till på kontot '{chosenAccount.Title}'.",
                                $"- Nytt saldo: {chosenAccount.Balance:C}" },
                        new[] { "Meny" },
                        null
                    ); 
                    return;
                }
                else
                {
                    choice = MenuSystem.MenuInput(
                        new[] { "❌ Insättningen avbröts." },
                        new[] { "Försök igen", "Meny" },
                        null
                    );
                    if (choice == 0) continue;
                    return;
                }
            }
        }

        public void Withdraw(int userId)
        {
            while (true)
            {
                var userAccounts = _context.BankAccounts
                    .Where(b => b.UserId == userId)
                    .ToList();

                string[] accountStrings = userAccounts
                    .Select(a => $"Titel: [{a.Title}] Kontosaldo: [{a.Balance} kr]")
                    .Concat(new[] { "Avbryt" })
                    .ToArray();

                int choice = MenuSystem.MenuInput(
                    new[] { "UTTAG", "Välj ett konto för uttag" },
                    accountStrings,
                    null
                );

                if (choice == accountStrings.Length - 1)
                {
                    MenuSystem.MenuInput(
                        new[] { "Uttag avbrutet." },
                        new[] { "Meny" },
                        null
                    );
                    return;
                }
                var chosenAccount = userAccounts[choice];

                choice = MenuSystem.MenuInput(
                        new[] { "Välj uttagsbelopp", $"Kontosaldo: {chosenAccount.Balance:C}" },
                        new[] { "100 kr", "200 kr", "500 kr", "1 000 kr", "2 000 kr",
                                "5 000 kr", "10 000 kr", "Ange manuellt", "Avbryt" },
                        null
                );

                if (choice == 8)
                {
                    choice = MenuSystem.MenuInput(
                        new[] { "Uttag avbröts." },
                        new[] { "Försök igen", "Meny" },
                        null
                    );

                    if (choice == 0) continue;
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
                        MenuSystem.CenterY(2);
                        MenuSystem.WriteCenteredXForeground("Tryck på [ESC] för att avbryta.", ConsoleColor.Green, false);
                        MenuSystem.WriteCenteredXForeground("Ange uttagsbelopp: ", ConsoleColor.Yellow, true);
                        string? input = MenuSystem.ReadNumberWithEscape();

                        if (input == null)
                        {
                            MenuSystem.MenuInput(
                                new[] { "Uttag avbröts." },
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

                if (chosenAccount.Balance < amount)
                {
                    choice = MenuSystem.MenuInput(
                        new[] { "Otillräckligt saldo. Försök igen." },
                        new[] { "Försök igen", "Meny" },
                        ConsoleColor.Red
                    );
                    if (choice == 0) continue;
                    return;
                }

                choice = MenuSystem.MenuInput(
                        new[] { $"Du skrev in {amount} kr", $"Vill du bekräfta uttaget från '{chosenAccount.Title}'?" },
                        new[] { "Ja", "Nej" },
                        null
                    );

                if (choice == 0)
                {
                    chosenAccount.Balance -= amount;

                    var transaction = new Transaction
                    {
                        Amount = amount,
                        Date = DateTime.UtcNow,
                        Type = Transaction.TransactionType.Withdrawal,
                        BankAccountTitle = chosenAccount.Title,
                        BankAccountId = chosenAccount.Id
                    };

                    chosenAccount.Transactions.Add(transaction);
                    _context.SaveChanges();

                    MenuSystem.MenuInput(
                        new[] { $"✅ Uttaget lyckades! {amount:C} har dragits från kontot '{chosenAccount.Title}'.",
                                $"- Nytt saldo: {chosenAccount.Balance:C}" },
                        new[] { "Meny" },
                        null
                    );
                    return;
                }
                else
                {
                    choice = MenuSystem.MenuInput(
                        new[] { "❌ Uttaget avbröts." },
                        new[] { "Försök igen", "Meny" },
                        null
                    );
                    if (choice == 0) continue;
                    return;
                }
            }
        }

        public void Transfer(int userId)
        {
            Console.Clear();

            var bankAccounts = _context.BankAccounts
                .Where(b => b.UserId == userId)
                .ToList();

            if (bankAccounts.Count == 1)
            {
                Console.WriteLine("Du har bara ett konto. Du kan inte överföra mellan konton.");
                Console.Write("\nTryck på valfri tangent för att återgå till menyn... ");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\nDina konton:");
            foreach (var account in bankAccounts)
                Console.WriteLine($"- {account.Title} (ID: {account.Id})");

            BankAccount? fromAccount = null;
            BankAccount? toAccount = null;

            while (fromAccount == null)
            {
                Console.Write("\nAnge konto att överföra ifrån: ");
                var input = Console.ReadLine();

                fromAccount = bankAccounts.SingleOrDefault(a => a.Title.Equals(input, StringComparison.OrdinalIgnoreCase));
                if (fromAccount == null)
                    Console.WriteLine("Kunde inte hitta kontot. Försök igen.");
            }

            while (toAccount == null)
            {
                Console.Write("Ange konto att överföra till: ");
                var input = Console.ReadLine();

                toAccount = bankAccounts.SingleOrDefault(a => a.Title.Equals(input, StringComparison.OrdinalIgnoreCase));
                if (toAccount == null)
                    Console.WriteLine("Kunde inte hitta kontot. Försök igen.");
                else if (toAccount.Id == fromAccount.Id)
                {
                    Console.WriteLine("Du kan inte överföra till samma konto.");
                    toAccount = null;
                }
            }

            decimal amount = 0;
            while (true)
            {
                Console.Write("Ange summa att överföra: ");
                if (!decimal.TryParse(Console.ReadLine(), out amount) || amount <= 0)
                {
                    Console.WriteLine("Ogiltigt belopp. Försök igen.");
                    continue;
                }

                if (fromAccount.Balance < amount)
                {
                    Console.WriteLine("Otillräckligt saldo. Försök igen.");
                    continue;
                }

                break;
            }

            Console.WriteLine($"\nBekräfta överföring av {amount:C} från '{fromAccount.Title}' till '{toAccount.Title}'.");
            Console.Write("Vill du fortsätta? (true/false): ");

            if (bool.TryParse(Console.ReadLine(), out bool confirm) && confirm)
            {
                fromAccount.Balance -= amount;
                toAccount.Balance += amount;

                var transferFrom = new Transaction
                {
                    Amount = amount,
                    Date = DateTime.Now,
                    Sender = true,
                    BankAccountTitle = fromAccount.Title,
                    OtherBankAccountTitle = toAccount.Title,
                    BankAccountId = fromAccount.Id
                };
                fromAccount.Transactions.Add(transferFrom);

                var transferTo = new Transaction
                {
                    Amount = amount,
                    Date = DateTime.Now,
                    Sender = false,
                    BankAccountTitle = fromAccount.Title,
                    OtherBankAccountTitle = toAccount.Title,
                    BankAccountId = toAccount.Id
                };
                toAccount.Transactions.Add(transferTo);

                _context.SaveChanges();

                Console.WriteLine($"\n✅ Överföringen lyckades! {amount:C} har överförts.");
                Console.WriteLine($"- Nytt saldo på '{fromAccount.Title}': {fromAccount.Balance:C}");
                Console.WriteLine($"- Nytt saldo på '{toAccount.Title}': {toAccount.Balance:C}");
            }
            else
            {
                Console.WriteLine("❌ Överföringen avbröts.");
            }

            Console.WriteLine("\nTryck på valfri tangent för att återgå till menyn...");
            Console.ReadKey();
        }

        public void ViewTransactionHistory(int userId)
        {
            var transactions = _context.Users
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Transactions)
                .SingleOrDefault(u => u.Id == userId)?
                .Accounts
                .SelectMany(a => a.Transactions)
                .ToList();

            if (transactions == null || !transactions.Any())
            {
                Console.Clear();
                Console.WriteLine("Inga transaktioner hittades.");
            }
            else
            {
                Console.Clear();
                Console.WriteLine($"Totalt antal transaktioner: {transactions.Count}");
                Console.WriteLine("Transaktionshistorik:\n");
                transactions = transactions.OrderByDescending(t => t.Date).ToList();

                foreach (var transaction in transactions)
                {
                    if (transaction.OtherBankAccountTitle != null && transaction.Type == Transaction.TransactionType.Transfer && transaction.Sender == true)
                    {
                        Console.WriteLine($"- {transaction.Date}: {transaction.Amount:C} ({transaction.Type}) från '{transaction.BankAccountTitle}' med id: '{transaction.BankAccountId}' till '{transaction.OtherBankAccountTitle}'");
                    }
                    else if(transaction.OtherBankAccountTitle != null && transaction.Type == Transaction.TransactionType.Transfer && transaction.Sender == false)
                    {
                        Console.WriteLine($"- {transaction.Date}: {transaction.Amount:C} ({transaction.Type}) till '{transaction.BankAccountTitle}' med id: '{transaction.BankAccountId}' från '{transaction.OtherBankAccountTitle}'");
                    }
                    else if (transaction.Type == Transaction.TransactionType.Deposit)
                    {
                        Console.WriteLine($"- {transaction.Date}: {transaction.Amount:C} ({transaction.Type}) på '{transaction.BankAccountTitle}'");
                    }
                    else
                    {
                        Console.WriteLine($"- {transaction.Date}: {transaction.Amount:C} ({transaction.Type}) på '{transaction.BankAccountTitle}'");
                    }
                }
            }

            Console.WriteLine("\nTryck på valfri tangent för att återgå till menyn...");
            Console.ReadKey();
        }

        public void TransferToExternalAccount(int userId)
        {
            var senderAccounts = _context.BankAccounts
                .Where(b => b.UserId == userId)
                .ToList();

            Console.WriteLine("\nDina konton:");
            foreach (var account in senderAccounts)
                Console.WriteLine($"- {account.Title} (ID: {account.Id})");

            BankAccount? fromAccount = null;
            while (fromAccount == null)
            {
                Console.Write("\nAnge konto att överföra ifrån (titel eller ID): ");
                var input = Console.ReadLine();

                fromAccount = senderAccounts
                    .FirstOrDefault(a =>
                        a.Title.Equals(input, StringComparison.OrdinalIgnoreCase) ||
                        a.Id.ToString() == input);

                if (fromAccount == null)
                    Console.WriteLine("Kunde inte hitta ditt konto. Försök igen.");
            }

            BankAccount? toAccount = null;
            while (toAccount == null)
            {
                Console.Write("Ange ID för mottagarens konto: ");
                var input = Console.ReadLine();

                if (!int.TryParse(input, out int toAccountId))
                {
                    Console.WriteLine("Ogiltigt ID. Försök igen.");
                    continue;
                }

                toAccount = _context.BankAccounts
                    .Include(b => b.Transactions)
                    .FirstOrDefault(a => a.Id == toAccountId);

                if (toAccount == null)
                {
                    Console.WriteLine("Kunde inte hitta mottagarkontot. Försök igen.");
                }
                else if (toAccount.UserId == userId)
                {
                    Console.WriteLine("Du kan inte överföra till ditt eget konto. Försök igen.");
                    toAccount = null;
                }
            }

            decimal amount = 0;
            while (true)
            {
                Console.Write("Ange summa att överföra: ");
                if (!decimal.TryParse(Console.ReadLine(), out amount) || amount <= 0)
                {
                    Console.WriteLine("Ogiltigt belopp. Försök igen.");
                    continue;
                }

                if (fromAccount.Balance < amount)
                {
                    Console.WriteLine("Otillräckligt saldo. Försök igen.");
                    continue;
                }

                break;
            }

            Console.WriteLine($"\nBekräfta överföring av {amount:C} från '{fromAccount.Title}' till konto ID {toAccount.Id} ('{toAccount.Title}').");
            Console.Write("Vill du fortsätta? (true/false): ");

            if (bool.TryParse(Console.ReadLine(), out bool confirm) && confirm)
            {
                fromAccount.Balance -= amount;
                toAccount.Balance += amount;

                var transferFrom = new Transaction
                {
                    Amount = amount,
                    Date = DateTime.Now,
                    Sender = true,
                    BankAccountTitle = fromAccount.Title,
                    OtherBankAccountTitle = toAccount.Title,
                    BankAccountId = fromAccount.Id
                };
                fromAccount.Transactions.Add(transferFrom);

                var transferTo = new Transaction
                {
                    Amount = amount,
                    Date = DateTime.Now,
                    Sender = false,
                    BankAccountTitle = fromAccount.Title,
                    OtherBankAccountTitle = toAccount.Title,
                    BankAccountId = toAccount.Id
                };
                toAccount.Transactions.Add(transferTo);

                _context.SaveChanges();

                Console.WriteLine($"\n✅ Överföringen lyckades! {amount:C} har överförts.");
                Console.WriteLine($"- Nytt saldo på '{fromAccount.Title}': {fromAccount.Balance:C}");
            }
            else
            {
                Console.WriteLine("❌ Överföringen avbröts.");
            }

            Console.WriteLine("\nTryck på valfri tangent för att återgå till menyn...");
            Console.ReadKey();
        }
    }
}