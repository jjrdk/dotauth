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

namespace SimpleAuth.ResourceServer.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Client;
    using SimpleAuth.Shared.Responses;

    internal class PostConfigureUmaAuthenticationOptions : IPostConfigureOptions<UmaAuthenticationOptions>
    {
        public void PostConfigure(string name, UmaAuthenticationOptions options)
        {
            if (options.Authority == null)
            {
                throw new InvalidOperationException("Authority must be provided in options.");
            }

            if (string.IsNullOrEmpty(options.ClientId))
            {
                throw new InvalidOperationException("Client Id must be provided.");
            }

            if (string.IsNullOrEmpty(options.ClientSecret))
            {
                throw new InvalidOperationException("Client secret must be provided.");
            }

            if (options.TokenValidationParameters == null)
            {
                throw new InvalidOperationException("TokenValidationParameters must be provided in options.");
            }

            if (options.Configuration == null && options.DiscoveryDocumentUri == null)
            {
                var builder = new UriBuilder(
                       options.Authority.Scheme,
                       options.Authority.Host,
                       options.Authority.Port,
                       "/.well-known/uma2-configuration");
                options.DiscoveryDocumentUri = builder.Uri;
            }

            if (options.Configuration == null
                && options.RequireHttpsMetadata
                && !string.Equals(
                    options.DiscoveryDocumentUri?.Scheme,
                    Uri.UriSchemeHttps,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The discovery document uri must use HTTPS unless disabled for development by setting RequireHttpsMetadata=false.");
            }

            if (string.IsNullOrEmpty(options.TokenValidationParameters.ValidAudience) && !string.IsNullOrEmpty(options.Audience))
            {
                options.TokenValidationParameters.ValidAudience = options.Audience;
            }

            if (options.TokenCache == null)
            {
                if (options.Configuration != null)
                {
                    options.TokenCache = new TokenCache(
                        options.ClientId,
                        options.ClientSecret,
                        options.Configuration,
                        options.BackchannelHttpHandler);
                }
                else
                {
                    options.TokenCache = new TokenCache(
                        options.ClientId,
                        options.ClientSecret,
                        options.DiscoveryDocumentUri,
                        options.BackchannelHttpHandler);
                }
            }
        }
    }
}