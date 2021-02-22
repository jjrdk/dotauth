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
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using System;
    using SimpleAuth.Events;
    using SimpleAuth.Shared;

    /// <summary>
    /// Defines the SimpleAuth configuration options.
    /// </summary>
    public class SimpleAuthOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleAuthOptions"/> class.
        /// </summary>
        /// <param name="salt">The hashing salt.</param>
        /// <param name="authorizationCodeValidity">The authorization code validity.</param>
        /// <param name="rptLifetime">The RPT lifetime.</param>
        /// <param name="ticketLifetime">The ticket lifetime.</param>
        /// <param name="claimsIncludedInUserCreation">The claims included in user creation.</param>
        public SimpleAuthOptions(
            string? salt = default,
            TimeSpan authorizationCodeValidity = default,
            TimeSpan rptLifetime = default,
            TimeSpan ticketLifetime = default,
            string[]? claimsIncludedInUserCreation = null)
        {
            Salt = salt ?? string.Empty;
            RptLifeTime = rptLifetime == default ? TimeSpan.FromMinutes(30) : rptLifetime;
            TicketLifeTime = ticketLifetime == default ? TimeSpan.FromMinutes(30) : ticketLifetime;
            AuthorizationCodeValidityPeriod = authorizationCodeValidity == default
                ? TimeSpan.FromMinutes(30)
                : authorizationCodeValidity;
            ClaimsIncludedInUserCreation = claimsIncludedInUserCreation ?? Array.Empty<string>();
        }

        /// <summary>
        /// Gets or sets the administrator role definition for access management
        /// </summary>
        public (string roleName, string roleClaim) AdministratorRoleDefinition { get; set; } = (OpenIdClaimTypes.Role, "administrator");

        /// <summary>
        /// Gets or sets the hashing salt.
        /// </summary>
        public string Salt { get; set; }

        /// <summary>
        /// Gets or sets the delegate to run when resource owner created.
        /// </summary>
        /// <value>
        /// The on resource owner created.
        /// </value>
        public Action<ResourceOwner>? OnResourceOwnerCreated { get; set; }

        /// <summary>
        /// Gets or sets the users.
        /// </summary>
        /// <value>
        /// The users.
        /// </value>
        public Func<IServiceProvider, IResourceOwnerRepository>? Users { get; set; }

        /// <summary>
        /// Gets or sets the clients.
        /// </summary>
        /// <value>
        /// The clients.
        /// </value>
        public Func<IServiceProvider, IClientRepository>? Clients { get; set; }

        /// <summary>
        /// Gets or sets the json web keys.
        /// </summary>
        /// <value>
        /// The json web keys.
        /// </value>
        public Func<IServiceProvider, IJwksRepository>? JsonWebKeys { get; set; }

        /// <summary>
        /// Gets or sets the consents.
        /// </summary>
        /// <value>
        /// The consents.
        /// </value>
        public Func<IServiceProvider, IConsentRepository>? Consents { get; set; }

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        public Func<IServiceProvider, IScopeRepository>? Scopes { get; set; }

        /// <summary>
        /// Gets or sets the resource sets.
        /// </summary>
        /// <value>
        /// The resource sets.
        /// </value>
        public Func<IServiceProvider, IResourceSetRepository>? ResourceSets { get; set; }

        /// <summary>
        /// Gets or sets the tickets.
        /// </summary>
        /// <value>
        /// The tickets.
        /// </value>
        public Func<IServiceProvider, ITicketStore>? Tickets { get; set; }

        /// <summary>
        /// Gets or sets the tokens.
        /// </summary>
        /// <value>
        /// The tokens.
        /// </value>
        public Func<IServiceProvider, ITokenStore>? Tokens { get; set; }

        /// <summary>
        /// Gets or sets the account filters.
        /// </summary>
        /// <value>
        /// The account filters.
        /// </value>
        public Func<IServiceProvider, IFilterStore>? AccountFilters { get; set; }

        /// <summary>
        /// Gets or sets the authorization codes.
        /// </summary>
        /// <value>
        /// The authorization codes.
        /// </value>
        public Func<IServiceProvider, IAuthorizationCodeStore>? AuthorizationCodes { get; set; }

        /// <summary>
        /// Gets or sets the confirmation codes.
        /// </summary>
        /// <value>
        /// The confirmation codes.
        /// </value>
        public Func<IServiceProvider, IConfirmationCodeStore>? ConfirmationCodes { get; set; }

        /// <summary>
        /// Gets or sets the event publisher.
        /// </summary>
        /// <value>
        /// The event publisher.
        /// </value>
        public Func<IServiceProvider, IEventPublisher>? EventPublisher { get; set; }

        /// <summary>
        /// Gets or sets the subject builder.
        /// </summary>
        /// <value>
        /// The subject builder.
        /// </value>
        public Func<IServiceProvider, ISubjectBuilder>? SubjectBuilder { get; set; }

        /// <summary>
        /// Gets or sets the authorization code validity period.
        /// </summary>
        /// <value>
        /// The authorization code validity period.
        /// </value>
        public TimeSpan AuthorizationCodeValidityPeriod { get; set; }

        /// <summary>
        /// Gets or sets the RPT lifetime.
        /// </summary>
        public TimeSpan RptLifeTime { get; set; }

        /// <summary>
        /// Gets or sets the PAT lifetime.
        /// </summary>
        public TimeSpan PatLifeTime { get; set; }

        /// <summary>
        /// Gets or sets the ticket lifetime.
        /// </summary>
        public TimeSpan TicketLifeTime { get; set; }

        /// <summary>
        /// Gets a list of claims include when the resource owner is created.
        /// If the list is empty then all the claims are included.
        /// </summary>
        public string[] ClaimsIncludedInUserCreation { get; set; }

        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        /// <value>
        /// The name of the application.
        /// </value>
        public string ApplicationName { get; set; } = "SimpleAuth";

        /// <summary>
        /// Gets or sets whether to redirect home screen to login screen.
        /// </summary>
        public bool RedirectToLogin { get; set; }
    }
}
