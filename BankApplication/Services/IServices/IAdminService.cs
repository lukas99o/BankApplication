using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApplication.Services.IServices
{
    internal interface IAdminService
    {
        void GetAllUsers();
        void GetAllBankAccounts();
        void EditUserName();
        void DeleteUser();
    }
}
