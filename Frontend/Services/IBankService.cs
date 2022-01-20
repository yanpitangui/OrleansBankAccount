namespace Frontend.Services;

public interface IBankService
{
    Task<HttpResponseMessage> CreateAccount(string userName, decimal balance);
    Task<IEnumerable<AccountViewModel>?> ListAccounts();
    Task<HttpResponseMessage> TransferAmount(Guid? modelAccountFrom, Guid? modelAccountTo, decimal modelAmount);
}