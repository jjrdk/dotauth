﻿// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

namespace DotAuth.AuthServer;

using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;

public sealed class Program
{
    public static async Task Main()
    {
#if DEBUG
        IdentityModelEventSource.ShowPII = true;
#endif
        Trace.Listeners.Clear();
        Trace.Listeners.Add(new ConsoleTraceListener
            { TraceOutputOptions = TraceOptions.DateTime | TraceOptions.ThreadId });
        using var webHost = new WebHostBuilder()
            .UseKestrel(
                o =>
                {
                    o.AddServerHeader = false;
                    o.ConfigureEndpointDefaults(l => l.Protocols = HttpProtocols.Http1AndHttp2AndHttp3);
                })
            .ConfigureAppConfiguration(c => c.AddEnvironmentVariables())
            .UseStartup<Startup>()
            .Build();
        await webHost
            .RunAsync()
            .ConfigureAwait(false);
    }
}
