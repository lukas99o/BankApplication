using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApplication.Services.IServices
{
    internal interface IFunService
    {
        void Gamble(int userId);
        void Leaderboard(int userId);
    }
}
