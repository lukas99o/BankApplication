using BankApplication.Data;
using BankApplication.Helpers;
using BankApplication.Models;
using BankApplication.Services.IServices;

namespace BankApplication.Services
{
    internal class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;

        public AccountService(ApplicationDbContext context)
        {
            _context = context;            
        }

        public (User? user, DateTime? lockoutUntil, int tries, int failedAttempts) Login(DateTime? lockoutUntil, int tries, int failedAttempts)
        {
            if (lockoutUntil.HasValue && lockoutUntil.Value > DateTime.Now)
            {
                MenuSystem.MenuInput(
                    new[] { $"Du är låst till {lockoutUntil.Value:T}. Försök igen senare." },
                    new[] { "Meny" },
                    ConsoleColor.Red
                );
                return (null, lockoutUntil, tries, failedAttempts);
            }

            while (true)
            {
                Console.Clear();
                MenuSystem.CenterY(2);    

                MenuSystem.WriteAllCenteredXForeground(
                    new[] { "Logga in.", "Tryck på [ESC] för att avbryta inloggningen.", "" },
                    ConsoleColor.Green
                );
                MenuSystem.WriteCenteredXForeground("Ange ditt användarnamn: ", ConsoleColor.Yellow, true);

                string? username = MenuSystem.ReadLineWithEscape();

                if (username == null)
                {
                    int choice = MenuSystem.MenuInput(new[] { "Inloggning avbröts." }, new[] { "Meny" }, null);
                    return (null, null, tries, failedAttempts);
                }

                if (string.IsNullOrWhiteSpace(username))
                {
                    int choice = MenuSystem.MenuInput(new[] { "Användarnamnet kan inte vara tomt." }, new[] { "Försök igen", "Meny" }, ConsoleColor.Red);
                    if (choice == 0) continue;

                    return (null, null, tries, failedAttempts);
                }

                var user = _context.Users.SingleOrDefault(u => u.Username == username);

                if (user != null)
                {
                    tries = 3;
                    return (user, null, tries, failedAttempts);
                }
                else
                {
                    tries--;

                    if (tries == 0)
                    {
                        failedAttempts++;
                        if (failedAttempts > 3) failedAttempts = 3;

                        switch (failedAttempts)
                        {
                            case 1:
                                lockoutUntil = DateTime.Now.AddMinutes(1);
                                break;

                            case 2:
                                lockoutUntil = DateTime.Now.AddMinutes(5);
                                break;

                            case 3:
                                lockoutUntil = DateTime.Now.AddMinutes(15);
                                break;
                        }

                        MenuSystem.MenuInput(
                            new[] { $"Du skrev fel användarnamn tre gånger. Du är nu låst till {lockoutUntil:T}." },
                            new[] { "Meny" },
                            ConsoleColor.Red
                        );

                        tries = 3;
                        return (null, lockoutUntil, tries, failedAttempts);
                    }
                    else
                    {
                        int choice = MenuSystem.MenuInput(
                            new[] { $"Användarnamnet finns inte. Försök igen. Försök kvar: {tries}." },
                            new[] { "Försök igen", "Meny" },
                            ConsoleColor.Red
                        );
                        if (choice == 0) continue;

                        return (null, null, tries, failedAttempts);
                    }
                }
            }
        }

        public void Register()
        {
            while (true)
            {
                Console.Clear();
                MenuSystem.CenterY(2);

                MenuSystem.WriteAllCenteredXForeground(
                    new[] { "Registrera dig.", "Tryck på [ESC] för att avbryta registreringen.", "" },
                    ConsoleColor.Green
                );

                MenuSystem.WriteCenteredXForeground("Ange ett användarnamn: ", ConsoleColor.Yellow, true);
                string? username = MenuSystem.ReadLineWithEscape();

                MenuSystem.WriteCenteredXForeground("Ange ett lösenord: ", ConsoleColor.Yellow, true);
                string? password = MenuSystem.ReadLineWithEscape();

                if (string.IsNullOrEmpty(username))
                {
                    int choice = MenuSystem.MenuInput(new[] { "Användarnamnet kan inte vara tomt." }, new[] { "Försök igen" ,"Meny" }, ConsoleColor.Red);
                    if (choice == 0) continue;
                    return;
                }
                else 
                {
                    if (username.Length < 3 || username.Length > 20)
                    {
                        int choice = MenuSystem.MenuInput(new[] { "Användarnamnet måste vara mellan 3 och 20 tecken långt." }, new[] { "Försök igen", "Meny" }, ConsoleColor.Red);
                        if (choice == 0) continue;
                        return;
                    }
                    else if (_context.Users.Any(u => u.Username == username))
                    {
                        int choice = MenuSystem.MenuInput(new[] { "Användarnamnet finns redan. Välj ett annat." }, new[] { "Försök igen", "Meny" }, ConsoleColor.Red);
                        if (choice == 0) continue;
                        return;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(password))
                        {
                            int choice = MenuSystem.MenuInput(new[] { "Lösenordet kan inte vara tomt." }, new[] { "Försök igen", "Meny" }, ConsoleColor.Red);
                            if (choice == 0) continue;
                            return;
                        }
                        else if (password.Length < 6 || password.Length > 20)
                        {
                            int choice = MenuSystem.MenuInput(new[] { "Lösenordet måste vara mellan 6 och 20 tecken långt." }, new[] { "Försök igen", "Meny" }, ConsoleColor.Red);
                            if (choice == 0) continue;
                            return;
                        }
                        else
                        {
                            User newUser = new User { Username = username };
                            _context.Users.Add(newUser);
                            _context.SaveChanges();
                            Console.WriteLine("\nRegistrering lyckades!");
                            Console.WriteLine("Tryck på valfri tangent för att återgå till menyn...");
                            Console.ReadKey();
                            return;
                        }
                    }
                }
            }
        }
    }
}
