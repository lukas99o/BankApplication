using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using BankApplication.Data;
using BankApplication.Services.IServices;
using BankApplication.Services;
using Microsoft.Extensions.Logging;

namespace BankApplication
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlite("Data Source=bankapp.db")
                        .EnableSensitiveDataLogging(false)
                        .LogTo(_ => { }, LogLevel.None));

                    services.AddScoped<IAccountService, AccountService>();
                    services.AddScoped<IBankAccountService, BankAccountService>();
                    services.AddScoped<IFunService, FunService>();
                    services.AddScoped<IAdminService, AdminService>();
                    services.AddTransient<App>();
                })
                .Build();

            var app = host.Services.GetRequiredService<App>();
            app.Run();
        }
    }
}
