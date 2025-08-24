using BankApplication.Data;
using BankApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApplication.Utilities
{
    internal class SeedAdmin
    {
        public static void Init(ApplicationDbContext context)
        {
            if (!context.Users.Any(u => u.Username == "admin"))
            {
                var admin = new User
                {
                    Username = "admin",
                    Password = PasswordHash.HashPassword("admin123") 
                };

                context.Users.Add(admin);
                context.SaveChanges();
            }
        }
    }
}
