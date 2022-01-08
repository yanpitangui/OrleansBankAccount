using System.Buffers;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Contracts;
using Contracts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IGrainFactory _grainFactory;

    public AccountController(IGrainFactory grainFactory, ILogger<AccountController> logger)
    {
        _grainFactory = grainFactory;
    }

    [HttpGet("{itemKey}")]
    public Task<Account> GetAsync([Required] Guid id) =>
        _grainFactory.GetGrain<IAccountGrain>(id).GetAsync();

    [HttpDelete("{itemKey}")]
    public async Task DeleteAsync([Required] Guid id)
    {
        await _grainFactory.GetGrain<IAccountGrain>(id).ClearAsync();
    }

    [HttpGet]
    public async Task<ImmutableArray<Account>> ListAsync()
    {
        // get all item keys for this owner
        var keys = await _grainFactory.GetGrain<IAccountManagerGrain>(0).GetAllAsync();

        // fast path for empty owner
        if (keys.Length == 0) return ImmutableArray<Account>.Empty;

        // fan out and get all individual items in parallel
        var tasks = ArrayPool<Task<Account>>.Shared.Rent(keys.Length);
        try
        {
            // issue all requests at the same time
            for (var i = 0; i < keys.Length; ++i)
            {
                tasks[i] = _grainFactory.GetGrain<IAccountGrain>(keys[i]).GetAsync();
            }

            // compose the result as requests complete
            var result = ImmutableArray.CreateBuilder<Account>(tasks.Length);
            for (var i = 0; i < keys.Length; ++i)
            {
                result.Add(await tasks[i]);
            }
            return result.ToImmutable();
        }
        finally
        {
            ArrayPool<Task<Account>>.Shared.Return(tasks);
        }
    }

    [HttpPost]
    public async Task<ActionResult> PostAsync([FromBody] AccountModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var item = new Account(Guid.NewGuid(), model.Username, model.Balance);
        await _grainFactory.GetGrain<IAccountGrain>(item.Key).SetAsync(item);
        return Ok();
    }

    public class AccountModel
    {
        [Required]
        public string? Username { get; set; }

        public int Balance { get; set; }
    }

}

