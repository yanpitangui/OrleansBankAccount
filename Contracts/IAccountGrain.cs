using Contracts.Models;
using Orleans;

namespace Contracts;

public interface IAccountGrain : IGrainWithGuidKey
{
    [Transaction(TransactionOption.Join)]
    Task Withdraw(uint amount);

    [Transaction(TransactionOption.Join)]
    Task Deposit(uint amount);

    [Transaction(TransactionOption.CreateOrJoin)]
    Task<uint> GetBalance();

    Task SetAsync(Account item);
    Task<Account> GetAsync();

    Task ClearAsync();
}
