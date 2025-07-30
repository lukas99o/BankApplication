namespace BankApplication.Models
{
    
    internal class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public required string BankAccountTitle { get; set; }
        public TransactionType Type { get; set; }
        public bool? Sender { get; set; }
        public int BankAccountId { get; set; }
        public BankAccount? BankAccount { get; set; }

        public string? OtherBankAccountTitle { get; set; }

        public enum TransactionType
        {
            Deposit,
            Withdrawal,
            Transfer
        }
    }
}

