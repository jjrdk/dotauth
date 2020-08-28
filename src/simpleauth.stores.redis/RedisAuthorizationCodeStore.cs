namespace SimpleAuth.Stores.Redis
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using StackExchange.Redis;

    /// <summary>
    /// Defines the Redis authorization code store.
    /// </summary>
    public class RedisAuthorizationCodeStore : IAuthorizationCodeStore
    {
        private readonly IDatabaseAsync _database;
        private readonly TimeSpan _expiry;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisAuthorizationCodeStore"/> class.
        /// </summary>
        /// <param name="database">The underlying Redis store.</param>
        /// <param name="expiry">The default cache expiration.</param>
        public RedisAuthorizationCodeStore(IDatabaseAsync database, TimeSpan expiry = default)
        {
            _database = database;
            _expiry = expiry == default ? TimeSpan.FromMinutes(30) : expiry;
        }

        /// <inheritdoc />
        public async Task<AuthorizationCode?> Get(string code, CancellationToken cancellationToken)
        {
            var authCode = await _database.StringGetAsync(code).ConfigureAwait(false);
            return authCode.HasValue ? JsonConvert.DeserializeObject<AuthorizationCode>(authCode) : null;
        }

        /// <inheritdoc />
        public Task<bool> Add(AuthorizationCode authorizationCode, CancellationToken cancellationToken)
        {
            var json = JsonConvert.SerializeObject(authorizationCode);
            return _database.StringSetAsync(authorizationCode.Code, json, _expiry);
        }

        /// <inheritdoc />
        public Task<bool> Remove(string code, CancellationToken cancellationToken)
        {
            return _database.KeyDeleteAsync(code);
        }
    }
}