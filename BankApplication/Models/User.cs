namespace BankApplication.Models
{
    internal class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public List<BankAccount> Accounts { get; set; } = new();
    }
}
