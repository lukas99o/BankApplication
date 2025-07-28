﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApplication.Models
{
    internal class BankAccount
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public decimal Balance { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
