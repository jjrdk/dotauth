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
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Shared.Responses;

    public class UmaAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string Authority { get; set; }

        public string Realm { get; set; }

        public Regex[] UmaResourcePaths { get; set; }

        public TokenValidationParameters TokenValidationParameters { get; set; } = new TokenValidationParameters();

        public ITokenCache TokenCache { get; set; }

        public DiscoveryInformation Configuration { get; set; }

        public string Audience { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public Uri DiscoveryDocumentUri { get; set; }

        public bool RequireHttpsMetadata { get; set; }

        public HttpMessageHandler BackchannelHttpHandler { get; set; }
    }
}
