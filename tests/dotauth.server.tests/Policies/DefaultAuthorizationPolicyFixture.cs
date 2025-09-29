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

namespace DotAuth.Server.Tests.Policies;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Policies;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Policies;
using DotAuth.Shared.Responses;
using Xunit;

public sealed class DefaultAuthorizationPolicyFixture
{
    private readonly IAuthorizationPolicy _authorizationPolicy;

    public DefaultAuthorizationPolicyFixture()
    {
        _authorizationPolicy = new DefaultAuthorizationPolicy();
    }

    [Fact]
    public async Task WhenTicketIsValidThenPolicyAuthorizes()
    {
        const string ticketJson = @"{
    ""id"": ""95FE0861AFF41E4ABECC748C026C36F8"",
    ""lines"": [
        {
            ""scopes"": [
                ""read""
            ],
            ""resource_id"": ""RES123""
        }
    ],
    ""created"": ""2021-04-30T21:42:25.7091988+00:00"",
    ""expires"": ""2021-04-30T22:12:25.7092013+00:00"",
    ""requester"": [
        {
            ""type"": ""sub"",
            ""value"": ""abc123""
        },
        {
            ""type"": ""name"",
            ""value"": ""A Tester""
        },
        {
            ""type"": ""email"",
            ""value"": ""a.tester@email.com""
        },
        {
            ""type"": ""given_name"",
            ""value"": ""Anne""
        },
        {
            ""type"": ""family_name"",
            ""value"": ""Tester""
        }
    ],
    ""resource_owner"": ""98765"",
    ""is_authorized_by_ro"": true
}";
        const string resourceSetJson = @"{
    ""_id"": ""RES123"",
    ""name"": ""tux.jpg"",
    ""type"": ""Picture"",
    ""owner"": ""98765"",
    ""resource_scopes"": [
        ""read""
    ],
    ""authorization_policies"": [
        {
            ""claims"": [{""type"":""email"", ""value"":""a.tester@email.com""}],
            ""scopes"": [
                ""read""
            ],
            ""clients"": [
                ""Test""
            ],
            ""consent_needed"": true
        }
    ]
}";
        var resourceSet =
            JsonSerializer.Deserialize<ResourceSet>(resourceSetJson, SharedSerializerContext.Default.ResourceSet);
        var ticket = JsonSerializer.Deserialize<Ticket>(ticketJson, SharedSerializerContext.Default.Ticket);
        var kind = AuthorizationPolicyResultKind.NotAuthorized;
        foreach (var ticketLine in ticket!.Lines)
        {
            var result = await _authorizationPolicy.Execute(
                    new TicketLineParameter("Test", ticketLine.Scopes, ticket.IsAuthorizedByRo),
                    UmaConstants.IdTokenType,
                    ticket.Requester.Select(c => new Claim(c.Type, c.Value)).ToArray(),
                    CancellationToken.None,
                    resourceSet!.AuthorizationPolicies)
                ;
            kind = result.Result;
            if (kind == AuthorizationPolicyResultKind.Authorized)
            {
                break;
            }
        }

        Assert.Equal(AuthorizationPolicyResultKind.Authorized, kind);
    }

    [Fact]
    public async Task WhenPassingNullTicketLineParameterThenExceptionIsThrown()
    {
        await Assert.ThrowsAsync<NullReferenceException>(() => _authorizationPolicy.Execute(
                null!,
                UmaConstants.IdTokenType,
                Array.Empty<Claim>(),
                CancellationToken.None,
                new PolicyRule()))
            ;
    }

    [Fact]
    public async Task When_Does_Not_have_Permission_To_Access_To_Scope_Then_NotAuthorized_Is_Returned()
    {
        var ticket = new TicketLineParameter("client_id", ["read", "create", "update"]);

        var authorizationPolicy = new[] { new PolicyRule { Scopes = ["read"] } };

        var result = await _authorizationPolicy.Execute(
                ticket,
                UmaConstants.IdTokenType,
                null!,
                CancellationToken.None,
                authorizationPolicy)
            ;

        Assert.Equal(AuthorizationPolicyResultKind.NotAuthorized, result.Result);
    }

    [Fact]
    public async Task When_Client_Is_Not_Allowed_Then_NotAuthorized_Is_Returned()
    {
        var ticket = new TicketLineParameter("invalid_client_id", ["read", "create", "update"]);

        var authorizationPolicy = new PolicyRule
        {
            ClientIdsAllowed = ["client_id"],
            Scopes = ["read", "create", "update"]
        };

        var result = await _authorizationPolicy.Execute(
                ticket,
                "http://openid.net/specs/openid-connect-core-1_0.html#IDToken",
                Array.Empty<Claim>(),
                CancellationToken.None,
                authorizationPolicy)
            ;

        Assert.Equal(AuthorizationPolicyResultKind.NotAuthorized, result.Result);
    }

    [Fact]
    public async Task When_There_Is_No_Access_Token_Passed_Then_NeedInfo_Is_Returned()
    {
        const string configurationUrl = "http://localhost/configuration";
        var ticket = new TicketLineParameter("client_id", ["read", "create", "update"]);

        var authorizationPolicy = new[]
        {
            new PolicyRule
            {
                ClientIdsAllowed = ["client_id"],
                Scopes = ["read", "create", "update"],
                Claims =
                [
                    new ClaimData { Type = "name", Value = "" },
                    new ClaimData { Type = "email", Value = "" }
                ],
                OpenIdProvider = configurationUrl
            }
        };

        var result = await _authorizationPolicy.Execute(
                ticket,
                "bad_format",
                Array.Empty<Claim>(),
                CancellationToken.None,
                authorizationPolicy)
            ;

        Assert.Equal(AuthorizationPolicyResultKind.NeedInfo, result.Result);

        var errorDetails = (Dictionary<string, object>)result.ErrorDetails!;

        Assert.True(errorDetails.ContainsKey("requesting_party_claims"));

        var requestingPartyClaims =
            (Dictionary<string, object>)errorDetails["requesting_party_claims"];

        Assert.True(requestingPartyClaims.ContainsKey("required_claims"));
        Assert.True(requestingPartyClaims.ContainsKey("redirect_user"));

        var requiredClaims =
            (List<Dictionary<string, string>>)requestingPartyClaims["required_claims"];

        Assert.Contains(
            requiredClaims,
            r => r.Any(kv => kv is { Key: "name", Value: "name" }));
        Assert.Contains(
            requiredClaims,
            r => r.Any(kv => kv is { Key: "friendly_name", Value: "name" }));
        Assert.Contains(
            requiredClaims,
            r => r.Any(kv => kv is { Key: "name", Value: "email" }));
        Assert.Contains(
            requiredClaims,
            r => r.Any(kv => kv is { Key: "friendly_name", Value: "email" }));
    }

    [Fact]
    public async Task When_JwsPayload_Cannot_Be_Extracted_Then_NotAuthorized_Is_Returned()
    {
        const string configurationUrl = "http://localhost/configuration";
        var ticket = new TicketLineParameter("client_id", ["read", "create", "update"]);

        var authorizationPolicy = new[]
        {
            new PolicyRule
            {
                ClientIdsAllowed = ["client_id"],
                Scopes = ["read", "create", "update"],
                Claims =
                [
                    new ClaimData { Type = "name", Value = "" },
                    new ClaimData { Type = "email", Value = "" }
                ],
                OpenIdProvider = configurationUrl
            }
        };

        var result = await _authorizationPolicy.Execute(
                ticket,
                "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                Array.Empty<Claim>(),
                CancellationToken.None,
                authorizationPolicy)
            ;

        Assert.Equal(AuthorizationPolicyResultKind.NeedInfo, result.Result);
    }

    [Fact]
    public async Task When_Role_Is_Not_Correct_Then_NotAuthorized_Is_Returned()
    {
        const string configurationUrl = "http://localhost/configuration";
        var ticket = new TicketLineParameter("client_id", ["read", "create", "update"]);

        var authorizationPolicy = new[]
        {
            new PolicyRule
            {
                ClientIdsAllowed = ["client_id"],
                Scopes = ["read", "create", "update"],
                Claims =
                [
                    new ClaimData { Type = "role", Value = "role1" },
                    new ClaimData { Type = "role", Value = "role2" }
                ],
                OpenIdProvider = configurationUrl
            }
        };

        var result = await _authorizationPolicy.Execute(
                ticket,
                "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                Array.Empty<Claim>(),
                CancellationToken.None,
                authorizationPolicy)
            ;

        Assert.Equal(AuthorizationPolicyResultKind.NeedInfo, result.Result);
    }

    [Fact]
    public async Task When_There_Is_No_Role_Then_NotAuthorized_Is_Returned()
    {
        const string configurationUrl = "http://localhost/configuration";
        var ticket = new TicketLineParameter("client_id", ["read", "create", "update"]);

        var authorizationPolicy = new[]
        {
            new PolicyRule
            {
                ClientIdsAllowed = ["client_id"],
                Scopes = ["read", "create", "update"],
                Claims =
                [
                    new ClaimData { Type = "role", Value = "role1" },
                    new ClaimData { Type = "role", Value = "role2" }
                ],
                OpenIdProvider = configurationUrl
            }
        };

        var result = await _authorizationPolicy.Execute(
                ticket,
                "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                Array.Empty<Claim>(),
                CancellationToken.None,
                authorizationPolicy)
            ;

        Assert.Equal(AuthorizationPolicyResultKind.NeedInfo, result.Result);
    }

    [Fact]
    public async Task When_Passing_Not_Valid_Roles_In_JArray_Then_NotAuthorized_Is_Returned()
    {
        const string configurationUrl = "http://localhost/configuration";
        var ticket = new TicketLineParameter("client_id", ["read", "create", "update"]);

        var authorizationPolicy = new[]
        {
            new PolicyRule
            {
                ClientIdsAllowed = ["client_id"],
                Scopes = ["read", "create", "update"],
                Claims =
                [
                    new ClaimData { Type = "role", Value = "role1" },
                    new ClaimData { Type = "role", Value = "role2" }
                ],
                OpenIdProvider = configurationUrl
            }
        };

        var result = await _authorizationPolicy.Execute(
                ticket,
                "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                Array.Empty<Claim>(),
                CancellationToken.None,
                authorizationPolicy)
            ;

        Assert.Equal(AuthorizationPolicyResultKind.NeedInfo, result.Result);
    }

    [Fact]
    public async Task When_Passing_Not_Valid_Roles_InStringArray_Then_NotAuthorized_Is_Returned()
    {
        const string configurationUrl = "http://localhost/configuration";
        var ticket = new TicketLineParameter("client_id", ["read", "create", "update"]);

        var authorizationPolicy = new[]
        {
            new PolicyRule
            {
                ClientIdsAllowed = ["client_id"],
                Scopes = ["read", "create", "update"],
                Claims =
                [
                    new ClaimData { Type = "role", Value = "role1" },
                    new ClaimData { Type = "role", Value = "role2" }
                ],
                OpenIdProvider = configurationUrl
            }
        };

        var result = await _authorizationPolicy.Execute(
                ticket,
                "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                Array.Empty<Claim>(),
                CancellationToken.None,
                authorizationPolicy)
            ;

        Assert.Equal(AuthorizationPolicyResultKind.NeedInfo, result.Result);
    }

    [Fact]
    public async Task When_Claims_Are_Not_Correct_Then_NotAuthorized_Is_Returned()
    {
        const string configurationUrl = "http://localhost/configuration";
        var ticket = new TicketLineParameter("client_id", ["read", "create", "update"]);

        var authorizationPolicy = new[]
        {
            new PolicyRule
            {
                ClientIdsAllowed = ["client_id"],
                Scopes = ["read", "create", "update"],
                Claims =
                [
                    new ClaimData { Type = "name", Value = "name" },
                    new ClaimData { Type = "email", Value = "email" }
                ],
                OpenIdProvider = configurationUrl
            }
        };

        var result = await _authorizationPolicy.Execute(
                ticket,
                "http://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken",
                Array.Empty<Claim>(),
                CancellationToken.None,
                authorizationPolicy)
            ;

        Assert.Equal(AuthorizationPolicyResultKind.NeedInfo, result.Result);
    }

    [Fact]
    public async Task When_ResourceOwnerConsent_Is_Required_Then_RequestSubmitted_Is_Returned()
    {
        var ticket = new TicketLineParameter("client_id", ["read", "create", "update"], false);

        var authorizationPolicy = new[]
        {
            new PolicyRule
            {
                ClientIdsAllowed = ["client_id"],
                IsResourceOwnerConsentNeeded = true,
                Scopes = ["read", "create", "update"]
            }
        };

        var result = await _authorizationPolicy.Execute(
                ticket,
                UmaConstants.IdTokenType,
                Array.Empty<Claim>(),
                CancellationToken.None,
                authorizationPolicy)
            ;

        Assert.Equal(AuthorizationPolicyResultKind.RequestSubmitted, result.Result);
    }

    [Fact]
    public async Task When_AuthorizationPassed_Then_Authorization_Is_Returned()
    {
        var ticket = new TicketLineParameter("client_id", ["create"], true);

        var authorizationPolicy = new[]
        {
            new PolicyRule
            {
                ClientIdsAllowed = ["client_id"],
                IsResourceOwnerConsentNeeded = true,
                Scopes = ["create"]
            }
        };

        var result = await _authorizationPolicy.Execute(
                ticket,
                UmaConstants.IdTokenType,
                Array.Empty<Claim>(),
                CancellationToken.None,
                authorizationPolicy)
            ;

        Assert.Equal(AuthorizationPolicyResultKind.Authorized, result.Result);
    }
}
