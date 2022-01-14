using Orleans;

namespace Contracts
{
    public interface IAtmGrain : IGrainWithIntegerKey
    {
        [Transaction(TransactionOption.Create)]
        public Task Transfer(IAccountGrain fromAccount, IAccountGrain toAccount, uint amount);
    }
}
