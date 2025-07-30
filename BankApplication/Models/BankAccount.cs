namespace BankApplication.Models
{
    internal class BankAccount
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public decimal Balance { get; set; }
        public List<Transaction> Transactions { get; set; } = new();
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
