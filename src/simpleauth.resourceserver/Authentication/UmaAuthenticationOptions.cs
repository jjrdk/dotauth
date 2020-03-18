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
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    /// <summary>
    /// Defines the UMA authentication options.
    /// </summary>
    public class UmaAuthenticationOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// Gets or sets the UMA token authority <see cref="Uri"/>.
        /// </summary>
        public Uri Authority { get; set; }

        /// <summary>
        /// Gets or sets the application realm.
        /// </summary>
        public string Realm { get; set; }

        /// <summary>
        /// Gets or sets the realm resolver. Only used if Realm is null.
        /// </summary>
        public Func<HttpRequest, string> RealmResolver { get; set; } = r => string.Empty;

        /// <summary>
        /// Gets or sets the request paths for which UMA tokens will be issued.
        /// </summary>
        public Regex[] UmaResourcePaths { get; set; } = Array.Empty<Regex>();

        /// <summary>
        /// Gets or sets the delegate to define the resource request.
        /// </summary>
        public Func<HttpRequest, PermissionRequest[]> ResourceSetRequest { get; set; } = r => Array.Empty<PermissionRequest>();

        /// <summary>
        /// Gets or sets the <see cref="TokenValidationParameters"/> to use to validate the token.
        /// </summary>
        public TokenValidationParameters TokenValidationParameters { get; set; } = new TokenValidationParameters();

        /// <summary>
        /// Gets or sets the <see cref="ITokenCache">token cache</see>.
        /// </summary>
        public ITokenCache TokenCache { get; set; }

        /// <summary>
        /// Gets or sets the UMA discovery document.
        /// </summary>
        public DiscoveryInformation Configuration { get; set; }

        /// <summary>
        /// Gets or sets the token audience.
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// Gets or sets the client id of the requesting application.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret of the requesting application.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Uri"/> where to fetch the discovery document.
        /// </summary>
        public Uri DiscoveryDocumentUri { get; set; }

        /// <summary>
        /// Gets or sets whether HTTPS should be required when fetching metadata.
        /// </summary>
        public bool RequireHttpsMetadata { get; set; }

        /// <summary>
        /// Gets or sets the backchannel <see cref="HttpMessageHandler"/>.
        /// </summary>
        public HttpMessageHandler BackchannelHttpHandler { get; set; }
    }
}
