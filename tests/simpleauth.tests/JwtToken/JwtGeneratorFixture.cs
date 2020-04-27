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

namespace SimpleAuth.Tests.JwtToken
{
    using Exceptions;
    using Fake;
    using Helpers;
    using Microsoft.IdentityModel.Tokens;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth;
    using SimpleAuth.JwtToken;
    using SimpleAuth.Shared.Errors;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;

    using SimpleAuth.Repositories;
    using Xunit;

    public class JwtGeneratorFixture
    {
        private JwtGenerator _jwtGenerator;
        private Mock<IClientStore> _clientRepositoryStub;
        private Mock<IScopeRepository> _scopeRepositoryStub;

        public JwtGeneratorFixture()
        {
            InitializeMockObjects();
        }

        [Fact]
        public async Task When_Passing_Null_Parameters_To_GenerateAccessToken_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<NullReferenceException>(
                    () => _jwtGenerator.GenerateAccessToken(null, null, null, default, null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Empty_Parameters_To_GenerateAccessToken_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<NullReferenceException>(
                    () => _jwtGenerator.GenerateAccessToken(null, null, null, default, null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task WhenPassingEmptyClientThenThrows()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                    () => _jwtGenerator.GenerateAccessToken(new Client(), null, null, default, null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task WhenSignatureParametersAreConfigedThenCanGenerateAccessToken()
        {
            const string clientId = "client_id";
            var scopes = new List<string> {"openid", "role"};
            var client = new Client
            {
                IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                ClientId = clientId
            };

            var result = await _jwtGenerator.GenerateAccessToken(client, scopes, "issuer", default, null)
                .ConfigureAwait(false);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task When_Passing_Null_Parameters_To_GenerateIdTokenPayloadForScopes_Then_Exception_Is_Thrown()
        {
            var authorizationParameter = new AuthorizationParameter();

            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _jwtGenerator.GenerateIdTokenPayloadForScopes(null, null, null, CancellationToken.None))
                .ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _jwtGenerator.GenerateIdTokenPayloadForScopes(
                        null,
                        authorizationParameter,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task
            When_Requesting_IdentityToken_JwsPayload_And_IndicateTheMaxAge_Then_TheJwsPayload_Contains_AuthenticationTime()
        {
            const string subject = "john.doe@email.com";
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString()),
                new Claim(OpenIdClaimTypes.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter {ClientId = "test", MaxAge = 2};
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients().First());
            _clientRepositoryStub.Setup(c => c.GetAll(It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients());

            var result = await _jwtGenerator.GenerateIdTokenPayloadForScopes(
                    claimsPrincipal,
                    authorizationParameter,
                    null,
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Contains(result.Claims, c => c.Type == OpenIdClaimTypes.Subject);
            Assert.Contains(result.Claims, c => c.Type == StandardClaimNames.AuthenticationTime);
            Assert.Equal(subject, result.Claims.First(c => c.Type == OpenIdClaimTypes.Subject).Value);
            Assert.NotEmpty(result.Claims.First(c => c.Type == StandardClaimNames.AuthenticationTime).Value);
        }

        [Fact]
        public async Task
            When_Requesting_IdentityToken_JwsPayload_And_NumberOfAudiencesIsMoreThanOne_Then_Azp_Should_Be_Returned()
        {
            const string issuerName = "IssuerName";
            var clientId = FakeOpenIdAssets.GetClients().First().ClientId;
            const string subject = "john.doe@email.com";
            var claims = new List<Claim> {new Claim(OpenIdClaimTypes.Subject, subject)};
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter {ClientId = clientId};
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients().First());
            _clientRepositoryStub.Setup(c => c.GetAll(It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients());

            var result = await _jwtGenerator.GenerateIdTokenPayloadForScopes(
                    claimsPrincipal,
                    authorizationParameter,
                    issuerName,
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Contains(result.Claims, c => c.Type == OpenIdClaimTypes.Subject);
            Assert.True(result.Aud.Count > 1);
            Assert.Equal(clientId, result.Azp);
        }

        [Fact]
        public async Task When_Requesting_IdentityToken_JwsPayload_And_ThereNoClient_Then_Azp_Should_Be_Returned()
        {
            const string issuerName = "IssuerName";
            const string clientId = "clientId";
            const string subject = "john.doe@email.com";
            var claims = new List<Claim> {new Claim(OpenIdClaimTypes.Subject, subject)};
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter {ClientId = clientId};
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client) null);
            _clientRepositoryStub.Setup(c => c.GetAll(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<Client>());

            var result = await _jwtGenerator.GenerateIdTokenPayloadForScopes(
                    claimsPrincipal,
                    authorizationParameter,
                    issuerName,
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Contains(result.Claims, c => c.Type == OpenIdClaimTypes.Subject);
            Assert.Single(result.Aud);
            Assert.Equal(clientId, result.Azp);
        }

        [Fact]
        public async Task When_Requesting_IdentityToken_JwsPayload_And_Multiple_Scopes_With_Same_Claim_Defined_Then_Role_Should_Be_Returned_Without_Duplicates()
        {
            const string issuerName = "IssuerName";
            const string clientId = "clientId";
            const string subject = "john.doe@email.com";
            const string role = "administrator";
            const string scope = "role";
            var claims = new List<Claim> { new Claim(OpenIdClaimTypes.Subject, subject), new Claim(OpenIdClaimTypes.Role, role) };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter { ClientId = clientId, Scope = scope };
            this._scopeRepositoryStub.Setup(sr => sr.SearchByNames(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
                .ReturnsAsync(
                    new[]
                        {
                            new Scope { Type = "role", Claims = new[] { OpenIdClaimTypes.Role } },
                            new Scope { Type = "manager", Claims = new[] { OpenIdClaimTypes.Role } }
                        }.ToArray());

            var result = await _jwtGenerator.GenerateIdTokenPayloadForScopes(
                             claimsPrincipal,
                             authorizationParameter,
                             issuerName,
                             CancellationToken.None).ConfigureAwait(false);

            Assert.Contains(result.Claims, c => c.Type == OpenIdClaimTypes.Role);
            Assert.Equal("administrator", result.Claims.Single(c => c.Type.Equals(OpenIdClaimTypes.Role)).Value);
        }

        [Fact]
        public async Task When_Requesting_IdentityToken_JwsPayload_And_Multiple_Scopes_With_Multiple_Unique_Claims_Defined_Then_Role_Should_Be_Returned_With_All_Unique_Claims()
        {
            const string issuerName = "IssuerName";
            const string clientId = "clientId";
            const string subject = "john.doe@email.com";
            const string role = "administrator";
            const string anotherRole = "superadministrator";
            const string scope = "role";
            var claims = new List<Claim>
                             {
                                 new Claim(OpenIdClaimTypes.Subject, subject),
                                 new Claim(OpenIdClaimTypes.Role, role),
                                 new Claim(OpenIdClaimTypes.Role, anotherRole)
                             };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter { ClientId = clientId, Scope = scope };
            this._scopeRepositoryStub.Setup(sr => sr.SearchByNames(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
                .ReturnsAsync(
                    new[]
                        {
                            new Scope { Type = "role", Claims = new[] { OpenIdClaimTypes.Role } },
                            new Scope { Type = "manager", Claims = new[] { OpenIdClaimTypes.Role } }
                        }.ToArray());

            var result = await _jwtGenerator.GenerateIdTokenPayloadForScopes(
                             claimsPrincipal,
                             authorizationParameter,
                             issuerName,
                             CancellationToken.None).ConfigureAwait(false);

            Assert.Contains(result.Claims, c => c.Type == OpenIdClaimTypes.Role);
            Assert.Equal("administrator superadministrator", result.Claims.Single(c => c.Type.Equals(OpenIdClaimTypes.Role)).Value);
        }

        [Fact]
        public async Task
            When_Requesting_IdentityToken_JwsPayload_With_No_Authorization_Request_Then_MandatoriesClaims_Are_Returned()
        {
            const string subject = "john.doe@email.com";
            var authorizationParameter = new AuthorizationParameter();
            var claims = new List<Claim> {new Claim(OpenIdClaimTypes.Subject, subject)};
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients().First());
            _clientRepositoryStub.Setup(c => c.GetAll(It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients());

            var result = await _jwtGenerator.GenerateIdTokenPayloadForScopes(
                    claimsPrincipal,
                    authorizationParameter,
                    null,
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Contains(result.Claims, c => c.Type == OpenIdClaimTypes.Subject);
            Assert.Contains(result.Claims, c => c.Type == StandardClaimNames.Audiences);
            Assert.Contains(result.Claims, c => c.Type == StandardClaimNames.ExpirationTime);
            Assert.Contains(result.Claims, c => c.Type == StandardClaimNames.Iat);
            Assert.Equal(subject, result.Sub);
        }

        [Fact]
        public async Task When_Passing_Null_Parameters_To_GenerateFilteredIdTokenPayload_Then_Exception_Is_Thrown()
        {
            var authorizationParameter = new AuthorizationParameter();

            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _jwtGenerator.GenerateFilteredIdTokenPayload(null, null, null, null, CancellationToken.None))
                .ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _jwtGenerator.GenerateFilteredIdTokenPayload(
                        null,
                        authorizationParameter,
                        null,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Requesting_Identity_Token_And_Audiences_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            const string subject = "john.doe@email.com";
            const string state = "state";
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString()),
                new Claim(OpenIdClaimTypes.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter {State = state};
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = StandardClaimNames.Audiences,
                    Parameters = new Dictionary<string, object>
                    {
                        {CoreConstants.StandardClaimParameterValueNames.EssentialName, true},
                        {CoreConstants.StandardClaimParameterValueNames.ValuesName, new[] {"audience"}}
                    }
                }
            };
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients().First());
            _clientRepositoryStub.Setup(c => c.GetAll(It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients());

            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _jwtGenerator.GenerateFilteredIdTokenPayload(
                        claimsPrincipal,
                        authorizationParameter,
                        claimsParameter,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InvalidGrant, exception.Code);
            Assert.Equal(
                string.Format(ErrorMessages.TheClaimIsNotValid, StandardClaimNames.Audiences),
                exception.Message);
            Assert.Equal(state, exception.State);
        }

        [Fact]
        public async Task When_Requesting_Identity_Token_And_Issuer_Claim_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            const string subject = "john.doe@email.com";
            const string state = "state";
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString()),
                new Claim(OpenIdClaimTypes.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter {State = state};
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = StandardClaimNames.Issuer,
                    Parameters = new Dictionary<string, object>
                    {
                        {CoreConstants.StandardClaimParameterValueNames.EssentialName, true},
                        {CoreConstants.StandardClaimParameterValueNames.ValueName, "issuer"}
                    }
                }
            };
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients().First());
            _clientRepositoryStub.Setup(c => c.GetAll(It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients());

            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _jwtGenerator.GenerateFilteredIdTokenPayload(
                        claimsPrincipal,
                        authorizationParameter,
                        claimsParameter,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InvalidGrant, exception.Code);
            Assert.Equal(
                exception.Message,
                string.Format(ErrorMessages.TheClaimIsNotValid, StandardClaimNames.Issuer));
            Assert.Equal(state, exception.State);
        }

        [Fact]
        public async Task When_Requesting_Identity_Token_And_ExpirationTime_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            const string subject = "john.doe@email.com";
            const string state = "state";
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString()),
                new Claim(OpenIdClaimTypes.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter {State = state};
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = StandardClaimNames.ExpirationTime,
                    Parameters = new Dictionary<string, object>
                    {
                        {CoreConstants.StandardClaimParameterValueNames.EssentialName, true},
                        {CoreConstants.StandardClaimParameterValueNames.ValueName, 12}
                    }
                }
            };
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients().First());
            _clientRepositoryStub.Setup(c => c.GetAll(It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients());

            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _jwtGenerator.GenerateFilteredIdTokenPayload(
                        claimsPrincipal,
                        authorizationParameter,
                        claimsParameter,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InvalidGrant, exception.Code);
            Assert.Equal(
                string.Format(ErrorMessages.TheClaimIsNotValid, StandardClaimNames.ExpirationTime),
                exception.Message);
            Assert.Equal(state, exception.State);
        }

        [Fact]
        public async Task
            When_Requesting_IdentityToken_JwsPayload_And_PassingANotValidClaimValue_Then_An_Exception_Is_Thrown()
        {
            const string subject = "john.doe@email.com";
            const string notValidSubject = "jane.doe@email.com";
            var claims = new List<Claim> {new Claim(OpenIdClaimTypes.Subject, subject)};
            var authorizationParameter = new AuthorizationParameter();

            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = OpenIdClaimTypes.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {CoreConstants.StandardClaimParameterValueNames.EssentialName, true},
                        {CoreConstants.StandardClaimParameterValueNames.ValueName, notValidSubject}
                    }
                }
            };
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients().First());
            _clientRepositoryStub.Setup(c => c.GetAll(It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients());

            var result = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _jwtGenerator.GenerateFilteredIdTokenPayload(
                        claimsPrincipal,
                        authorizationParameter,
                        claimsParameter,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);

            Assert.Equal(result.Code, ErrorCodes.InvalidGrant);
            Assert.Equal(result.Message, string.Format(ErrorMessages.TheClaimIsNotValid, OpenIdClaimTypes.Subject));
        }

        [Fact]
        public async Task
            When_Requesting_IdentityToken_JwsPayload_And_Pass_AuthTime_As_ClaimEssential_Then_TheJwsPayload_Contains_AuthenticationTime()
        {
            const string subject = "john.doe@email.com";
            const string nonce = "nonce";
            var currentDateTimeOffset = (double) DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.AuthenticationInstant,
                    currentDateTimeOffset.ToString(CultureInfo.InvariantCulture)),
                new Claim(OpenIdClaimTypes.Subject, subject),
                new Claim(OpenIdClaimTypes.Role, "['role1', 'role2']", ClaimValueTypes.String)
            };
            var authorizationParameter = new AuthorizationParameter {Nonce = nonce};
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = StandardClaimNames.AuthenticationTime,
                    Parameters = new Dictionary<string, object>
                    {
                        {CoreConstants.StandardClaimParameterValueNames.EssentialName, true}
                    }
                },
                new ClaimParameter
                {
                    Name = StandardClaimNames.Audiences,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            CoreConstants.StandardClaimParameterValueNames.ValuesName,
                            new[] {FakeOpenIdAssets.GetClients().First().ClientId}
                        }
                    }
                },
                new ClaimParameter
                {
                    Name = StandardClaimNames.Nonce,
                    Parameters = new Dictionary<string, object>
                    {
                        {CoreConstants.StandardClaimParameterValueNames.ValueName, nonce}
                    }
                },
                new ClaimParameter
                {
                    Name = OpenIdClaimTypes.Role,
                    Parameters = new Dictionary<string, object>
                    {
                        {CoreConstants.StandardClaimParameterValueNames.EssentialName, true}
                    }
                }
            };
            _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients().First());
            _clientRepositoryStub.Setup(c => c.GetAll(It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients());

            var result = await _jwtGenerator.GenerateFilteredIdTokenPayload(
                    claimsPrincipal,
                    authorizationParameter,
                    claimsParameter,
                    null,
                    CancellationToken.None)
                .ConfigureAwait(false);


            Assert.Equal(subject, result.Claims.First(c => c.Type == OpenIdClaimTypes.Subject).Value);
            Assert.Equal(
                currentDateTimeOffset,
                double.Parse(result.Claims.First(c => c.Type == StandardClaimNames.AuthenticationTime).Value));

            Assert.Contains(result.Claims, c => c.Type == OpenIdClaimTypes.Role);
            Assert.Contains(result.Claims, c => c.Type == StandardClaimNames.AuthenticationTime);
            Assert.Contains(result.Claims, c => c.Type == StandardClaimNames.Nonce);
        }

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exception_Is_Thrown()
        {
            var authorizationParameter = new AuthorizationParameter();

            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _jwtGenerator.GenerateUserInfoPayloadForScope(null, null, CancellationToken.None))
                .ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _jwtGenerator.GenerateUserInfoPayloadForScope(
                        null,
                        authorizationParameter,
                        CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Requesting_UserInformation_JwsPayload_For_Scopes_Then_The_JwsPayload_Is_Correct()
        {
            const string subject = "john.doe@email.com";
            const string name = "John Doe";
            var claims = new List<Claim>
            {
                new Claim(OpenIdClaimTypes.Name, name), new Claim(OpenIdClaimTypes.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter {Scope = "profile"};
            var scopes = FakeOpenIdAssets.GetScopes().Where(s => s.Name == "profile").ToArray();
            _clientRepositoryStub.Setup(c => c.GetAll(It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients());
            _scopeRepositoryStub.Setup(s => s.SearchByNames(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
                .ReturnsAsync(scopes);

            var result = await _jwtGenerator.GenerateUserInfoPayloadForScope(
                    claimsPrincipal,
                    authorizationParameter,
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Equal(subject, result.Claims.First(c => c.Type == OpenIdClaimTypes.Subject).Value);
            Assert.Equal(name, result.Claims.First(c => c.Type == OpenIdClaimTypes.Name).Value);
        }

        [Fact]
        public void When_Passing_Null_Parameters_To_GenerateFilteredUserInfoPayload_Then_Exception_Is_Thrown()
        {
            var authorizationParameter = new AuthorizationParameter();

            Assert.Throws<ArgumentNullException>(() => JwtGenerator.GenerateFilteredUserInfoPayload(null, null, null));
            Assert.Throws<ArgumentNullException>(
                () => JwtGenerator.GenerateFilteredUserInfoPayload(null, null, authorizationParameter));
        }

        [Fact]
        public void When_Requesting_UserInformation_But_The_Essential_Claim_Subject_Is_Empty_Then_Exception_Is_Thrown()
        {
            const string subject = "";
            const string state = "state";
            var claims = new List<Claim> {new Claim(OpenIdClaimTypes.Subject, subject)};
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = OpenIdClaimTypes.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {CoreConstants.StandardClaimParameterValueNames.EssentialName, true}
                    }
                }
            };

            var authorizationParameter = new AuthorizationParameter {Scope = "profile", State = state};
            var scopes = FakeOpenIdAssets.GetScopes().Where(s => s.Name == "profile").ToArray();
            _clientRepositoryStub.Setup(c => c.GetAll(It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients());
            _scopeRepositoryStub.Setup(s => s.SearchByNames(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
                .ReturnsAsync(scopes);

            var exception = Assert.Throws<SimpleAuthExceptionWithState>(
                () => JwtGenerator.GenerateFilteredUserInfoPayload(
                    claimsParameter,
                    claimsPrincipal,
                    authorizationParameter));
            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InvalidGrant, exception.Code);
            Assert.Equal(
                string.Format(ErrorMessages.TheClaimIsNotValid, OpenIdClaimTypes.Subject),
                exception.Message);
            Assert.Equal(state, exception.State);
        }

        [Fact]
        public void
            When_Requesting_UserInformation_But_The_Subject_Claim_Value_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            const string subject = "invalid@loki.be";
            const string state = "state";
            var claims = new List<Claim> {new Claim(OpenIdClaimTypes.Subject, subject)};
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = OpenIdClaimTypes.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {CoreConstants.StandardClaimParameterValueNames.ValueName, "john.doe@email.com"}
                    }
                }
            };

            var authorizationParameter = new AuthorizationParameter {Scope = "profile", State = state};
            var scopes = FakeOpenIdAssets.GetScopes().Where(s => s.Name == "profile").ToArray();
            _clientRepositoryStub.Setup(c => c.GetAll(It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients());
            _scopeRepositoryStub.Setup(s => s.SearchByNames(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
                .ReturnsAsync(scopes);

            var exception = Assert.Throws<SimpleAuthExceptionWithState>(
                () => JwtGenerator.GenerateFilteredUserInfoPayload(
                    claimsParameter,
                    claimsPrincipal,
                    authorizationParameter));

            Assert.Equal(ErrorCodes.InvalidGrant, exception.Code);
            Assert.True(
                exception.Message == string.Format(ErrorMessages.TheClaimIsNotValid, OpenIdClaimTypes.Subject));
            Assert.True(exception.State == state);
        }

        [Fact]
        public void When_Requesting_UserInformation_But_The_Essential_Claim_Name_Is_Empty_Then_Exception_Is_Thrown()
        {
            const string subject = "john.doe@email.com";
            const string state = "state";
            var claims = new List<Claim> {new Claim(OpenIdClaimTypes.Subject, subject)};
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = OpenIdClaimTypes.Name,
                    Parameters = new Dictionary<string, object>
                    {
                        {CoreConstants.StandardClaimParameterValueNames.EssentialName, true}
                    }
                }
            };

            var authorizationParameter = new AuthorizationParameter {Scope = "profile", State = state};
            var scopes = FakeOpenIdAssets.GetScopes().Where(s => s.Name == "profile").ToArray();
            _clientRepositoryStub.Setup(c => c.GetAll(It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients());
            _scopeRepositoryStub.Setup(s => s.SearchByNames(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
                .ReturnsAsync(scopes);

            var exception = Assert.Throws<SimpleAuthExceptionWithState>(
                () => JwtGenerator.GenerateFilteredUserInfoPayload(
                    claimsParameter,
                    claimsPrincipal,
                    authorizationParameter));
            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InvalidGrant, exception.Code);
            Assert.True(
                exception.Message == string.Format(ErrorMessages.TheClaimIsNotValid, OpenIdClaimTypes.Name));
            Assert.True(exception.State == state);
        }

        [Fact]
        public void When_Requesting_UserInformation_But_The_Name_Claim_Value_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            const string subject = "john.doe@email.com";
            const string state = "state";
            var claims = new List<Claim>
            {
                new Claim(OpenIdClaimTypes.Subject, subject), new Claim(OpenIdClaimTypes.Name, "invalid_name")
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = OpenIdClaimTypes.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {CoreConstants.StandardClaimParameterValueNames.ValueName, subject}
                    }
                },
                new ClaimParameter
                {
                    Name = OpenIdClaimTypes.Name,
                    Parameters = new Dictionary<string, object>
                    {
                        {CoreConstants.StandardClaimParameterValueNames.ValueName, "name"}
                    }
                }
            };

            var authorizationParameter = new AuthorizationParameter {Scope = "profile", State = state};
            var scopes = FakeOpenIdAssets.GetScopes().Where(s => s.Name == "profile").ToArray();
            _clientRepositoryStub.Setup(c => c.GetAll(It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients());
            _scopeRepositoryStub.Setup(s => s.SearchByNames(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
                .ReturnsAsync(scopes);

            var exception = Assert.Throws<SimpleAuthExceptionWithState>(
                () => JwtGenerator.GenerateFilteredUserInfoPayload(
                    claimsParameter,
                    claimsPrincipal,
                    authorizationParameter));

            Assert.Equal(ErrorCodes.InvalidGrant, exception.Code);
            Assert.True(
                exception.Message == string.Format(ErrorMessages.TheClaimIsNotValid, OpenIdClaimTypes.Name));
            Assert.True(exception.State == state);
        }

        [Fact]
        public void When_Requesting_UserInformation_For_Some_Valid_Claims_Then_The_JwsPayload_Is_Correct()
        {
            const string subject = "john.doe@email.com";
            const string name = "John Doe";
            var claims = new List<Claim>
            {
                new Claim(OpenIdClaimTypes.Name, name), new Claim(OpenIdClaimTypes.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = OpenIdClaimTypes.Name,
                    Parameters = new Dictionary<string, object>
                    {
                        {CoreConstants.StandardClaimParameterValueNames.EssentialName, true}
                    }
                },
                new ClaimParameter
                {
                    Name = OpenIdClaimTypes.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {CoreConstants.StandardClaimParameterValueNames.EssentialName, true},
                        {CoreConstants.StandardClaimParameterValueNames.ValueName, subject}
                    }
                }
            };

            var authorizationParameter = new AuthorizationParameter {Scope = "profile"};
            var scopes = FakeOpenIdAssets.GetScopes().Where(s => s.Name == "profile").ToArray();
            _clientRepositoryStub.Setup(c => c.GetAll(It.IsAny<CancellationToken>()))
                .ReturnsAsync(FakeOpenIdAssets.GetClients());
            _scopeRepositoryStub.Setup(s => s.SearchByNames(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
                .ReturnsAsync(scopes);

            var result = JwtGenerator.GenerateFilteredUserInfoPayload(
                claimsParameter,
                claimsPrincipal,
                authorizationParameter);

            Assert.Equal(subject, result.Claims.First(c => c.Type == OpenIdClaimTypes.Subject).Value);
            Assert.Equal(name, result.Claims.First(c => c.Type == OpenIdClaimTypes.Name).Value);
        }

        [Fact]
        public void When_Passing_Null_Parameters_To_FillInOtherClaimsIdentityTokenPayload_Then_Exceptions_Are_Thrown()
        {
            Assert.Throws<NullReferenceException>(
                () => _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(null, null, null, null));
        }

        [Fact]
        public void When_Passing_Empty_Parameters_To_FillInOtherClaimsIdentityTokenPayload_Then_Exceptions_Are_Thrown()
        {
            Assert.Throws<NullReferenceException>(
                () => _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(new JwtPayload(), null, null, null));
        }

        [Fact]
        public void When_JwsAlg_Is_None_And_Trying_To_FillIn_Other_Claims_Then_The_Properties_Are_Not_Filled_In()
        {
            var client = FakeOpenIdAssets.GetClients().First();
            client.IdTokenSignedResponseAlg = "none";
            var jwsPayload = new JwtPayload();

            _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(jwsPayload, null, null, new Client());

            Assert.DoesNotContain(jwsPayload.Claims, c => c.Type == StandardClaimNames.AtHash);
            Assert.DoesNotContain(jwsPayload.Claims, c => c.Type == StandardClaimNames.CHash);
        }

        [Fact]
        public void
            When_JwsAlg_Is_RS256_And_AuthorizationCode_And_AccessToken_Are_Not_Empty_Then_OtherClaims_Are_FilledIn()
        {
            var client = FakeOpenIdAssets.GetClients().First();
            client.IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256;
            var jwsPayload = new JwtPayload();

            _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(
                jwsPayload,
                "authorization_code",
                "access_token",
                client);
            Assert.Contains(jwsPayload.Claims, c => c.Type == StandardClaimNames.AtHash);
            Assert.Contains(jwsPayload.Claims, c => c.Type == StandardClaimNames.CHash);
        }

        [Fact]
        public void
            When_JwsAlg_Is_RS384_And_AuthorizationCode_And_AccessToken_Are_Not_Empty_Then_OtherClaims_Are_FilledIn()
        {
            var client = FakeOpenIdAssets.GetClients().First();
            client.IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha384;
            var jwsPayload = new JwtPayload();

            _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(
                jwsPayload,
                "authorization_code",
                "access_token",
                client);
            Assert.Contains(jwsPayload.Claims, c => c.Type == StandardClaimNames.AtHash);
            Assert.Contains(jwsPayload.Claims, c => c.Type == StandardClaimNames.CHash);
        }

        [Fact]
        public void
            When_JwsAlg_Is_RS512_And_AuthorizationCode_And_AccessToken_Are_Not_Empty_Then_OtherClaims_Are_FilledIn()
        {
            var client = FakeOpenIdAssets.GetClients().First();
            client.IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha512;
            var jwsPayload = new JwtPayload();

            _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(
                jwsPayload,
                "authorization_code",
                "access_token",
                client);
            Assert.Contains(jwsPayload.Claims, c => c.Type == StandardClaimNames.AtHash);
            Assert.Contains(jwsPayload.Claims, c => c.Type == StandardClaimNames.CHash);
        }

        private void InitializeMockObjects()
        {
            _clientRepositoryStub = new Mock<IClientStore>();
            _scopeRepositoryStub = new Mock<IScopeRepository>();

            _jwtGenerator = new JwtGenerator(
                _clientRepositoryStub.Object,
                _scopeRepositoryStub.Object,
                new InMemoryJwksRepository());
        }
    }
}
