﻿using System.Buffers;
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

    [HttpGet("{id:guid}")]
    public Task<Account?> GetAsync([Required] Guid id) =>
        _grainFactory.GetGrain<IAccountGrain>(id).GetAsync();

    [HttpDelete("{id:guid}")]
    public async Task DeleteAsync([Required] Guid id)
    {
        await _grainFactory.GetGrain<IAccountGrain>(id).ClearAsync();
    }

    [HttpGet]
    public async Task<ImmutableArray<Account?>> ListAsync()
    {
        // get all item keys for this owner
        var keys = await _grainFactory.GetGrain<IAccountManagerGrain>(0).GetAllAsync();

        // fast path for empty owner
        if (keys.Length == 0) return ImmutableArray<Account?>.Empty;

        // fan out and get all individual items in parallel
        var tasks = ArrayPool<Task<Account?>>.Shared.Rent(keys.Length);
        try
        {
            // issue all requests at the same time
            for (var i = 0; i < keys.Length; ++i)
            {
                tasks[i] = _grainFactory.GetGrain<IAccountGrain>(keys[i]).GetAsync();
            }

            // compose the result as requests complete
            var result = ImmutableArray.CreateBuilder<Account?>(tasks.Length);
            for (var i = 0; i < keys.Length; ++i)
            {
                result.Add(await tasks[i]);
            }
            return result.ToImmutable();
        }
        finally
        {
            ArrayPool<Task<Account?>>.Shared.Return(tasks);
        }
    }

    [HttpPost("transfer/{fromAccountId:guid}/{toAccountId:guid}")]
    public async Task<ActionResult> Transfer(Guid fromAccountId, Guid toAccountId, [FromBody] uint amount)
    {
        var fromAccount = _grainFactory.GetGrain<IAccountGrain>(fromAccountId);
        var toAccount = _grainFactory.GetGrain<IAccountGrain>(toAccountId);

        await Task.WhenAll(fromAccount.Withdraw(amount), toAccount.Deposit(amount));

        return Ok(new
        {
            fromBalance = await fromAccount.GetBalance(),
            toBalance = await toAccount.GetBalance()
        });
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

        public uint Balance { get; set; }
    }

}

