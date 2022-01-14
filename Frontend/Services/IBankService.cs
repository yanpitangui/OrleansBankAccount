namespace Frontend.Services;

public interface IBankService
{
    Task CreateAccount(string userName, int balance);
    Task<IEnumerable<AccountViewModel>?> ListAccounts();
}