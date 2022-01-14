using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace Contracts;

public interface IAccountManagerGrain : IGrainWithIntegerKey
{
    public Task RegisterAsync(Guid itemKey);
    Task UnregisterAsync(Guid itemKey);
    Task<ImmutableArray<Guid>> GetAllAsync();

}