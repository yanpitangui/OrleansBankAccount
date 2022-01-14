using Contracts.Models;
using Orleans;

namespace Contracts;

public interface IAccountGrain : IGrainWithGuidKey
{
    [Transaction(TransactionOption.Join)]
    Task Withdraw(decimal amount);

    [Transaction(TransactionOption.Join)]
    Task Deposit(decimal amount);

    [Transaction(TransactionOption.CreateOrJoin)]
    Task<decimal> GetBalance();

    Task SetAsync(Account item);
    Task<Account?> GetAsync();

    Task ClearAsync();
}
