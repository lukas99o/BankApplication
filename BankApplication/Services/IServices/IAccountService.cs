﻿using BankApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApplication.Services.IServices
{
    internal interface IAccountService
    {
        User? Login();
        void Register();
    }
}
