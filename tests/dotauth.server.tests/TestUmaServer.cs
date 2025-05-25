﻿// Copyright © 2018 Habart Thierry, © 2018 Jacob Reimers
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

namespace DotAuth.Server.Tests;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit.Abstractions;

public sealed class TestUmaServer : IDisposable
{
    public TestServer Server { get; }
    public Func<HttpClient> Client { get; }
    public SharedUmaContext SharedUmaCtx { get; }

    public TestUmaServer(ITestOutputHelper outputHelper)
    {
        SharedUmaCtx = new SharedUmaContext();
        var startup = new FakeUmaStartup(outputHelper);
        Server = new TestServer(new WebHostBuilder()
            .UseUrls("http://localhost:5000")
            .ConfigureServices(services =>
            {
                startup.ConfigureServices(services);
            })
            .UseSetting(WebHostDefaults.ApplicationKey, typeof(FakeUmaStartup).Assembly.FullName)
            .Configure(startup.Configure));
        Client = () =>
        {
            var c = Server.CreateClient();
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return c;
        };
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Server.Dispose();
        Client.Invoke().Dispose();
    }
}
