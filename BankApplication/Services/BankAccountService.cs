using BankApplication.Data;
using BankApplication.Helpers;
using BankApplication.Models;
using BankApplication.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
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

        public void ViewAccounts(int userId)
        {
            var bankAccounts = _context.BankAccounts
                .Where(b => b.UserId == userId)
                .ToList();

            var accountStrings = new[] { "Konton:" }
                .Concat(bankAccounts.Select(a => $"Titel: [{a.Title}] Kontosaldo: [{a.Balance} kr] ID: [{a.Id}]"))
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
                MenuSystem.CenterY(9);
                MenuSystem.Header();
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

        public void RemoveAccount(int userId)
        {
            while (true)
            {
                var userAccounts = _context.BankAccounts
                .Where(b => b.UserId == userId)
                .ToList();

                if (userAccounts.Count == 1)
                {
                    MenuSystem.MenuInput(
                        new[] { "Du har 1 konto du kan inte ta bort alla konton." },
                        new[] { "Meny" },
                        null
                    );
                    return;
                }

                string[] accountStrings = userAccounts
                    .Select(a => $"Titel: [{a.Title}] Kontosaldo: [{a.Balance} kr]")
                    .Concat(new[] { "Avbryt" })
                    .ToArray();

                int choice = MenuSystem.MenuInput(
                    new[] { "TA BORT KONTO", "Välj ett konto att ta bort" },
                    accountStrings,
                    null
                );

                if (choice == accountStrings.Length - 1)
                {
                    MenuSystem.MenuInput(
                        new[] { "Konto borttagning avbruten." },
                        new[] { "Meny" },
                        null
                    );
                    return;
                }

                var chosenAccount = userAccounts[choice];
                if (chosenAccount.Balance > 0)
                {
                    choice = MenuSystem.MenuInput(
                        new[] { $"Konto '{chosenAccount.Title}' har ett saldo på {chosenAccount.Balance:C}.",
                            "Du kan inte ta bort ett konto med saldo.",
                            "Försök igen eller ta ut pengarna först." },
                        new[] { "Försök igen", "Överför till konto", "Ta ut pengar", "Meny" },
                        ConsoleColor.Green
                    );

                    if (choice == 0)
                    {
                        continue;
                    } 
                    else if (choice == 1)
                    {
                        Transfer(userId);
                        continue;
                    }
                    else if (choice == 2)
                    {
                        Withdraw(userId);
                        continue;
                    }
                    else
                    {
                        return;
                    }  
                }

                _context.BankAccounts.Remove(chosenAccount);
                _context.SaveChanges();
                MenuSystem.MenuInput(
                    new[] { $"✅ Konto '{chosenAccount.Title}' har tagits bort." },
                    new[] { "Meny" },
                    null
                );
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
                        MenuSystem.CenterY(8);
                        MenuSystem.Header();
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
                        new[] { "Uttag avbruten." },
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
                        MenuSystem.CenterY(8);
                        MenuSystem.Header();
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
            while (true)
            {
                int transferTypeChoice = MenuSystem.MenuInput(
                    new[] { "ÖVERFÖRING", "Välj typ av överföring" },
                    new[] { "Mellan egna konton", "Till någon annans konto", "Avbryt" },
                    null
                );

                if (transferTypeChoice == 2)
                {
                    MenuSystem.MenuInput(
                        new[] { "Överföring avbruten." },
                        new[] { "Meny" },
                        null
                    );
                    return;
                }

                BankAccount? fromAccount = null;
                BankAccount? toAccount = null;

                if (transferTypeChoice == 0) 
                {
                    var userAccounts = _context.BankAccounts
                        .Where(b => b.UserId == userId)
                        .ToList();

                    if (userAccounts.Count < 2)
                    {
                        int choice = MenuSystem.MenuInput(
                            new[] { "Du har inte tillräckligt många konton för att göra en överföring." },
                            new[] { "Skapa konto", "Meny" },
                            null
                        );

                        if (choice == 0)
                        {
                            CreateAccount(userId);

                            userAccounts = _context.BankAccounts
                                .Where(b => b.UserId == userId)
                                .ToList();
                        }
                        else
                        {
                            return;
                        }
                    }

                    string[] accountStrings = userAccounts
                        .Select(a => $"Titel: [{a.Title}] Kontosaldo: [{a.Balance} kr]")
                        .Concat(new[] { "Avbryt" })
                        .ToArray();

                    int fromChoice = MenuSystem.MenuInput(
                        new[] { "ÖVERFÖRING", "Välj konto att överföra från" },
                        accountStrings,
                        null
                    );

                    if (fromChoice == accountStrings.Length - 1)
                    {
                        MenuSystem.MenuInput(
                            new[] { "Överföring avbruten." },
                            new[] { "Meny" },
                            null
                        );
                        return;
                    }
                    fromAccount = userAccounts[fromChoice];

                    int toChoice = MenuSystem.MenuInput(
                        new[] { "Välj konto att överföra till" },
                        accountStrings.Where((_, index) => index != fromChoice).ToArray(),
                        null
                    );

                    if (toChoice == accountStrings.Length - 2)
                    {
                        MenuSystem.MenuInput(
                            new[] { "Överföring avbruten." },
                            new[] { "Meny" },
                            null
                        );
                        return;
                    }
                    toAccount = userAccounts.Where((_, index) => index != fromChoice).ToList()[toChoice];
                }
                else if (transferTypeChoice == 1) 
                {
                    var senderAccounts = _context.BankAccounts
                        .Where(b => b.UserId == userId)
                        .ToList();

                    string[] senderAccountStrings = senderAccounts
                        .Select(a => $"Titel: [{a.Title}] Kontosaldo: [{a.Balance} kr]")
                        .Concat(new[] { "Avbryt" })
                        .ToArray();

                    int fromChoice = MenuSystem.MenuInput(
                        new[] { "ÖVERFÖRING", "Välj konto att överföra från" },
                        senderAccountStrings,
                        null
                    );

                    if (fromChoice == senderAccountStrings.Length - 1)
                    {
                        MenuSystem.MenuInput(
                            new[] { "Överföring avbruten." },
                            new[] { "Meny" },
                            null
                        );
                        return;
                    }
                    fromAccount = senderAccounts[fromChoice];

                    while (toAccount == null)
                    {
                        MenuSystem.CenterY(8);
                        MenuSystem.Header();
                        MenuSystem.WriteCenteredXForeground("Tryck på [ESC] för att avbryta.", ConsoleColor.Green, false);
                        MenuSystem.WriteCenteredXForeground("Ange ID för mottagarens konto: ", ConsoleColor.Yellow, true);
                        string? input = MenuSystem.ReadNumberWithEscape();

                        if (input == null)
                        {
                            MenuSystem.MenuInput(
                                new[] { "Överföring avbröts." },
                                new[] { "Meny" },
                                null
                            );
                            return;
                        }

                        if (input == "")
                        {
                            int choice = MenuSystem.MenuInput(
                                new[] { "ID kan inte vara tomt. Försök igen." },
                                new[] { "Försök igen", "Meny" },
                                ConsoleColor.Red
                            );
                            if (choice == 0) continue;
                            return;
                        }

                        if (!int.TryParse(input, out int toAccountId))
                        {
                            int choice = MenuSystem.MenuInput(
                                new[] { "Ogiltigt ID. Försök igen." },
                                new[] { "Försök igen", "Meny" },
                                ConsoleColor.Red
                            ); 
                            if (choice == 0) continue;
                            return;
                        }

                        toAccount = _context.BankAccounts
                            .Include(b => b.Transactions)
                            .Include(b => b.User)
                            .FirstOrDefault(a => a.Id == toAccountId);

                        if (toAccount == null)
                        {
                            int choice = MenuSystem.MenuInput(
                                new[] { "Kunde inte hitta mottagarkontot. Försök igen." },
                                new[] { "Försök igen", "Meny" },
                                ConsoleColor.Red
                            );
                            if (choice == 0) continue;
                            return;
                        }
                        else if (toAccount.UserId == userId)
                        {
                            int choice = MenuSystem.MenuInput(
                                new[] { "Du kan inte överföra till ditt eget konto. Försök igen." },
                                new[] { "Försök igen", "Meny" },
                                ConsoleColor.Red
                            );
                            toAccount = null;
                            if (choice == 0) continue;
                            return;
                        }
                    }
                }

                decimal amount = 0;
                while (true)
                {
                    Console.Clear();
                    MenuSystem.CenterY(11);
                    MenuSystem.Header();
                    MenuSystem.WriteCenteredXForeground($"Från: Title: [{fromAccount!.Title}] Kontosaldo [{fromAccount.Balance}]", ConsoleColor.Green, false);
                    MenuSystem.WriteCenteredXForeground($"Till: Title: [{toAccount!.Title}] ID: [{toAccount.Id}]      ", ConsoleColor.Green, false);
                    Console.WriteLine();
                    MenuSystem.WriteCenteredXForeground("Tryck på [ESC] för att avbryta.", ConsoleColor.Green, false);
                    MenuSystem.WriteCenteredXForeground("Ange överföringsbelopp: ", ConsoleColor.Yellow, true);
                    string? input = MenuSystem.ReadNumberWithEscape();

                    if (input == null)
                    {
                        MenuSystem.MenuInput(
                            new[] { "Överföring avbruten." },
                            new[] { "Meny" },
                            null
                        );
                        return;
                    }

                    if (input == "")
                    {
                        int choice = MenuSystem.MenuInput(
                            new[] { "Beloppet kan inte vara tomt. Försök igen." },
                            new[] { "Försök igen", "Meny" },
                            ConsoleColor.Red
                        );

                        if (choice == 0) continue;
                        return;
                    }

                    if (!decimal.TryParse(input, out amount) || amount <= 0)
                    {
                        int choice = MenuSystem.MenuInput(
                            new[] { "Ogiltigt belopp. Försök igen." },
                            new[] { "Försök igen", "Meny" },
                            ConsoleColor.Red
                        );
                        if (choice == 0) continue;
                        return;
                    }

                    if (fromAccount!.Balance < amount)
                    {
                        int choice = MenuSystem.MenuInput(
                            new[] { "Otillräckligt saldo. Försök igen." },
                            new[] { "Försök igen", "Meny" },
                            ConsoleColor.Red
                        );
                        if (choice == 0) continue;
                        return;
                    }

                    break;
                }

                int confirmChoice = MenuSystem.MenuInput(
                    new[] { $"Du skrev in {amount:C}", $"Vill du bekräfta överföringen från '{fromAccount.Title}' till '{toAccount!.Title}' ID: [{toAccount.Id}]?" },
                    new[] { "Ja", "Nej" },
                    null
                );

                if (confirmChoice == 0)
                {
                    fromAccount.Balance -= amount;
                    toAccount.Balance += amount;

                    var transferFrom = new Transaction
                    {
                        Amount = amount,
                        Date = DateTime.UtcNow,
                        Sender = true,
                        BankAccountTitle = fromAccount.Title,
                        OtherBankAccountTitle = toAccount.Title,
                        BankAccountId = fromAccount.Id,
                        Type = Transaction.TransactionType.Transfer
                    };
                    fromAccount.Transactions.Add(transferFrom);
                    _context.SaveChanges();

                    if (toAccount.User!.Id == userId)
                    {
                        MenuSystem.MenuInput(
                            new[] { $"✅ Överföringen lyckades! {amount:C} har överförts från '{fromAccount.Title}' till '{toAccount.Title}'.",
                                    $"- Nytt saldo på '{fromAccount.Title}': {fromAccount.Balance:C}",
                                    $"- Nytt saldo på '{toAccount.Title}': {toAccount.Balance:C}" },
                            new[] { "Meny" },
                            null
                        );
                    }
                    else
                    {
                        MenuSystem.MenuInput(
                            new[] { $"✅ Överföringen lyckades! {amount:C} har överförts från '{fromAccount.Title}' till '{toAccount.Title}' ID: [{toAccount.Id}].",
                                    $"- Nytt saldo på '{fromAccount.Title}': {fromAccount.Balance:C}",
                                    $"- Nytt saldo på '{toAccount.Title}' ID: [{toAccount.Id}]" },
                            new[] { "Meny" },
                            null
                        );
                    }
                    return;
                }
                else
                {
                    int choice = MenuSystem.MenuInput(
                        new[] { "❌ Överföringen avbröts." },
                        new[] { "Försök igen", "Meny" },
                        null
                    );
                    if (choice == 0) continue;
                    return;
                }
            }
        }

        public void ViewTransactionHistory(int userId)
        {
            var transactions = _context.Users
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Transactions)
                .SingleOrDefault(u => u.Id == userId)?
                .Accounts
                .SelectMany(a => a.Transactions)
                .OrderByDescending(t => t.Date)
                .ToList();

            if (transactions == null || !transactions.Any())
            {
                MenuSystem.MenuInput(
                    new[] { "Inga transaktioner hittades." },
                    new[] { "Meny" },
                    null
                );
                return;
            }

            var transactionData = new List<string>();
            foreach (var transaction in transactions)
            {
                if (transaction.OtherBankAccountTitle != null && transaction.Type == Transaction.TransactionType.Transfer && transaction.Sender == true)
                {
                    transactionData.Add(
                        $"- {transaction.Date}: {transaction.Amount:C} ({transaction.Type}) från '{transaction.BankAccountTitle}' (id: {transaction.BankAccountId}) till '{transaction.OtherBankAccountTitle}'"
                    );
                }
                else if (transaction.OtherBankAccountTitle != null && transaction.Type == Transaction.TransactionType.Transfer && transaction.Sender == false)
                {
                    transactionData.Add(
                        $"- {transaction.Date}: {transaction.Amount:C} ({transaction.Type}) till '{transaction.BankAccountTitle}' (id: {transaction.BankAccountId}) från '{transaction.OtherBankAccountTitle}'"
                    );
                }
                else if (transaction.Type == Transaction.TransactionType.Deposit)
                {
                    transactionData.Add(
                        $"- {transaction.Date}: {transaction.Amount:C} ({transaction.Type}) på '{transaction.BankAccountTitle}'"
                    );
                }
                else
                {
                    transactionData.Add(
                        $"- {transaction.Date}: {transaction.Amount:C} ({transaction.Type}) på '{transaction.BankAccountTitle}'"
                    );
                }
            }

            int choice = MenuSystem.MenuInput(
                new[] { "KONTOUTDRAG", "Dina transaktioner:", "" }
                    .Concat(transactionData)
                    .ToArray(),
                new[] { "Meny" },
                null
            );
        }

    }
}