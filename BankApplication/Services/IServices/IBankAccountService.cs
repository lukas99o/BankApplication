using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApplication.Services.IServices
{
    internal interface IBankAccountService
    {
        void ViewBalance(int userId);
        void Deposit(int userId);
        void Withdraw(decimal amount);
        void Transfer(decimal amount, int targetAccountId);
        void ViewTransactionHistory();
        void CreateAccount(int userId);
    }
}
