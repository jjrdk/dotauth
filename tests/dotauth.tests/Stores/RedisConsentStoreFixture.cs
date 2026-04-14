namespace DotAuth.Tests.Stores;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Stores.Redis;
using StackExchange.Redis;
using Testcontainers.Redis;
using Xunit;

public sealed class RedisConsentStoreFixture(RedisConsentStoreFixture.Context context) : IClassFixture<RedisConsentStoreFixture.Context>
{
    [Fact]
    public async Task When_Inserting_Multiple_Consents_For_The_Same_User_Then_All_Consents_Are_Returned()
    {
        await context.ResetAsync();
        var store = context.Store;

        await store.Insert(new Consent { Id = "1", Subject = "alice", ClientId = "client-a", GrantedScopes = ["manager"] }, CancellationToken.None);
        await store.Insert(new Consent { Id = "2", Subject = "alice", ClientId = "client-b", GrantedScopes = ["uma_protection"] }, CancellationToken.None);

        var consents = await store.GetConsentsForGivenUser("alice", CancellationToken.None);

        Assert.Equal(2, consents.Count);
        Assert.Contains(consents, c => c.Id == "1" && c.ClientId == "client-a");
        Assert.Contains(consents, c => c.Id == "2" && c.ClientId == "client-b");
    }

    [Fact]
    public async Task When_Reading_A_Legacy_Single_Consent_Payload_Then_It_Is_Returned()
    {
        await context.ResetAsync();
        var consent = new Consent { Id = "legacy", Subject = "alice", ClientId = "client-a", GrantedScopes = ["openid"] };
        await context.Database.StringSetAsync("alice", JsonSerializer.Serialize(consent, SharedSerializerContext.Default.Consent), TimeSpan.FromMinutes(5));
        var store = context.Store;

        var consents = await store.GetConsentsForGivenUser("alice", CancellationToken.None);

        var result = Assert.Single(consents);
        Assert.Equal("legacy", result.Id);
        Assert.Equal("client-a", result.ClientId);
    }

    [Fact]
    public async Task When_Deleting_A_Consent_Then_Other_Consents_For_That_User_Are_Kept()
    {
        await context.ResetAsync();
        var store = context.Store;
        var first = new Consent { Id = "1", Subject = "alice", ClientId = "client-a", GrantedScopes = ["manager"] };
        var second = new Consent { Id = "2", Subject = "alice", ClientId = "client-b", GrantedScopes = ["uma_protection"] };

        await store.Insert(first, CancellationToken.None);
        await store.Insert(second, CancellationToken.None);
        var deleted = await store.Delete(first, CancellationToken.None);
        var remaining = await store.GetConsentsForGivenUser("alice", CancellationToken.None);

        Assert.True(deleted);
        var consent = Assert.Single(remaining);
        Assert.Equal("2", consent.Id);
        Assert.Equal("client-b", consent.ClientId);
    }

    public sealed class Context : IAsyncLifetime, IAsyncDisposable
    {
        private readonly RedisContainer _redisContainer = new RedisBuilder("redis:latest").Build();
        private ConnectionMultiplexer _connectionMultiplexer = null!;

        public IDatabaseAsync Database => _connectionMultiplexer.GetDatabase();

        public RedisConsentStore Store { get; private set; } = null!;

        public async ValueTask InitializeAsync()
        {
            await _redisContainer.StartAsync();
            _connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(_redisContainer.GetConnectionString());
            Store = new RedisConsentStore(_connectionMultiplexer.GetDatabase());
        }

        public Task ResetAsync()
        {
            return Database.ExecuteAsync("FLUSHDB");
        }

        public async ValueTask DisposeAsync()
        {
            if (_connectionMultiplexer != null)
            {
                await _connectionMultiplexer.CloseAsync();
                _connectionMultiplexer.Dispose();
            }

            await _redisContainer.DisposeAsync();
        }

    }
}






