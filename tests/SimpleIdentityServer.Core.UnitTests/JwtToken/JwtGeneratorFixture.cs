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

using Moq;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.UnitTests.Fake;
using SimpleIdentityServer.Core.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.JwtToken
{
    using SimpleAuth.Jwt;
    using SimpleAuth.Jwt.Encrypt;
    using SimpleAuth.Jwt.Encrypt.Encryption;
    using SimpleAuth.Jwt.Signature;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using Client = SimpleAuth.Shared.Models.Client;

    public class JwtGeneratorFixture
    {
        private IJwtGenerator _jwtGenerator;
        private Mock<IClientStore> _clientRepositoryStub;
        private Mock<IJsonWebKeyRepository> _jsonWebKeyRepositoryStub;
        private Mock<IScopeRepository> _scopeRepositoryStub;

        [Fact]
        public async Task When_Passing_Null_Parameters_To_GenerateAccessToken_Then_Exception_Is_Thrown()
        {
            InitializeMockObjects();

            await Assert
                .ThrowsAsync<ArgumentNullException>(() => _jwtGenerator.GenerateAccessToken(null, null, null, null))
                .ConfigureAwait(false);
            await Assert
                .ThrowsAsync<ArgumentNullException>(() =>
                    _jwtGenerator.GenerateAccessToken(new Client(), null, null, null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Generate_AccessToken()
        {
            const string clientId = "client_id";
            var scopes = new List<string> { "openid", "role" };
            InitializeMockObjects();
            var client = new Client
            {
                ClientId = clientId
            };

            var result = await _jwtGenerator.GenerateAccessToken(client, scopes, null, null).ConfigureAwait(false);

            // ASSERTS.
            Assert.NotNull(result);
        }


        [Fact]
        public async Task When_Passing_Null_Parameters_To_GenerateIdTokenPayloadForScopes_Then_Exception_Is_Thrown()
        {
            InitializeMockObjects();
            var authorizationParameter = new AuthorizationParameter();

            await Assert
                .ThrowsAsync<ArgumentNullException>(() =>
                    _jwtGenerator.GenerateIdTokenPayloadForScopesAsync(null, null, null))
                .ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                    _jwtGenerator.GenerateIdTokenPayloadForScopesAsync(null, authorizationParameter, null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task
            When_Requesting_IdentityToken_JwsPayload_And_IndicateTheMaxAge_Then_TheJwsPayload_Contains_AuthenticationTime()
        {
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString()),
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                MaxAge = 2
            };
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));

            var result = await _jwtGenerator.GenerateIdTokenPayloadForScopesAsync(
                    claimsPrincipal,
                    authorizationParameter,
                    null)
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.HasClaim(JwtConstants.StandardResourceOwnerClaimNames.Subject));
            Assert.True(result.HasClaim(StandardClaimNames.AuthenticationTime));
            Assert.Equal(subject, result.GetStringClaim(JwtConstants.StandardResourceOwnerClaimNames.Subject));
            Assert.NotEmpty(result.GetStringClaim(StandardClaimNames.AuthenticationTime));
        }

        [Fact]
        public async Task
            When_Requesting_IdentityToken_JwsPayload_And_NumberOfAudiencesIsMoreThanOne_Then_Azp_Should_Be_Returned()
        {
            InitializeMockObjects();
            const string issuerName = "IssuerName";
            var clientId = FakeOpenIdAssets.GetClients().First().ClientId;
            const string subject = "habarthierry@hotmail.fr";
            var claims = new List<Claim>
            {
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId
            };
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));

            var result = await _jwtGenerator.GenerateIdTokenPayloadForScopesAsync(
                    claimsPrincipal,
                    authorizationParameter,
                    issuerName)
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.HasClaim(JwtConstants.StandardResourceOwnerClaimNames.Subject));
            Assert.True(result.Audiences.Length > 1);
            Assert.True(result.Azp == clientId);
        }

        [Fact]
        public async Task When_Requesting_IdentityToken_JwsPayload_And_ThereNoClient_Then_Azp_Should_Be_Returned()
        {
            InitializeMockObjects();
            const string issuerName = "IssuerName";
            const string clientId = "clientId";
            const string subject = "habarthierry@hotmail.fr";
            var claims = new List<Claim>
            {
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId
            };
            _clientRepositoryStub.Setup(c => c.GetAllAsync())
                .Returns(Task.FromResult((IEnumerable<Client>)new List<Client>()));

            var result = await _jwtGenerator.GenerateIdTokenPayloadForScopesAsync(
                    claimsPrincipal,
                    authorizationParameter,
                    issuerName)
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.HasClaim(JwtConstants.StandardResourceOwnerClaimNames.Subject));
            Assert.True(result.Audiences.Count() == 1);
            Assert.True(result.Azp == clientId);
        }

        [Fact]
        public async Task
            When_Requesting_IdentityToken_JwsPayload_With_No_Authorization_Request_Then_MandatoriesClaims_Are_Returned()
        {
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            var authorizationParameter = new AuthorizationParameter();
            var claims = new List<Claim>
            {
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));

            var result = await _jwtGenerator.GenerateIdTokenPayloadForScopesAsync(
                    claimsPrincipal,
                    authorizationParameter,
                    null)
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.HasClaim(JwtConstants.StandardResourceOwnerClaimNames.Subject));
            Assert.True(result.HasClaim(StandardClaimNames.Audiences));
            Assert.True(result.HasClaim(StandardClaimNames.ExpirationTime));
            Assert.True(result.HasClaim(StandardClaimNames.Iat));
            Assert.True(result.GetStringClaim(JwtConstants.StandardResourceOwnerClaimNames.Subject) == subject);
        }

        [Fact]
        public async Task When_Passing_Null_Parameters_To_GenerateFilteredIdTokenPayload_Then_Exception_Is_Thrown()
        {
            InitializeMockObjects();
            var authorizationParameter = new AuthorizationParameter();

            await Assert
                .ThrowsAsync<ArgumentNullException>(() =>
                    _jwtGenerator.GenerateFilteredIdTokenPayloadAsync(null, null, null, null))
                .ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                    _jwtGenerator.GenerateFilteredIdTokenPayloadAsync(null, authorizationParameter, null, null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Requesting_Identity_Token_And_Audiences_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string state = "state";
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString()),
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                State = state
            };
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = StandardClaimNames.Audiences,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            CoreConstants.StandardClaimParameterValueNames.EssentialName,
                            true
                        },
                        {
                            CoreConstants.StandardClaimParameterValueNames.ValuesName,
                            new[] {"audience"}
                        }
                    }
                }
            };
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));

            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() =>
                    _jwtGenerator.GenerateFilteredIdTokenPayloadAsync(
                        claimsPrincipal,
                        authorizationParameter,
                        claimsParameter,
                        null))
                .ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message ==
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, StandardClaimNames.Audiences));
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task When_Requesting_Identity_Token_And_Issuer_Claim_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string state = "state";
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString()),
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                State = state
            };
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = StandardClaimNames.Issuer,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            CoreConstants.StandardClaimParameterValueNames.EssentialName,
                            true
                        },
                        {
                            CoreConstants.StandardClaimParameterValueNames.ValueName,
                            "issuer"
                        }
                    }
                }
            };
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));

            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() =>
                    _jwtGenerator.GenerateFilteredIdTokenPayloadAsync(
                        claimsPrincipal,
                        authorizationParameter,
                        claimsParameter,
                        null))
                .ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message ==
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, StandardClaimNames.Issuer));
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task When_Requesting_Identity_Token_And_ExpirationTime_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string state = "state";
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString()),
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                State = state
            };
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = StandardClaimNames.ExpirationTime,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            CoreConstants.StandardClaimParameterValueNames.EssentialName,
                            true
                        },
                        {
                            CoreConstants.StandardClaimParameterValueNames.ValueName,
                            12
                        }
                    }
                }
            };
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));

            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() =>
                    _jwtGenerator.GenerateFilteredIdTokenPayloadAsync(
                        claimsPrincipal,
                        authorizationParameter,
                        claimsParameter,
                        null))
                .ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message ==
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, StandardClaimNames.ExpirationTime));
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task
            When_Requesting_IdentityToken_JwsPayload_And_PassingANotValidClaimValue_Then_An_Exception_Is_Thrown()
        {
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string notValidSubject = "habarthierry@hotmail.be";
            var claims = new List<Claim>
            {
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var authorizationParameter = new AuthorizationParameter();

            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = JwtConstants.StandardResourceOwnerClaimNames.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            CoreConstants.StandardClaimParameterValueNames.EssentialName,
                            true
                        },
                        {
                            CoreConstants.StandardClaimParameterValueNames.ValueName,
                            notValidSubject
                        }
                    }
                }
            };
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));

            // ACT & ASSERTS
            var result = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() =>
                    _jwtGenerator.GenerateFilteredIdTokenPayloadAsync(
                        claimsPrincipal,
                        authorizationParameter,
                        claimsParameter,
                        null))
                .ConfigureAwait(false);

            Assert.Equal(result.Code, ErrorCodes.InvalidGrant);
            Assert.Equal(result.Message,
                string.Format(ErrorDescriptions.TheClaimIsNotValid,
                    JwtConstants.StandardResourceOwnerClaimNames.Subject));
        }

        [Fact]
        public async Task
            When_Requesting_IdentityToken_JwsPayload_And_Pass_AuthTime_As_ClaimEssential_Then_TheJwsPayload_Contains_AuthenticationTime()
        {
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string nonce = "nonce";
            var currentDateTimeOffset = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationInstant, currentDateTimeOffset.ToString()),
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject),
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Role,
                    "['role1', 'role2']",
                    ClaimValueTypes.String)
            };
            var authorizationParameter = new AuthorizationParameter
            {
                Nonce = nonce
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = StandardClaimNames.AuthenticationTime,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            CoreConstants.StandardClaimParameterValueNames.EssentialName,
                            true
                        }
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
                        {
                            CoreConstants.StandardClaimParameterValueNames.ValueName,
                            nonce
                        }
                    }
                },
                new ClaimParameter
                {
                    Name = JwtConstants.StandardResourceOwnerClaimNames.Role,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            CoreConstants.StandardClaimParameterValueNames.EssentialName,
                            true
                        }
                    }
                }
            };
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));

            var result = await _jwtGenerator.GenerateFilteredIdTokenPayloadAsync(
                    claimsPrincipal,
                    authorizationParameter,
                    claimsParameter,
                    null)
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.HasClaim(JwtConstants.StandardResourceOwnerClaimNames.Subject));
            Assert.True(result.HasClaim(JwtConstants.StandardResourceOwnerClaimNames.Role));
            Assert.True(result.HasClaim(StandardClaimNames.AuthenticationTime));
            Assert.True(result.HasClaim(StandardClaimNames.Nonce));
            Assert.Equal(subject, result.GetStringClaim(JwtConstants.StandardResourceOwnerClaimNames.Subject));
            Assert.True(long.Parse(result.GetStringClaim(StandardClaimNames.AuthenticationTime))
                .Equals(currentDateTimeOffset));
        }

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exception_Is_Thrown()
        {
            InitializeMockObjects();
            var authorizationParameter = new AuthorizationParameter();

            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _jwtGenerator.GenerateUserInfoPayloadForScopeAsync(null, null))
                .ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                    _jwtGenerator.GenerateUserInfoPayloadForScopeAsync(null, authorizationParameter))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Requesting_UserInformation_JwsPayload_For_Scopes_Then_The_JwsPayload_Is_Correct()
        {
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string name = "Habart Thierry";
            var claims = new List<Claim>
            {
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Name, name),
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                Scope = "profile"
            };
            ICollection<Scope> scopes = FakeOpenIdAssets.GetScopes().Where(s => s.Name == "profile").ToList();
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));
            _scopeRepositoryStub.Setup(s => s.SearchByNames(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));

            var result = await _jwtGenerator
                .GenerateUserInfoPayloadForScopeAsync(claimsPrincipal, authorizationParameter)
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.HasClaim(JwtConstants.StandardResourceOwnerClaimNames.Subject));
            Assert.True(result.HasClaim(JwtConstants.StandardResourceOwnerClaimNames.Name));
            Assert.Equal(result.GetStringClaim(JwtConstants.StandardResourceOwnerClaimNames.Subject), subject);
            Assert.Equal(result.GetStringClaim(JwtConstants.StandardResourceOwnerClaimNames.Name), name);
        }

        [Fact]
        public void When_Passing_Null_Parameters_To_GenerateFilteredUserInfoPayload_Then_Exception_Is_Thrown()
        {
            InitializeMockObjects();
            var authorizationParameter = new AuthorizationParameter();

            Assert.Throws<ArgumentNullException>(() => _jwtGenerator.GenerateFilteredUserInfoPayload(null, null, null));
            Assert.Throws<ArgumentNullException>(() =>
                _jwtGenerator.GenerateFilteredUserInfoPayload(null, null, authorizationParameter));
        }

        [Fact]
        public void When_Requesting_UserInformation_But_The_Essential_Claim_Subject_Is_Empty_Then_Exception_Is_Thrown()
        {
            InitializeMockObjects();
            const string subject = "";
            const string state = "state";
            var claims = new List<Claim>
            {
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = JwtConstants.StandardResourceOwnerClaimNames.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            CoreConstants.StandardClaimParameterValueNames.EssentialName,
                            true
                        }
                    }
                }
            };

            var authorizationParameter = new AuthorizationParameter
            {
                Scope = "profile",
                State = state
            };
            ICollection<Scope> scopes = FakeOpenIdAssets.GetScopes().Where(s => s.Name == "profile").ToList();
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));
            _scopeRepositoryStub.Setup(s => s.SearchByNames(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));

            var exception = Assert.Throws<IdentityServerExceptionWithState>(() =>
                _jwtGenerator.GenerateFilteredUserInfoPayload(
                    claimsParameter,
                    claimsPrincipal,
                    authorizationParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message ==
                        string.Format(ErrorDescriptions.TheClaimIsNotValid,
                            JwtConstants.StandardResourceOwnerClaimNames.Subject));
            Assert.True(exception.State == state);
        }

        [Fact]
        public void
            When_Requesting_UserInformation_But_The_Subject_Claim_Value_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            InitializeMockObjects();
            const string subject = "invalid@loki.be";
            const string state = "state";
            var claims = new List<Claim>
            {
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = JwtConstants.StandardResourceOwnerClaimNames.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            CoreConstants.StandardClaimParameterValueNames.ValueName,
                            "habarthierry@lokie.be"
                        }
                    }
                }
            };

            var authorizationParameter = new AuthorizationParameter
            {
                Scope = "profile",
                State = state
            };
            ICollection<Scope> scopes = FakeOpenIdAssets.GetScopes().Where(s => s.Name == "profile").ToList();
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));
            _scopeRepositoryStub.Setup(s => s.SearchByNames(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));

            var exception = Assert.Throws<IdentityServerExceptionWithState>(() =>
                _jwtGenerator.GenerateFilteredUserInfoPayload(
                    claimsParameter,
                    claimsPrincipal,
                    authorizationParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message ==
                        string.Format(ErrorDescriptions.TheClaimIsNotValid,
                            JwtConstants.StandardResourceOwnerClaimNames.Subject));
            Assert.True(exception.State == state);
        }

        [Fact]
        public void When_Requesting_UserInformation_But_The_Essential_Claim_Name_Is_Empty_Then_Exception_Is_Thrown()
        {
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string state = "state";
            var claims = new List<Claim>
            {
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = JwtConstants.StandardResourceOwnerClaimNames.Name,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            CoreConstants.StandardClaimParameterValueNames.EssentialName,
                            true
                        }
                    }
                }
            };

            var authorizationParameter = new AuthorizationParameter
            {
                Scope = "profile",
                State = state
            };
            ICollection<Scope> scopes = FakeOpenIdAssets.GetScopes().Where(s => s.Name == "profile").ToList();
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));
            _scopeRepositoryStub.Setup(s => s.SearchByNames(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));

            var exception = Assert.Throws<IdentityServerExceptionWithState>(
                () => _jwtGenerator.GenerateFilteredUserInfoPayload(
                    claimsParameter,
                    claimsPrincipal,
                    authorizationParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message ==
                        string.Format(ErrorDescriptions.TheClaimIsNotValid,
                            JwtConstants.StandardResourceOwnerClaimNames.Name));
            Assert.True(exception.State == state);
        }

        [Fact]
        public void When_Requesting_UserInformation_But_The_Name_Claim_Value_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            InitializeMockObjects();
            const string subject = "habarthierry@lokie.be";
            const string state = "state";
            var claims = new List<Claim>
            {
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject),
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Name, "invalid_name")
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = JwtConstants.StandardResourceOwnerClaimNames.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            CoreConstants.StandardClaimParameterValueNames.ValueName,
                            subject
                        }
                    }
                },
                new ClaimParameter
                {
                    Name = JwtConstants.StandardResourceOwnerClaimNames.Name,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            CoreConstants.StandardClaimParameterValueNames.ValueName,
                            "name"
                        }
                    }
                }
            };

            var authorizationParameter = new AuthorizationParameter
            {
                Scope = "profile",
                State = state
            };
            ICollection<Scope> scopes = FakeOpenIdAssets.GetScopes().Where(s => s.Name == "profile").ToList();
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));
            _scopeRepositoryStub.Setup(s => s.SearchByNames(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));

            var exception = Assert.Throws<IdentityServerExceptionWithState>(() =>
                _jwtGenerator.GenerateFilteredUserInfoPayload(
                    claimsParameter,
                    claimsPrincipal,
                    authorizationParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidGrant);
            Assert.True(exception.Message ==
                        string.Format(ErrorDescriptions.TheClaimIsNotValid,
                            JwtConstants.StandardResourceOwnerClaimNames.Name));
            Assert.True(exception.State == state);
        }

        [Fact]
        public void When_Requesting_UserInformation_For_Some_Valid_Claims_Then_The_JwsPayload_Is_Correct()
        {
            InitializeMockObjects();
            const string subject = "habarthierry@hotmail.fr";
            const string name = "Habart Thierry";
            var claims = new List<Claim>
            {
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Name, name),
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimIdentity = new ClaimsIdentity(claims, "fake");
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            var claimsParameter = new List<ClaimParameter>
            {
                new ClaimParameter
                {
                    Name = JwtConstants.StandardResourceOwnerClaimNames.Name,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            CoreConstants.StandardClaimParameterValueNames.EssentialName,
                            true
                        }
                    }
                },
                new ClaimParameter
                {
                    Name = JwtConstants.StandardResourceOwnerClaimNames.Subject,
                    Parameters = new Dictionary<string, object>
                    {
                        {
                            CoreConstants.StandardClaimParameterValueNames.EssentialName,
                            true
                        },
                        {
                            CoreConstants.StandardClaimParameterValueNames.ValueName,
                            subject
                        }
                    }
                }
            };

            var authorizationParameter = new AuthorizationParameter
            {
                Scope = "profile"
            };
            ICollection<Scope> scopes = FakeOpenIdAssets.GetScopes().Where(s => s.Name == "profile").ToList();
            _clientRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult(FakeOpenIdAssets.GetClients()));
            _scopeRepositoryStub.Setup(s => s.SearchByNames(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));

            var result = _jwtGenerator.GenerateFilteredUserInfoPayload(
                claimsParameter,
                claimsPrincipal,
                authorizationParameter);

            Assert.NotNull(result);
            Assert.True(result.HasClaim(JwtConstants.StandardResourceOwnerClaimNames.Subject));
            Assert.True(result.HasClaim(JwtConstants.StandardResourceOwnerClaimNames.Name));
            Assert.Equal(subject, result.GetStringClaim(JwtConstants.StandardResourceOwnerClaimNames.Subject));
            Assert.Equal(name, result.GetStringClaim(JwtConstants.StandardResourceOwnerClaimNames.Name));
        }

        [Fact]
        public void When_Passing_Null_Parameters_To_FillInOtherClaimsIdentityTokenPayload_Then_Exceptions_Are_Thrown()
        {
            InitializeMockObjects();

            Assert.Throws<ArgumentNullException>(() =>
                _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(null, null, null, null));
            Assert.Throws<ArgumentNullException>(() =>
                _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(new JwsPayload(), null, null, null));
        }

        [Fact]
        public void When_JwsAlg_Is_None_And_Trying_To_FillIn_Other_Claims_Then_The_Properties_Are_Not_Filled_In()
        {
            InitializeMockObjects();
            var client = FakeOpenIdAssets.GetClients().First();
            client.IdTokenSignedResponseAlg = "none";
            var jwsPayload = new JwsPayload();

            _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(jwsPayload, null, null, new Client());
            Assert.False(jwsPayload.HasClaim(StandardClaimNames.AtHash));
            Assert.False(jwsPayload.HasClaim(StandardClaimNames.CHash));
        }

        [Fact]
        public void
            When_JwsAlg_Is_RS256_And_AuthorizationCode_And_AccessToken_Are_Not_Empty_Then_OtherClaims_Are_FilledIn()
        {
            InitializeMockObjects();
            var client = FakeOpenIdAssets.GetClients().First();
            client.IdTokenSignedResponseAlg = JwtConstants.JwsAlgNames.RS256;
            var jwsPayload = new JwsPayload();

            _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(jwsPayload,
                "authorization_code",
                "access_token",
                client);
            Assert.True(jwsPayload.HasClaim(StandardClaimNames.AtHash));
            Assert.True(jwsPayload.HasClaim(StandardClaimNames.CHash));
        }

        [Fact]
        public void
            When_JwsAlg_Is_RS384_And_AuthorizationCode_And_AccessToken_Are_Not_Empty_Then_OtherClaims_Are_FilledIn()
        {
            InitializeMockObjects();
            var client = FakeOpenIdAssets.GetClients().First();
            client.IdTokenSignedResponseAlg = JwtConstants.JwsAlgNames.RS384;
            var jwsPayload = new JwsPayload();

            _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(jwsPayload,
                "authorization_code",
                "access_token",
                client);
            Assert.True(jwsPayload.HasClaim(StandardClaimNames.AtHash));
            Assert.True(jwsPayload.HasClaim(StandardClaimNames.CHash));
        }

        [Fact]
        public void
            When_JwsAlg_Is_RS512_And_AuthorizationCode_And_AccessToken_Are_Not_Empty_Then_OtherClaims_Are_FilledIn()
        {
            InitializeMockObjects();
            var client = FakeOpenIdAssets.GetClients().First();
            client.IdTokenSignedResponseAlg = JwtConstants.JwsAlgNames.RS512;
            var jwsPayload = new JwsPayload();

            _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(jwsPayload,
                "authorization_code",
                "access_token",
                client);
            Assert.True(jwsPayload.HasClaim(StandardClaimNames.AtHash));
            Assert.True(jwsPayload.HasClaim(StandardClaimNames.CHash));
        }

        [Fact]
        public async Task When_Encrypt_Jws_Then_Jwe_Is_Returned()
        {
            InitializeMockObjects();
            var client = FakeOpenIdAssets.GetClients().First();
            client.IdTokenEncryptedResponseAlg = JwtConstants.JweAlgNames.RSA1_5;
            var serializedRsa = string.Empty;
            using (var provider = new RSACryptoServiceProvider())
            {
                serializedRsa = RsaExtensions.ToXmlString(provider, true); //.ToXmlString(true);
            }

            ;

            var jsonWebKey = new JsonWebKey
            {
                Alg = AllAlg.RSA1_5,
                KeyOps = new[]
                {
                    KeyOperations.Encrypt,
                    KeyOperations.Decrypt
                },
                Kid = "3",
                Kty = KeyType.RSA,
                Use = Use.Enc,
                SerializedKey = serializedRsa,
            };
            var jws = "jws";
            ICollection<JsonWebKey> jwks = new List<JsonWebKey> { jsonWebKey };
            _jsonWebKeyRepositoryStub.Setup(j =>
                    j.GetByAlgorithmAsync(It.IsAny<Use>(), It.IsAny<AllAlg>(), It.IsAny<KeyOperations[]>()))
                .Returns(Task.FromResult(jwks));
            //_clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>())).Returns(Task.FromResult(client));

            var jwe = await _jwtGenerator.EncryptAsync(jws,
                    JweAlg.RSA1_5,
                    JweEnc.A128CBC_HS256)
                .ConfigureAwait(false);

            Assert.NotEmpty(jwe);
        }

        [Fact]
        public async Task When_Sign_Payload_Then_Jws_Is_Returned()
        {
            InitializeMockObjects();
            var client = FakeOpenIdAssets.GetClients().First();
            client.IdTokenEncryptedResponseAlg = JwtConstants.JwsAlgNames.RS256;
            var serializedRsa = string.Empty;
            using (var provider = new RSACryptoServiceProvider())
            {
                serializedRsa = RsaExtensions.ToXmlString(provider, true);
            }

            var jsonWebKey = new JsonWebKey
            {
                Alg = AllAlg.RS256,
                KeyOps = new[]
                {
                    KeyOperations.Sign,
                    KeyOperations.Verify
                },
                Kid = "a3rMUgMFv9tPclLa6yF3zAkfquE",
                Kty = KeyType.RSA,
                Use = Use.Sig,
                SerializedKey = serializedRsa
            };
            ICollection<JsonWebKey> jwks = new List<JsonWebKey> { jsonWebKey };
            _jsonWebKeyRepositoryStub.Setup(j =>
                    j.GetByAlgorithmAsync(It.IsAny<Use>(), It.IsAny<AllAlg>(), It.IsAny<KeyOperations[]>()))
                .Returns(Task.FromResult(jwks));
            //_clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>())).Returns(Task.FromResult(client));
            var jwsPayload = new JwsPayload();

            var jws = await _jwtGenerator.SignAsync(jwsPayload,
                    JwsAlg.RS256)
                .ConfigureAwait(false);

            Assert.NotEmpty(jws);
        }

        private void InitializeMockObjects()
        {
            _clientRepositoryStub = new Mock<IClientStore>();
            _jsonWebKeyRepositoryStub = new Mock<IJsonWebKeyRepository>();
            _scopeRepositoryStub = new Mock<IScopeRepository>();
            var clientValidator = new ClientValidator();
            var parameterParserHelper = new ParameterParserHelper();
            var createJwsSignature = new CreateJwsSignature();
            var aesEncryptionHelper = new AesEncryptionHelper();
            var jweHelper = new JweHelper(aesEncryptionHelper);
            var jwsGenerator = new JwsGenerator(createJwsSignature);
            var jweGenerator = new JweGenerator(jweHelper);

            _jwtGenerator = new JwtGenerator(
                new OAuthConfigurationOptions(
                    authorizationCodeValidity: TimeSpan.FromMinutes(60)),
                _clientRepositoryStub.Object,
                clientValidator,
                _jsonWebKeyRepositoryStub.Object,
                _scopeRepositoryStub.Object,
                parameterParserHelper,
                jwsGenerator,
                jweGenerator);
        }
    }
}
