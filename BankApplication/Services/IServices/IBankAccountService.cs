namespace BankApplication.Services.IServices
{
    internal interface IBankAccountService
    {
        void ViewAccounts(int userId);
        void Deposit(int userId);
        void Withdraw(int userId);
        void Transfer(int userId);
        void ViewTransactionHistory(int userId);
        bool CreateAccount(int userId);
        void RemoveAccount(int userId);
    }
}
