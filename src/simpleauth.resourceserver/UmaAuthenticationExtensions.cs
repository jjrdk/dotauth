// Copyright © 2016 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleAuth.ResourceServer
{
    using System;
    using System.Net.Http;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using SimpleAuth.Client;
    using SimpleAuth.ResourceServer.Authentication;

    public static class UmaAuthenticationExtensions
    {
        public static AuthenticationBuilder AddUmaTicket(
            this AuthenticationBuilder builder,
            string authenticationScheme = UmaAuthenticationDefaults.AuthenticationScheme,
            string displayName = "UMA Ticket",
            Action<UmaAuthenticationOptions> configureOptions = null)
        {
            builder.Services.AddSingleton<IPostConfigureOptions<UmaAuthenticationOptions>, PostConfigureUmaAuthenticationOptions>();
            var optionsBuilder = builder.Services.AddOptions<UmaAuthenticationOptions>();
            if (configureOptions != null)
            {
                optionsBuilder.Configure(configureOptions);
            }

            builder.Services.AddSingleton<IUmaPermissionClient, UmaClient>(
                sp =>
                {
                    var options = sp.GetRequiredService<IOptions<UmaAuthenticationOptions>>();
                    return new UmaClient(
                        sp.GetRequiredService<HttpClient>(),
                        options.Value.Authority);
                });

            return builder.AddScheme<UmaAuthenticationOptions, UmaAuthenticationHandler>(
                authenticationScheme,
                displayName,
                configureOptions);
        }
    }
}
