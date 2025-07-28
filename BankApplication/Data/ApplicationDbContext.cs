using BankApplication.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApplication.Data
{
    internal class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { } 

        public DbSet<User> Users { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
    }
}
