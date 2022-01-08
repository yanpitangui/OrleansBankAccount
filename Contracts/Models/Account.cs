namespace Contracts.Models;

public class Account
{
    public Account(Guid key, string userName, int balance)
    {
        Key = key;
        UserName = userName;
        Balance = balance;
    }

    public Guid Key { get; set; }

    public string UserName { get; set; }

    public int Balance { get; set; }
}

