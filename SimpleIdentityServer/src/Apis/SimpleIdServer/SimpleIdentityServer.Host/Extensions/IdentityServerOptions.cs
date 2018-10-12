// Copyright 2015 Habart Thierry
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

namespace SimpleIdentityServer.Host.Extensions
{
    using System.Collections.Generic;
    using Core;
    using Core.Common;
    using Core.Common.Models;

    public class ScimOptions
    {
        public string EndPoint { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class OpenIdServerConfiguration
    {
        public List<ResourceOwner> Users { get; set; }
        public List<Core.Common.Models.Client> Clients { get; set; }
        public List<Translation> Translations { get; set; }
        public List<JsonWebKey> JsonWebKeys { get; set; }
    }

    public class IdentityServerOptions
    {
        public IdentityServerOptions()
        {
            Scim = new ScimOptions();
        }

        /// <summary>
        /// Scim options.
        /// </summary>
        public ScimOptions Scim { get; set; }
        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        public OpenIdServerConfiguration Configuration { get; set; }
        /// <summary>
        /// Gets or sets the OAUTH configuration options.
        /// </summary>
        public OAuthConfigurationOptions OAuthConfigurationOptions { get; set; }
    }
}
