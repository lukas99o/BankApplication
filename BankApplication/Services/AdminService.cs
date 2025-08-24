using BankApplication.Data;
using BankApplication.Helpers;
using BankApplication.Services.IServices;

namespace BankApplication.Services
{
    internal class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Login()
        {

        }

        public void DeleteUser()
        {
            while (true)
            {
                MenuSystem.CenterY(8);
                MenuSystem.Header();
                MenuSystem.WriteCenteredXForeground("RADERA ANVÄNDARE", ConsoleColor.Green, false);
                MenuSystem.WriteCenteredXForeground("Tryck på [ESC] för att avbryta.", ConsoleColor.Green, false);
                Console.WriteLine();
                MenuSystem.WriteCenteredXForeground("Ange användar-ID för att ta bort: ", ConsoleColor.Yellow, true);
                string? input = MenuSystem.ReadNumberWithEscape();

                if (input == null)
                {
                    MenuSystem.MenuInput(
                            new[] { "Radering av användare avbröts." },
                            new[] { "Meny" },
                            null
                        );
                    return;
                }

                if (input == "")
                {
                    int choice = MenuSystem.MenuInput(
                            new[] { "Ogiltig inmatning. Försök igen." },
                            new[] { "Försök", "Meny" },
                            null
                        );
                    if (choice == 0) continue;
                    return;
                }

                if (!int.TryParse(input, out int userId))
                {
                    var user = _context.Users.Find(userId);

                    if (user == null)
                    {
                        int choice = MenuSystem.MenuInput(
                            new[] { "Användare med det angivna ID:t hittades inte." },
                            new[] { "Försök igen", "Meny" },
                            null
                        );
                        if (choice == 0) continue;
                        return;
                    }
                    else
                    {
                        _context.Users.Remove(user);
                        _context.SaveChanges();

                        MenuSystem.MenuInput(
                            new[] { $"Användare med ID {userId} har raderats." },
                            new[] { "Meny" },
                            null
                        );
                    }
                }
            }
        }

        public void EditUserName()
        {
            while (true)
            {
                MenuSystem.CenterY(8);
                MenuSystem.Header();
                MenuSystem.WriteCenteredXForeground("ÄNDRA ANVÄNDARNAMN", ConsoleColor.Green, false);
                MenuSystem.WriteCenteredXForeground("Tryck på [ESC] för att avbryta.", ConsoleColor.Green, false);
                Console.WriteLine();
                MenuSystem.WriteCenteredXForeground("Ange användar-ID: ", ConsoleColor.Yellow, true);
                string? input = MenuSystem.ReadNumberWithEscape();

                if (input == null)
                {
                    MenuSystem.MenuInput(
                            new[] { "Ändring av användarnamn avbröts." },
                            new[] { "Meny" },
                            null
                        );
                    return;
                }

                if (input == "")
                {
                    int choice = MenuSystem.MenuInput(
                            new[] { "Ogiltig inmatning. Försök igen." },
                            new[] { "Försök", "Meny" },
                            null
                        );
                    if (choice == 0) continue;
                    return;
                }

                if (!int.TryParse(input, out int userId))
                {
                    var user = _context.Users.Find(userId);

                    if (user == null)
                    {
                        int choice = MenuSystem.MenuInput(
                            new[] { "Användare med det angivna ID:t hittades inte." },
                            new[] { "Försök igen", "Meny" },
                            null
                        );
                        if (choice == 0) continue;
                        return;
                    }
                    else
                    {
                        MenuSystem.WriteCenteredXForeground("Ange det nya användarnamnet: ", ConsoleColor.Yellow, true);
                        input = MenuSystem.ReadLineWithEscape();

                        if (input == null)
                        {
                            MenuSystem.MenuInput(
                                new[] { "Ändring av användarnamn avbröts." },
                                new[] { "Meny" },
                                null
                            );
                        }

                        if (input == "")
                        {
                            int choice = MenuSystem.MenuInput(
                                new[] { "Ogiltig inmatning. Försök igen." },
                                new[] { "Försök igen", "Meny" },
                                null
                            );
                            if (choice == 0) continue;
                            return;
                        }

                        user.Username = input!;
                        _context.SaveChanges();

                        MenuSystem.MenuInput(
                                new[] { $"Användarnamnet har uppdaterats till '{user.Username}'." },
                                new[] { "Meny" },
                                null
                        );
                        return;
                    }
                }
            }
        }

        public void GetAllBankAccounts()
        {
            var accounts = _context.BankAccounts.ToList();

            if (!accounts.Any())
            {
                MenuSystem.MenuInput(
                    new[] { "Inga bankkonton hittades." },
                    new[] { "Meny" },
                    null
                );
                return;
            }

            var accountStrings = accounts
                .Select(a => $"ID: [{a.Id}] | Titel: [{a.Title}] | Kontosaldo: [{a.Balance:C}] | Användar-ID: [{a.UserId}]")
                .ToArray();

            accountStrings = new[] { "ALLA BANKKONTON" }.Concat(accountStrings).ToArray();

            MenuSystem.MenuInput(
                accountStrings,
                new[] { "Meny" },
                null
            );
        }

        public void GetAllUsers()
        {
            var users = _context.Users.ToList();

            if (!users.Any())
            {
                MenuSystem.MenuInput(
                    new[] { "Inga användare hittades." },
                    new[] { "Meny" },
                    null
                );
                return;
            }

            var userStrings = users
                .Select(u => $"ID: [{u.Id}] | Namn: [{u.Username}]")
                .ToArray();

            userStrings = new[] { "ANVÄNDARE" }.Concat(userStrings).ToArray();

            MenuSystem.MenuInput(
                userStrings,
                new[] { "Meny" },
                null
            );
        }
    }
}
