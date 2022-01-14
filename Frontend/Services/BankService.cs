namespace Frontend.Services;

public class BankService : IBankService
{
    private readonly HttpClient _client;

    public BankService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("bankApi");
    }

    public Task CreateAccount(string userName, int balance)
    {
        return _client
            .PostAsJsonAsync("/api/account", new AccountViewModel(Guid.Empty, userName, balance));
    }

    public Task<IEnumerable<AccountViewModel>?> ListAccounts() => _client.GetFromJsonAsync<IEnumerable<AccountViewModel>>("/api/account");
}