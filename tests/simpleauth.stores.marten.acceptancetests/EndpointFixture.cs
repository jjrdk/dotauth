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

namespace SimpleAuth.Stores.Marten.AcceptanceTests
{
    using Microsoft.IdentityModel.Logging;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    public class EndpointFixture : IDisposable
    {
        private const string BaseUrl = "http://localhost:5000";
        private readonly TestServerFixture _server;
        private readonly string _connectionString;

        public EndpointFixture()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false).Build();

            IdentityModelEventSource.ShowPII = true;

            _connectionString = DbInitializer.Init(
                    configuration["Db:ConnectionString"],
                    DefaultStores.Consents(),
                    DefaultStores.Users(),
                    DefaultStores.Clients(SharedContext.Instance),
                    DefaultStores.Scopes())
                .Result;

            _server = new TestServerFixture(_connectionString, BaseUrl);
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
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{BaseUrl}/{path}")
            };

            var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);

            Assert.Equal(statusCode, httpResult.StatusCode);
        }

        public void Dispose()
        {
            _server?.Dispose();
            DbInitializer.Drop(_connectionString).Wait();
        }
    }
}
