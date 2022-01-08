using System.Collections.Immutable;
using Contracts;
using Orleans;
using Orleans.Runtime;

namespace Server.Grains;

public class AccountManagerGrain : Grain, IAccountManagerGrain
{
    private readonly ILogger<AccountManagerGrain> _logger;
    private readonly IPersistentState<State> _state;

    public AccountManagerGrain(
        [PersistentState("State")] IPersistentState<State> state,
        ILogger<AccountManagerGrain> logger)
    {
        _logger = logger;
        _state = state;
    }

    public override Task OnActivateAsync()
    {
        _state.State.Items ??= new HashSet<Guid>();

        return base.OnActivateAsync();
    }

    public async Task RegisterAsync(Guid itemKey)
    {
        _state.State.Items!.Add(itemKey);
        await _state.WriteStateAsync();
    }

    public async Task UnregisterAsync(Guid itemKey)
    {
        _state.State.Items!.Remove(itemKey);
        await _state.WriteStateAsync();
    }

    public Task<ImmutableArray<Guid>> GetAllAsync() =>
        Task.FromResult(ImmutableArray.CreateRange(_state.State.Items!));

    [Serializable]
    public class State
    {
        public HashSet<Guid>? Items { get; set; }
    }
}



