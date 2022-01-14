namespace Frontend.Services;

public interface IBankService
{
    Task CreateAccount(string userName, decimal balance);
    Task<IEnumerable<AccountViewModel>?> ListAccounts();
}