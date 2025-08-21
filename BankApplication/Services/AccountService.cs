using BankApplication.Data;
using BankApplication.Helpers;
using BankApplication.Models;
using BankApplication.Services.IServices;
using BankApplication.Utilities;

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
                MenuSystem.CenterY(9);
                MenuSystem.Header();

                MenuSystem.WriteAllCenteredXForeground(
                    new[] { "LOGGA IN", "Tryck på [ESC] för att avbryta.", "" },
                    ConsoleColor.Green
                );

                MenuSystem.WriteCenteredXForeground("Ange ditt användarnamn: ", ConsoleColor.Yellow, true);
                string? username = MenuSystem.ReadLineWithEscape();

                if (username == null)
                {
                    MenuSystem.MenuInput(new[] { "Inloggning avbröts." }, new[] { "Meny" }, null);
                    return (null, null, tries, failedAttempts);
                }

                if (string.IsNullOrWhiteSpace(username))
                {
                    int choice = MenuSystem.MenuInput(new[] { "Användarnamnet kan inte vara tomt." }, new[] { "Försök igen", "Meny" }, ConsoleColor.Red);
                    if (choice == 0) continue;

                    return (null, null, tries, failedAttempts);
                }

                var user = _context.Users.SingleOrDefault(u => u.Username == username);

                if (user == null)
                {
                    tries--;
                    if (tries == 0)
                    {
                        failedAttempts++;
                        if (failedAttempts > 3) failedAttempts = 3;

                        lockoutUntil = failedAttempts switch
                        {
                            1 => DateTime.Now.AddMinutes(1),
                            2 => DateTime.Now.AddMinutes(5),
                            3 => DateTime.Now.AddMinutes(15),
                            _ => DateTime.Now.AddMinutes(15)
                        };

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

                MenuSystem.WriteCenteredXForeground("Ange ditt lösenord: ", ConsoleColor.Yellow, true);
                string? password = MenuSystem.ReadLineWithEscape();

                if (password == null)
                {
                    MenuSystem.MenuInput(new[] { "Inloggning avbröts." }, new[] { "Meny" }, null);
                    return (null, null, tries, failedAttempts);
                }

                if (!PasswordHash.VerifyPassword(password, user.Password))
                {
                    tries--;
                    if (tries == 0)
                    {
                        failedAttempts++;
                        if (failedAttempts > 3) failedAttempts = 3;

                        lockoutUntil = failedAttempts switch
                        {
                            1 => DateTime.Now.AddMinutes(1),
                            2 => DateTime.Now.AddMinutes(5),
                            3 => DateTime.Now.AddMinutes(15),
                            _ => DateTime.Now.AddMinutes(15)
                        };

                        MenuSystem.MenuInput(
                            new[] { $"Du skrev fel lösenord tre gånger. Du är nu låst till {lockoutUntil:T}." },
                            new[] { "Meny" },
                            ConsoleColor.Red
                        );

                        tries = 3;
                        return (null, lockoutUntil, tries, failedAttempts);
                    }
                    else
                    {
                        int choice = MenuSystem.MenuInput(
                            new[] { $"Fel lösenord. Försök kvar: {tries}." },
                            new[] { "Försök igen", "Meny" },
                            ConsoleColor.Red
                        );
                        if (choice == 0) continue;

                        return (null, null, tries, failedAttempts);
                    }
                }

                tries = 3;
                failedAttempts = 0;
                return (user, null, tries, failedAttempts);
            }
        }

        public void Register()
        {
            while (true)
            {
                Console.Clear();
                MenuSystem.CenterY(9);
                MenuSystem.Header();

                MenuSystem.WriteAllCenteredXForeground(
                    new[] { "REGISTRERA DIG", "Tryck på [ESC] för att avbryta.", "" },
                    ConsoleColor.Green
                );

                MenuSystem.WriteCenteredXForeground("Ange ett användarnamn: ", ConsoleColor.Yellow, true);
                string? username = MenuSystem.ReadLineWithEscape();
                if (username == null)
                {
                    MenuSystem.MenuInput(new[] { "Registrering avbröts." }, new[] { "Meny" }, null);
                    return;
                }

                if (string.IsNullOrWhiteSpace(username))
                {
                    if (MenuSystem.MenuInput(new[] { "Användarnamnet kan inte vara tomt." }, new[] { "Försök igen", "Meny" }, ConsoleColor.Red) == 0)
                        continue;
                    return;
                }

                if (username.Length < 3 || username.Length > 20)
                {
                    if (MenuSystem.MenuInput(new[] { "Användarnamnet måste vara mellan 3 och 20 tecken långt." }, new[] { "Försök igen", "Meny" }, ConsoleColor.Red) == 0)
                        continue;
                    return;
                }

                if (_context.Users.Any(u => u.Username == username))
                {
                    if (MenuSystem.MenuInput(new[] { "Användarnamnet finns redan. Välj ett annat." }, new[] { "Försök igen", "Meny" }, ConsoleColor.Red) == 0)
                        continue;
                    return;
                }

                MenuSystem.WriteCenteredXForeground("Ange ett lösenord: ", ConsoleColor.Yellow, true);
                string? password = MenuSystem.ReadLineWithEscape();
                if (password == null)
                {
                    MenuSystem.MenuInput(new[] { "Registrering avbröts." }, new[] { "Meny" }, null);
                    return;
                }

                if (string.IsNullOrWhiteSpace(password) || password.Length < 6 || password.Length > 20)
                {
                    if (MenuSystem.MenuInput(new[] { "Lösenordet måste vara mellan 6 och 20 tecken långt." }, new[] { "Försök igen", "Meny" }, ConsoleColor.Red) == 0)
                        continue;
                    return;
                }

                string hashedPassword = PasswordHash.HashPassword(password);
                var newUser = new User { Username = username, Password = hashedPassword };
                _context.Users.Add(newUser);
                _context.SaveChanges();

                MenuSystem.MenuInput(new[] { "Registreringen lyckades! Du kan nu logga in." }, new[] { "Meny" }, null);
                return;
            }
        }
    }
}