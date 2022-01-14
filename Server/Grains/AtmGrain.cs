using Contracts;
using Orleans;
using Orleans.Concurrency;

namespace Server.Grains
{
    [StatelessWorker]
    public class AtmGrain : Grain, IAtmGrain
    {
        public async Task Transfer(IAccountGrain fromAccount, IAccountGrain toAccount, uint amount)
        {
            await Task.WhenAll(fromAccount.Withdraw(amount), toAccount.Deposit(amount));

        }
    }
}
