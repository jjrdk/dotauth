// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace DotAuth.Stores.Redis.AcceptanceTests;

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Logging;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;
using Xunit.Abstractions;

public sealed class EndpointFixture : IDisposable
{
    private const string BaseUrl = "http://localhost:5000";
    private readonly TestServerFixture _server;
    private readonly string _connectionString;
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RedisContainer _redisContainer;

    public EndpointFixture(ITestOutputHelper output)
    {_postgresContainer = new PostgreSqlBuilder().WithUsername("dotauth").WithPassword("dotauth")
            .WithDatabase("dotauth").Build();
        _redisContainer = new RedisBuilder().Build();
        _postgresContainer.StartAsync().GetAwaiter().GetResult();
        _redisContainer.StartAsync().GetAwaiter().GetResult();
        _connectionString = _postgresContainer.GetConnectionString();
        var redisConnectionString = _redisContainer.GetConnectionString();
        IdentityModelEventSource.ShowPII = true;

        _connectionString = DbInitializer.Init(
                output,
                _connectionString,
                DefaultStores.Consents(),
                DefaultStores.Users(),
                DefaultStores.Clients(SharedContext.Instance),
                DefaultStores.Scopes())
            .GetAwaiter().GetResult();

        _server = new TestServerFixture(output, _connectionString, redisConnectionString, BaseUrl);
    }

    [Theory]
    [InlineData("", HttpStatusCode.OK)]
    [InlineData("error?code=404", HttpStatusCode.NotFound)]
    [InlineData("error/404", HttpStatusCode.NotFound)]
    [InlineData("home", HttpStatusCode.OK)]
    [InlineData(".well-known/openid-configuration", HttpStatusCode.OK)]
    [InlineData("authenticate", HttpStatusCode.OK)]
    [InlineData("jwks", HttpStatusCode.OK)]
    public async Task WhenRequestingEndpointThenReturnsExpectedStatus(string path, HttpStatusCode statusCode)
    {
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Get, RequestUri = new Uri($"{BaseUrl}/{path}")
        };

        var httpResult = await _server.Client().SendAsync(httpRequest);

        Assert.Equal(statusCode, httpResult.StatusCode);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _server.Dispose();
        DbInitializer.Drop(_connectionString).Wait();
        _postgresContainer.StopAsync().GetAwaiter().GetResult();
        _redisContainer.StopAsync().GetAwaiter().GetResult();
    }
}
