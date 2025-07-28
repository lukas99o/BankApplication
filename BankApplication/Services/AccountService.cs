using BankApplication.Data;
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

        public User? Login()
        {
            Console.Clear();
            User? user = null;
            int tries = 3;
            string? username = "";

            while (true)
            {
                Console.Write("Ange ditt användarnamn: ");
                username = Console.ReadLine();

                if (string.IsNullOrEmpty(username))
                {
                    Console.WriteLine("Anändarnamnet kan inte vara tomt.");
                    continue;
                }

                user = _context.Users.SingleOrDefault(u => u.Username == username);

                if (user == null && tries != 0)
                {
                    tries--;
                    Console.WriteLine($"Användarnamnet finns inte. Försök igen. Försök kvar: {tries}.");
                    continue;
                }
                else if (tries == 0)
                {
                    Console.WriteLine($"Försök kvar: {tries}. Försök igen senare.");
                    return null;
                }
                else
                {
                    return user;
                }
            }
        }

        public void Register()
        {
            Console.Clear();
            string? username = "";

            while (true)
            {
                Console.Write("Ange ett användarnamn för registrering: ");
                username = Console.ReadLine();

                if (string.IsNullOrEmpty(username))
                {
                    Console.WriteLine("Användarnamnet kan inte vara tomt.");
                }
                else 
                {
                    if (username.Length < 3 || username.Length > 20)
                    {
                        Console.WriteLine("Användarnamnet måste vara mellan 3 och 20 tecken långt.");
                        continue;
                    }
                    else if (_context.Users.Any(u => u.Username == username))
                    {
                        Console.WriteLine("Användarnamnet finns redan. Välj ett annat.");
                        continue;
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
