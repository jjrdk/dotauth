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

namespace SimpleAuth
{
    using Shared;
    using SimpleAuth.Repositories;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Net.Http;

    public class SimpleAuthOptions
    {
        public SimpleAuthOptions(
            TimeSpan authorizationCodeValidity = default,
            TimeSpan rptLifetime = default,
            TimeSpan ticketLifetime = default,
            string[] claimsIncludedInUserCreation = null,
            params string[] userClaimsToIncludeInAuthToken)
        {
            RptLifeTime = rptLifetime == default ? TimeSpan.FromSeconds(3600) : rptLifetime;
            TicketLifeTime = ticketLifetime == default ? TimeSpan.FromSeconds(3600) : ticketLifetime;
            AuthorizationCodeValidityPeriod = authorizationCodeValidity == default
                ? TimeSpan.FromSeconds(3600)
                : authorizationCodeValidity;
            ClaimsIncludedInUserCreation = claimsIncludedInUserCreation ?? Array.Empty<string>();
            UserClaimsToIncludeInAuthToken = userClaimsToIncludeInAuthToken ?? Array.Empty<string>();
        }

        public Func<HttpClient> HttpClientFactory { get; set; }
        public Func<IServiceProvider, IResourceOwnerRepository> Users { get; set; }
        public Func<IServiceProvider, IClientRepository> Clients { get; set; }
        public Func<IServiceProvider, IConsentRepository> Consents { get; set; }
        public Func<IServiceProvider, IScopeRepository> Scopes { get; set; }
        public Func<IServiceProvider, IPolicyRepository> Policies { get; set; }
        public Func<IServiceProvider, IResourceSetRepository> ResourceSets { get; set; }
        public Func<IServiceProvider, ITicketStore> Tickets { get; set; }
        public Func<IServiceProvider, ITokenStore> Tokens { get; set; }
        public Func<IServiceProvider, IFilterStore> AccountFilters { get; set; }
        public Func<IServiceProvider, IAuthorizationCodeStore> AuthorizationCodes { get; set; }
        public Func<IServiceProvider, IConfirmationCodeStore> ConfirmationCodes { get; set; }

        public Func<IServiceProvider, IEventPublisher> EventPublisher { get; set; }

        public Func<IServiceProvider, ISubjectBuilder> SubjectBuilder { get; set; }

        public TimeSpan AuthorizationCodeValidityPeriod { get; set; }

        public string[] UserClaimsToIncludeInAuthToken { get; set; }

        /// <summary>
        /// Gets or sets the RPT lifetime (seconds).
        /// </summary>
        public TimeSpan RptLifeTime { get; set; }
        /// <summary>
        /// Gets or sets the ticket lifetime (seconds).
        /// </summary>
        public TimeSpan TicketLifeTime { get; set; }

        /// <summary>
        /// Gets a list of claims include when the resource owner is created.
        /// If the list is empty then all the claims are included.
        /// </summary>
        public string[] ClaimsIncludedInUserCreation { get; set; }

        public string ApplicationName { get; set; } = "Simple Auth";
    }
}
