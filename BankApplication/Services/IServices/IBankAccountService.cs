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
        void Withdraw(int userId);
        void Transfer(int userId);
        void ViewTransactionHistory(int userId);
        void CreateAccount(int userId);
        void TransferToExternalAccount(int userId);
    }
}
