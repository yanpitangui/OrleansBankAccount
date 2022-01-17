namespace Frontend.Services;

public interface IBankService
{
    Task CreateAccount(string userName, decimal balance);
    Task<IEnumerable<AccountViewModel>?> ListAccounts();
    Task<HttpResponseMessage> TransferAmount(Guid? modelAccountFrom, Guid? modelAccountTo, decimal modelAmount);
}