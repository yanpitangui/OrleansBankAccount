namespace Contracts.Models;

public class Account
{
    public Account(Guid key, string userName, decimal balance)
    {
        Key = key;
        UserName = userName;
        Balance = balance;
    }

    public Guid Key { get; set; }

    public string UserName { get; set; }

    public decimal Balance { get; set; }
}

