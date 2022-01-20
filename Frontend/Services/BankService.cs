namespace Frontend.Services;

public class BankService : IBankService
{
    private readonly HttpClient _client;

    public BankService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("bankApi");
    }

    public Task<HttpResponseMessage> CreateAccount(string userName, decimal balance)
    {
        return _client
            .PostAsJsonAsync("/api/account", new AccountViewModel(Guid.Empty, userName, balance));
    }

    public Task<IEnumerable<AccountViewModel>?> ListAccounts() => _client.GetFromJsonAsync<IEnumerable<AccountViewModel>>("/api/account");
    public Task<HttpResponseMessage> TransferAmount(Guid? modelAccountFrom, Guid? modelAccountTo, decimal modelAmount)
    {
        return _client
            .PostAsJsonAsync($"/api/account/transfer/{modelAccountFrom}/{modelAccountTo}", modelAmount);
    }
}