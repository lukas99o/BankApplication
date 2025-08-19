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
            var user = _context.Users
                .Include(u => u.Accounts)
                .SingleOrDefault(u => u.Id == userId);

            if (user?.Accounts == null || !user.Accounts.Any())
            {
                Console.WriteLine("👋 Välkommen till banken!");
                Console.WriteLine("Vänligen skapa ditt första konto för att börja.");
            }

            Console.Clear();
            string? title;

            while (true)
            {
                Console.Write("\nAnge det nya kontots titel: ");
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

            Console.WriteLine($"\n✅ Ditt konto '{title}' har skapats med ID: {account.Id}.");
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
                        break;

                    Console.WriteLine("Inget konto med den titeln hittades. Försök igen.");
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
                    break;

                Console.WriteLine("Beloppet måste vara ett positivt nummer. Försök igen.");
            }

            Console.WriteLine($"\nBekräfta insättning av {amount:C} till '{account.Title}'.");
            Console.Write("Vill du fortsätta? (true/false): ");

            if (bool.TryParse(Console.ReadLine(), out bool confirm) && confirm)
            {
                account.Balance += amount;

                var transaction = new Transaction
                {
                    Amount = amount,
                    Date = DateTime.Now,
                    Type = Transaction.TransactionType.Deposit,
                    BankAccountTitle = account.Title,
                    BankAccountId = account.Id
                };

                account.Transactions.Add(transaction);
                _context.SaveChanges();

                Console.WriteLine($"\n✅ Insättningen lyckades! {amount:C} har lagts till på kontot \"{account.Title}\".");
                Console.WriteLine($"- Nytt saldo: {account.Balance:C}");
            }
            else
            {
                Console.WriteLine("❌ Insättningen avbröts.");
            }

            Console.WriteLine("\nTryck på valfri tangent för att återgå till menyn...");
            Console.ReadKey();
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

        public void Withdraw(int userId)
        {
            Console.Clear();
            BankAccount? account = null;

            while (true)
            {
                Console.Write("Ange kontots titel för att ta ut pengar: ");
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

            decimal amount = 0;
            while (true)
            {
                Console.Write("Ange belopp att ta ut: ");
                if (!decimal.TryParse(Console.ReadLine(), out amount) || amount <= 0)
                {
                    Console.WriteLine("Ogiltigt belopp. Försök igen.");
                    continue;
                }

                if (account.Balance < amount)
                {
                    Console.WriteLine("Otillräckligt saldo. Försök igen.");
                    continue;
                }

                break;
            }

            Console.WriteLine($"\nBekräfta uttag av {amount:C} från '{account.Title}'.");
            Console.Write("Vill du fortsätta? (true/false): ");

            if (bool.TryParse(Console.ReadLine(), out bool confirm) && confirm)
            {
                account.Balance -= amount;

                var transaction = new Transaction
                {
                    Amount = amount,
                    Date = DateTime.Now,
                    Type = Transaction.TransactionType.Withdrawal,
                    BankAccountTitle = account.Title,
                    BankAccountId = account.Id
                };

                account.Transactions.Add(transaction);
                _context.SaveChanges();

                Console.WriteLine($"\n✅ Uttaget lyckades! {amount:C} har tagits ut.");
                Console.WriteLine($"- Nytt saldo på '{account.Title}': {account.Balance:C}");
            }
            else
            {
                Console.WriteLine("❌ Uttaget avbröts.");
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