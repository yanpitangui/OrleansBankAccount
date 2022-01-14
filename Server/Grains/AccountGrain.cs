using Contracts;
using Contracts.Models;
using Newtonsoft.Json.Linq;
using Orleans;
using Orleans.Runtime;
using Orleans.Transactions.Abstractions;

namespace Server.Grains;

[Serializable]
public class Balance
{
    public decimal Value { get; set; } = 0;
}

public class AccountGrain : Grain, IAccountGrain
{
    private readonly ILogger<AccountGrain> _logger;
    private readonly ITransactionalState<Balance> _balance;
    private readonly IPersistentState<State> _state;
    private string GrainType => nameof(AccountGrain);
    private Guid GrainKey => this.GetPrimaryKey();

    public AccountGrain(
        [TransactionalState("balance")] ITransactionalState<Balance> balance,
        ILogger<AccountGrain> logger,
        [PersistentState("State")] IPersistentState<State> state)
    {
        _balance = balance ?? throw new ArgumentNullException(nameof(balance));
        _logger = logger;
        _state = state;
    }

    public Task<Account?> GetAsync() => Task.FromResult(_state.State.Item);


    public async Task Deposit(decimal amount)
    {
        await _balance.PerformUpdate(x =>
        {
            _logger.LogInformation("Depositing {0} amount into account {1}", amount, this.GetPrimaryKey());
            return x.Value += amount;
        });
        _state.State.Item!.Balance = await GetBalance();
        await _state.WriteStateAsync();
    }

    public async Task Withdraw(decimal amount)
    {
        await _balance.PerformUpdate(x =>
        {
            _logger.LogInformation("Withdrawing {0} amount from account {1}", amount, this.GetPrimaryKey());
            if (x.Value < amount)
            {
                throw new InvalidOperationException(
                    $"Withdrawing {amount} credits from account \"{this.GetPrimaryKey()}\" would overdraw it."
                    + $" This account has {x.Value} credits.");
            }

            x.Value -= amount;
        });
        _state.State.Item!.Balance = await GetBalance();
        await _state.WriteStateAsync();
    }

    public Task<decimal> GetBalance() => _balance.PerformRead(x => x.Value);
    
    public async Task SetAsync(Account item)
    {
        // ensure the key is consistent
        if (item.Key != GrainKey)
        {
            throw new InvalidOperationException();
        }

        // save the item
        _state.State.Item = item;
        await _state.WriteStateAsync();

        // register the item with its owner list
        await GrainFactory.GetGrain<IAccountManagerGrain>(0)
            .RegisterAsync(item.Key);

        // for sample debugging
        _logger.LogInformation(
            "{@GrainType} {@GrainKey} now contains {@Account}",
            GrainType, GrainKey, JObject.FromObject(item));

        // notify listeners - best effort only
    }

    public async Task ClearAsync()
    {
        // fast path for already cleared state
        if (_state.State.Item == null) return;

        // hold on to the keys
        var itemKey = _state.State.Item.Key;

        // unregister from the registry
        await GrainFactory.GetGrain<IAccountManagerGrain>(0)
            .UnregisterAsync(itemKey);

        // clear the state
        await _state.ClearStateAsync();

        // for sample debugging
        _logger.LogInformation(
            "{@GrainType} {@GrainKey} is now cleared",
            GrainType, GrainKey);

        // notify listeners - best effort only

        // no need to stay alive anymore
        DeactivateOnIdle();
    }

    [Serializable]
    public class State
    {
        public Account? Item { get; set; }
    }
}

