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

namespace SimpleAuth.Server.Tests.Policies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using SimpleAuth.Parameters;
    using SimpleAuth.Policies;
    using SimpleAuth.Repositories;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Responses;
    using Xunit;

    public class BasicAuthorizationPolicyFixture
    {
        private readonly IAuthorizationPolicy _authorizationPolicy;

        public BasicAuthorizationPolicyFixture()
        {
            _authorizationPolicy = new DefaultAuthorizationPolicy(
                new Mock<IClientStore>().Object,
                new InMemoryJwksRepository());
        }

        [Fact]
        public async Task WhenPassingNullTicketLineParameterThenExceptionsAreThrown()
        {
            await Assert
                .ThrowsAsync<NullReferenceException>(
                    () => _authorizationPolicy.Execute(null, null, CancellationToken.None, new PolicyRule()))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task WhenPassingNullPolicyParameterThenIsNotAuthorized()
        {
            var result = await _authorizationPolicy.Execute(
                         new TicketLineParameter("client_id"),
                         null,
                         CancellationToken.None,
                         null).ConfigureAwait(false);

            Assert.Equal(AuthorizationPolicyResultKind.NotAuthorized, result.Result);
        }

        [Fact]
        public async Task When_Does_Not_have_Permission_To_Access_To_Scope_Then_NotAuthorized_Is_Returned()
        {
            var ticket = new TicketLineParameter("client_id") { Scopes = new[] { "read", "create", "update" } };

            var authorizationPolicy = new[] { new PolicyRule { Scopes = new[] { "read" } } };

            var result = await _authorizationPolicy.Execute(ticket, null, CancellationToken.None, authorizationPolicy)
                .ConfigureAwait(false);

            Assert.Equal(AuthorizationPolicyResultKind.NotAuthorized, result.Result);
        }

        [Fact]
        public async Task When_Client_Is_Not_Allowed_Then_NotAuthorized_Is_Returned()
        {
            var ticket = new TicketLineParameter("invalid_client_id") { Scopes = new[] { "read", "create", "update" } };

            var authorizationPolicy = new PolicyRule
            {
                ClientIdsAllowed = new[] { "client_id" },
                Scopes = new[] { "read", "create", "update" }
            };

            var result = await _authorizationPolicy.Execute(ticket, null, CancellationToken.None, authorizationPolicy)
                .ConfigureAwait(false);

            Assert.Equal(AuthorizationPolicyResultKind.NotAuthorized, result.Result);
        }

        [Fact]
        public async Task When_There_Is_No_Access_Token_Passed_Then_NeedInfo_Is_Returned()
        {
            const string configurationUrl = "http://localhost/configuration";
            var ticket = new TicketLineParameter("client_id") { Scopes = new[] { "read", "create", "update" } };

            var authorizationPolicy = new[]
            {
                new PolicyRule
                {
                    ClientIdsAllowed = new[] {"client_id"},
                    Scopes = new[] {"read", "create", "update"},
                    Claims = new[]
                    {
                        new ClaimData {Type = "name", Value = ""},
                        new ClaimData {Type = "email", Value = ""}
                    },
                    OpenIdProvider = configurationUrl
                }
            };
            var claimTokenParameter = new ClaimTokenParameter { Format = "bad_format", Token = "token" };

            var result = await _authorizationPolicy.Execute(
                    ticket,
                    claimTokenParameter,
                    CancellationToken.None,
                    authorizationPolicy)
                .ConfigureAwait(false);

            Assert.Equal(AuthorizationPolicyResultKind.NeedInfo, result.Result);

            var errorDetails = (Dictionary<string, object>)result.ErrorDetails;

            Assert.True(errorDetails.ContainsKey(UmaConstants.ErrorDetailNames.RequestingPartyClaims));

            var requestingPartyClaims =
                (Dictionary<string, object>)errorDetails[UmaConstants.ErrorDetailNames.RequestingPartyClaims];

            Assert.True(requestingPartyClaims.ContainsKey(UmaConstants.ErrorDetailNames.RequiredClaims));
            Assert.True(requestingPartyClaims.ContainsKey(UmaConstants.ErrorDetailNames.RedirectUser));

            var requiredClaims =
                (List<Dictionary<string, string>>)requestingPartyClaims[UmaConstants.ErrorDetailNames.RequiredClaims];

            Assert.Contains(
                requiredClaims,
                r => r.Any(kv => kv.Key == UmaConstants.ErrorDetailNames.ClaimName && kv.Value == "name"));
            Assert.Contains(
                requiredClaims,
                r => r.Any(kv => kv.Key == UmaConstants.ErrorDetailNames.ClaimFriendlyName && kv.Value == "name"));
            Assert.Contains(
                requiredClaims,
                r => r.Any(kv => kv.Key == UmaConstants.ErrorDetailNames.ClaimName && kv.Value == "email"));
            Assert.Contains(
                requiredClaims,
                r => r.Any(kv => kv.Key == UmaConstants.ErrorDetailNames.ClaimFriendlyName && kv.Value == "email"));
        }

        [Fact]
        public async Task When_JwsPayload_Cannot_Be_Extracted_Then_NotAuthorized_Is_Returned()
        {
            const string configurationUrl = "http://localhost/configuration";
            var ticket = new TicketLineParameter("client_id") { Scopes = new[] { "read", "create", "update" } };

            var authorizationPolicy = new[]
            {
                new PolicyRule
                {
                    ClientIdsAllowed = new[] {"client_id"},
                    Scopes = new[] {"read", "create", "update"},
                    Claims = new[]
                    {
                        new ClaimData {Type = "name", Value = ""},
                        new ClaimData {Type = "email", Value = ""}
                    },
                    OpenIdProvider = configurationUrl
                }
            };
            var claimTokenParameters = new ClaimTokenParameter
            {
                Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                Token = "token"
            };

            var result = await _authorizationPolicy.Execute(
                    ticket,
                    claimTokenParameters,
                    CancellationToken.None,
                    authorizationPolicy)
                .ConfigureAwait(false);

            Assert.Equal(AuthorizationPolicyResultKind.NeedInfo, result.Result);
        }

        [Fact]
        public async Task When_Role_Is_Not_Correct_Then_NotAuthorized_Is_Returned()
        {
            const string configurationUrl = "http://localhost/configuration";
            var ticket = new TicketLineParameter("client_id") { Scopes = new[] { "read", "create", "update" } };

            var authorizationPolicy = new[]
            {
                new PolicyRule
                {
                    ClientIdsAllowed = new[] {"client_id"},
                    Scopes = new[] {"read", "create", "update"},
                    Claims = new[]
                    {
                        new ClaimData {Type = "role", Value = "role1"},
                        new ClaimData {Type = "role", Value = "role2"}
                    },
                    OpenIdProvider = configurationUrl
                }
            };
            var claimTokenParameter = new ClaimTokenParameter
            {
                Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                Token = "token"
            };

            var result = await _authorizationPolicy.Execute(
                    ticket,
                    claimTokenParameter,
                    CancellationToken.None,
                    authorizationPolicy)
                .ConfigureAwait(false);

            Assert.Equal(AuthorizationPolicyResultKind.NeedInfo, result.Result);
        }

        [Fact]
        public async Task When_There_Is_No_Role_Then_NotAuthorized_Is_Returned()
        {
            const string configurationUrl = "http://localhost/configuration";
            var ticket = new TicketLineParameter("client_id") { Scopes = new[] { "read", "create", "update" } };

            var authorizationPolicy = new[]
            {
                new PolicyRule
                {
                    ClientIdsAllowed = new[] {"client_id"},
                    Scopes = new[] {"read", "create", "update"},
                    Claims = new[]
                    {
                        new ClaimData {Type = "role", Value = "role1"},
                        new ClaimData {Type = "role", Value = "role2"}
                    },
                    OpenIdProvider = configurationUrl
                }
            };
            var claimTokenParameters = new ClaimTokenParameter
            {
                Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                Token = "token"
            };

            var result = await _authorizationPolicy.Execute(
                    ticket,
                    claimTokenParameters,
                    CancellationToken.None,
                    authorizationPolicy)
                .ConfigureAwait(false);

            Assert.Equal(AuthorizationPolicyResultKind.NeedInfo, result.Result);
        }

        [Fact]
        public async Task When_Passing_Not_Valid_Roles_In_JArray_Then_NotAuthorized_Is_Returned()
        {
            const string configurationUrl = "http://localhost/configuration";
            var ticket = new TicketLineParameter("client_id") { Scopes = new[] { "read", "create", "update" } };

            var authorizationPolicy = new[]
            {
                new PolicyRule
                {
                    ClientIdsAllowed = new[] {"client_id"},
                    Scopes = new[] {"read", "create", "update"},
                    Claims = new[]
                    {
                        new ClaimData {Type = "role", Value = "role1"},
                        new ClaimData {Type = "role", Value = "role2"}
                    },
                    OpenIdProvider = configurationUrl
                }
            };
            var claimTokenParameters = new ClaimTokenParameter
            {
                Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                Token = "token"
            };

            var result = await _authorizationPolicy.Execute(
                    ticket,
                    claimTokenParameters,
                    CancellationToken.None,
                    authorizationPolicy)
                .ConfigureAwait(false);

            Assert.Equal(AuthorizationPolicyResultKind.NeedInfo, result.Result);
        }

        [Fact]
        public async Task When_Passing_Not_Valid_Roles_InStringArray_Then_NotAuthorized_Is_Returned()
        {
            const string configurationUrl = "http://localhost/configuration";
            var ticket = new TicketLineParameter("client_id") { Scopes = new[] { "read", "create", "update" } };

            var authorizationPolicy = new[]
            {
                new PolicyRule
                {
                    ClientIdsAllowed = new[] {"client_id"},
                    Scopes = new[] {"read", "create", "update"},
                    Claims = new[]
                    {
                        new ClaimData {Type = "role", Value = "role1"},
                        new ClaimData {Type = "role", Value = "role2"}
                    },
                    OpenIdProvider = configurationUrl
                }
            };
            var claimTokenParameter = new ClaimTokenParameter
            {
                Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                Token = "token"
            };

            var result = await _authorizationPolicy.Execute(
                    ticket,
                    claimTokenParameter,
                    CancellationToken.None,
                    authorizationPolicy)
                .ConfigureAwait(false);

            Assert.Equal(AuthorizationPolicyResultKind.NeedInfo, result.Result);
        }

        [Fact]
        public async Task When_Claims_Are_Not_Correct_Then_NotAuthorized_Is_Returned()
        {
            const string configurationUrl = "http://localhost/configuration";
            var ticket = new TicketLineParameter("client_id") { Scopes = new[] { "read", "create", "update" } };

            var authorizationPolicy = new[]
            {
                new PolicyRule
                {
                    ClientIdsAllowed = new[] {"client_id"},
                    Scopes = new[] {"read", "create", "update"},
                    Claims = new[]
                    {
                        new ClaimData {Type = "name", Value = "name"},
                        new ClaimData {Type = "email", Value = "email"}
                    },
                    OpenIdProvider = configurationUrl
                }
            };
            var claimTokenParameter = new ClaimTokenParameter
            {
                Format = "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                Token = "token"
            };

            var result = await _authorizationPolicy.Execute(
                    ticket,
                    claimTokenParameter,
                    CancellationToken.None,
                    authorizationPolicy)
                .ConfigureAwait(false);

            Assert.Equal(AuthorizationPolicyResultKind.NeedInfo, result.Result);
        }

        [Fact]
        public async Task When_ResourceOwnerConsent_Is_Required_Then_RequestSubmitted_Is_Returned()
        {
            var ticket = new TicketLineParameter("client_id")
            {
                IsAuthorizedByRo = false,
                Scopes = new[] { "read", "create", "update" }
            };

            var authorizationPolicy = new[]
            {
                new PolicyRule
                {
                    ClientIdsAllowed = new[] {"client_id"},
                    IsResourceOwnerConsentNeeded = true,
                    Scopes = new[] {"read", "create", "update"}
                }
            };

            var result = await _authorizationPolicy.Execute(ticket, null, CancellationToken.None, authorizationPolicy)
                .ConfigureAwait(false);

            Assert.Equal(AuthorizationPolicyResultKind.RequestSubmitted, result.Result);
        }

        [Fact]
        public async Task When_AuthorizationPassed_Then_Authorization_Is_Returned()
        {
            var ticket = new TicketLineParameter("client_id") { IsAuthorizedByRo = true, Scopes = new[] { "create" } };

            var authorizationPolicy = new[]
            {
                new PolicyRule
                {
                    ClientIdsAllowed = new[] {"client_id"},
                    IsResourceOwnerConsentNeeded = true,
                    Scopes = new[] {"create"}
                }
            };

            var result = await _authorizationPolicy.Execute(ticket, null, CancellationToken.None, authorizationPolicy)
                .ConfigureAwait(false);

            Assert.Equal(AuthorizationPolicyResultKind.Authorized, result.Result);
        }
    }
}
