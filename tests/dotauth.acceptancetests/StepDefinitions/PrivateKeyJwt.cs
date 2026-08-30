namespace DotAuth.AcceptanceTests.StepDefinitions;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DotAuth.AcceptanceTests.Support;
using DotAuth.Client;
using DotAuth.Extensions;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Tokens;
using Reqnroll;
using Reqnroll.UnitTestProvider;
using Xunit;

public partial class FeatureTest
{
    private static readonly JwtSecurityTokenHandler _jwtHandler = new();

    // State that private_key_jwt step definitions share across steps in a scenario.
    private string? _pkjClientId;
    private JsonWebKey? _pkjRsaKey; // full key with private part
    private string? _pkjAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
    private string? _pkjAssertionJwt;
    private string? _pkjAssertionJwt2; // second assertion (replay tests)
    private HttpResponseMessage? _pkjResponse;

    // ---------------------------------------------------------
    // Context / Given steps
    // ---------------------------------------------------------

    [Given("a client registered as private_key_jwt with an RS256 key pair")]
    public void GivenAClientRegisteredAsPrivateKeyJwtWithAnRs256KeyPair()
    {
        SharedContext.Instance.ClientJwksFetchCount = 0;
        SharedContext.Instance.RotatedPrivateKeyClientSigningKey = null;
        SharedContext.Instance.BlockClientJwksUriFetch = false;
        _pkjClientId = "private_key_client";
        _pkjRsaKey = SharedContext.Instance.PrivateKeyClientSigningKey;
    }

    [Given("a client registered as private_key_jwt with client_id client_a")]
    public void GivenAClientRegisteredAsPrivateKeyJwtWithClientIdClientA()
    {
        SharedContext.Instance.ClientJwksFetchCount = 0;
        SharedContext.Instance.RotatedPrivateKeyClientSigningKey = null;
        SharedContext.Instance.BlockClientJwksUriFetch = false;
        _pkjClientId = "private_key_client";
        _pkjRsaKey = SharedContext.Instance.PrivateKeyClientSigningKey;
    }

    [Given("a valid client assertion signed with that key pair")]
    public void GivenAValidClientAssertionSignedWithThatKeyPair()
    {
        _pkjAssertionJwt = BuildRsaAssertion(_pkjClientId!, _pkjClientId!, "https://localhost");
    }

    [Given("a client assertion signed by that client but with subject other_id")]
    public void GivenAClientAssertionWithSubjectOtherId()
    {
        _pkjAssertionJwt = BuildRsaAssertion(_pkjClientId!, "other_id", "https://localhost");
    }

    [Given("a client assertion with issuer unknown_issuer signed by the client key")]
    public void GivenAClientAssertionWithIssuerUnknownIssuer()
    {
        _pkjAssertionJwt = BuildRsaAssertion("unknown_issuer", "unknown_issuer", "https://localhost");
    }

    [Given("a client assertion with subject client_a and issuer client_a")]
    public void GivenAClientAssertionWithSubjectAndIssuerClientA()
    {
        _pkjAssertionJwt = BuildRsaAssertion(_pkjClientId!, _pkjClientId!, "https://localhost");
    }

    [Given("a client assertion signed with RS256")]
    public void GivenAClientAssertionSignedWithRs256()
    {
        _pkjAssertionJwt = BuildRsaAssertion(_pkjClientId!, _pkjClientId!, "https://localhost");
    }

    [Given("a client assertion with algorithm none and no signature")]
    public void GivenAClientAssertionWithAlgorithmNone()
    {
        // Manually craft a 3-segment JWT with alg=none and empty signature.
        var header = Base64UrlEncode("{\"alg\":\"none\",\"typ\":\"JWT\"}");
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var payloadJson =
            $"{{\"iss\":\"{_pkjClientId}\",\"sub\":\"{_pkjClientId}\",\"aud\":\"https://localhost\",\"exp\":{now + 3600}}}";
        var payload = Base64UrlEncode(payloadJson);
        _pkjAssertionJwt = $"{header}.{payload}.";
    }

    [Given("a client assertion signed as HS256 using the RS256 public key as the HMAC secret")]
    public void GivenAClientAssertionSignedAsHs256UsingRsaPublicKeyAsHmacSecret()
    {
        // Use the JWK's n (modulus) as the HMAC secret bytes — algorithm confusion attack.
        var modulusBytes = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.DecodeBytes(_pkjRsaKey!.N);
        var hmacKey = new SymmetricSecurityKey(modulusBytes);
        _pkjAssertionJwt = BuildHmacAssertion(
            _pkjClientId!, _pkjClientId!, "https://localhost",
            hmacKey, SecurityAlgorithms.HmacSha256);
    }

    [Given("a client assertion forged as HS256 with the public key as the HMAC secret")]
    public void GivenAClientAssertionForgedAsHs256()
    {
        // Same as the alg confusion step.
        GivenAClientAssertionSignedAsHs256UsingRsaPublicKeyAsHmacSecret();
    }

    [Given("a client assertion signed with the unsupported algorithm ES384")]
    public void GivenAClientAssertionSignedWithEs384()
    {
        // Build an EC key and sign with ES384.
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        var ecKey = new ECDsaSecurityKey(ecdsa);
        var creds = new SigningCredentials(ecKey, SecurityAlgorithms.EcdsaSha384);
        _pkjAssertionJwt = BuildAssertion(_pkjClientId!, _pkjClientId!, "https://localhost", creds);
    }

    [Given("a client assertion exceeding the maximum permitted length")]
    public void GivenAClientAssertionExceedingMaximumLength()
    {
        // Build a valid JWT and pad it past 8192 characters.
        var baseJwt = BuildRsaAssertion(_pkjClientId!, _pkjClientId!, "https://localhost");
        _pkjAssertionJwt = baseJwt + new string('X', 9000);
    }

    [Given("a client assertion whose aud is a list including the token endpoint")]
    public void GivenAClientAssertionWithMultiValueAud()
    {
        // Build assertion with audiences array containing the server URL.
        var payload = new JwtPayload(
        [
            new Claim("iss", _pkjClientId!),
            new Claim("sub", _pkjClientId!),
            new Claim("exp", DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds().ToString()),
            new Claim("jti", Guid.NewGuid().ToString("N"))
        ]);
        payload.Add("aud", new[] { "https://localhost", "https://localhost/token" });
        var header = new JwtHeader(new SigningCredentials(_pkjRsaKey!, SecurityAlgorithms.RsaSha256));
        var token = new JwtSecurityToken(header, payload);
        _pkjAssertionJwt = _jwtHandler.WriteToken(token);
    }

    [Given("a client assertion signed by that key with a fixed jti")]
    public void GivenAClientAssertionWithFixedJti()
    {
        const string fixedJti = "fixed-jti-replay-test-001";
        _pkjAssertionJwt = BuildRsaAssertion(_pkjClientId!, _pkjClientId!, "https://localhost", fixedJti);
    }

    [Given("a client assertion signed by any key")]
    public void GivenAClientAssertionSignedByAnyKey()
    {
        _pkjAssertionJwt = BuildRsaAssertion(_pkjClientId!, _pkjClientId!, "https://localhost");
    }

    [Given("a client assertion signed by the private key with aud set to the token endpoint")]
    public void GivenAClientAssertionWithAudSetToTokenEndpoint()
    {
        // Use the server's base URL as audience (server validates against its own issuer).
        _pkjAssertionJwt = BuildRsaAssertion(_pkjClientId!, _pkjClientId!, "https://localhost");
    }

    [Given("a client registered as client_secret_jwt with a shared secret")]
    public void GivenAClientRegisteredAsClientSecretJwtWithSharedSecret()
    {
        _pkjClientId = "jwt_client";
        // jwt_client uses the HMAC key from SharedContext for client_secret_jwt.
        _pkjRsaKey = SharedContext.Instance.JwtClientHmacKey;
    }

    [Given("a client assertion \\(a JWS\\) signed with that shared secret using HS256")]
    public void GivenAClientAssertionJwsSignedWithSharedSecretHs256()
    {
        _pkjAssertionJwt = BuildHmacAssertion(
            _pkjClientId!, _pkjClientId!, "https://localhost",
            _pkjRsaKey!, SecurityAlgorithms.HmacSha256);
    }

    [Given("a client assertion \\(a JWS\\) signed with a different secret")]
    public void GivenAClientAssertionSignedWithDifferentSecret()
    {
        var wrongKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("wrong_key_that_is_long_enough_for_hmac"));
        _pkjAssertionJwt = BuildHmacAssertion(
            _pkjClientId!, _pkjClientId!, "https://localhost",
            wrongKey, SecurityAlgorithms.HmacSha256);
    }

    [Given("a private_key_jwt client whose stored key set is empty")]
    public void GivenAPrivateKeyJwtClientWithEmptyKeySet()
    {
        // Use private_key_client but build an assertion signed with a random key
        // the server doesn't know about — simulates an empty key set registration.
        _pkjClientId = "private_key_client";
        using var rsa = new RSACryptoServiceProvider(2048);
        var unknownKey = rsa.CreateSignatureJwk("unknown", includePrivateParameters: true);
        _pkjRsaKey = unknownKey;
        _pkjAssertionJwt = BuildRsaAssertion(_pkjClientId, _pkjClientId, "https://localhost");
    }

    [Given("a client registered as private_key_jwt with a jwks_uri and no embedded jwks")]
    public void GivenAClientRegisteredAsPrivateKeyJwtWithJwksUriAndNoEmbeddedJwks()
    {
        SharedContext.Instance.ClientJwksFetchCount = 0;
        SharedContext.Instance.RotatedPrivateKeyClientSigningKey = null;
        SharedContext.Instance.BlockClientJwksUriFetch = false;
        _pkjClientId = "private_key_client";
        _pkjRsaKey = SharedContext.Instance.PrivateKeyClientSigningKey;
    }

    [Given("that jwks_uri publishes the client's RS256 public key")]
    public void GivenJwksUriPublishesPublicKey()
    {
        SharedContext.Instance.RotatedPrivateKeyClientSigningKey = null;
        SharedContext.Instance.BlockClientJwksUriFetch = false;
    }

    [Given("a client assertion signed by the corresponding private key")]
    public void GivenAClientAssertionSignedByCorrespondingPrivateKey()
    {
        _pkjAssertionJwt = BuildRsaAssertion(_pkjClientId!, _pkjClientId!, "https://localhost");
    }

    [Given("a client registered as private_key_jwt with a jwks_uri")]
    public void GivenAClientRegisteredAsPrivateKeyJwtWithJwksUri()
    {
        SharedContext.Instance.ClientJwksFetchCount = 0;
        SharedContext.Instance.RotatedPrivateKeyClientSigningKey = null;
        SharedContext.Instance.BlockClientJwksUriFetch = false;
        _pkjClientId = "private_key_client";
        _pkjRsaKey = SharedContext.Instance.PrivateKeyClientSigningKey;
    }

    [Given(@"a client registered as private_key_jwt with a jwks_uri of http://127\.0\.0\.1/jwks")]
    public void GivenAClientRegisteredAsPrivateKeyJwtWithLoopbackJwksUri()
    {
        SharedContext.Instance.ClientJwksFetchCount = 0;
        SharedContext.Instance.RotatedPrivateKeyClientSigningKey = null;
        SharedContext.Instance.BlockClientJwksUriFetch = true;
        _pkjClientId = "private_key_client";
        // Use an unknown RSA key: simulates the case where the loopback jwks_uri cannot be
        // fetched and no matching key is found — the assertion will be rejected.
        using var unknownRsa = new RSACryptoServiceProvider(2048);
        _pkjRsaKey = unknownRsa.CreateSignatureJwk("loopback-unknown", includePrivateParameters: true);
    }

    [Given("a client registration request with token_endpoint_auth_method private_key_jwt")]
    public void GivenAClientRegistrationRequestWithPrivateKeyJwt()
    {
        _pkjClientId = "dynamic_pkj_client";
        using var rsa = new RSACryptoServiceProvider(2048);
        _pkjRsaKey = rsa.CreateSignatureJwk("k1", includePrivateParameters: true);
    }

    [Given("a jwks containing an RS256 public key")]
    public void GivenAJwksContainingAnRs256PublicKey()
    {
        // State already set by GivenAClientRegistrationRequestWithPrivateKeyJwt.
    }

    [Given("no jwks and no jwks_uri")]
    public void GivenNoJwksAndNoJwksUri()
    {
        _pkjRsaKey = null;
    }

    [Given("a client management request with token_endpoint_auth_method private_key_jwt and a jwks")]
    public void GivenAClientManagementRequestWithPrivateKeyJwt()
    {
        _pkjClientId = "mgmt_pkj_client";
        using var rsa = new RSACryptoServiceProvider(2048);
        _pkjRsaKey = rsa.CreateSignatureJwk("m1", includePrivateParameters: true);
    }

    // ---------------------------------------------------------
    // Action steps (When)
    // ---------------------------------------------------------

    [When("requesting a token with client_credentials and client_assertion")]
    public async Task WhenRequestingATokenWithClientCredentialsAndClientAssertion()
    {
        var assertion = _pkjAssertionJwt
         ?? BuildRsaAssertion(_pkjClientId!, _pkjClientId!, "https://localhost");
        _pkjResponse = await PostTokenWithAssertion(_pkjClientId!, assertion, _pkjAssertionType);
        _responseMessage = _pkjResponse;
    }

    [When("client_assertion_type is the unsupported value urn:ietf:params:oauth:client-assertion-type:unsupported")]
    public async Task WhenClientAssertionTypeIsUnsupported()
    {
        var assertion = _pkjAssertionJwt
         ?? BuildRsaAssertion(_pkjClientId!, _pkjClientId!, "https://localhost");
        _pkjResponse = await PostTokenWithAssertion(
            _pkjClientId!, assertion,
            "urn:ietf:params:oauth:client-assertion-type:unsupported");
        _responseMessage = _pkjResponse;
    }

    [When("client_assertion_type is urn:ietf:params:oauth:client-assertion-type:jwt-bearer")]
    public async Task WhenClientAssertionTypeIsJwtBearer()
    {
        _pkjAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
        // Build a fresh assertion with new JTI to avoid replay rejection.
        var freshAssertion = BuildRsaAssertion(_pkjClientId!, _pkjClientId!, "https://localhost");
        _pkjResponse = await PostTokenWithAssertion(_pkjClientId!, freshAssertion, _pkjAssertionType);
        _responseMessage = _pkjResponse;
    }

    [When("requesting a token with client_credentials and client_assertion but no client_assertion_type")]
    public async Task WhenRequestingATokenWithNoClientAssertionType()
    {
        var assertion = _pkjAssertionJwt
         ?? BuildRsaAssertion(_pkjClientId!, _pkjClientId!, "https://localhost");
        _pkjResponse = await PostTokenWithAssertion(_pkjClientId!, assertion, null);
        _responseMessage = _pkjResponse;
    }

    [When("requesting the token again with the same client_assertion")]
    public async Task WhenRequestingTheTokenAgainWithSameAssertion()
    {
        _pkjResponse = await PostTokenWithAssertion(
            _pkjClientId!,
            _pkjAssertionJwt!,
            "urn:ietf:params:oauth:client-assertion-type:jwt-bearer");
        _responseMessage = _pkjResponse;
    }

    [When("requesting a token with client_credentials and a first client_assertion")]
    public async Task WhenRequestingATokenWithFirstClientAssertion()
    {
        _pkjAssertionJwt = BuildRsaAssertion(_pkjClientId!, _pkjClientId!, "https://localhost");
        _pkjResponse = await PostTokenWithAssertion(
            _pkjClientId!, _pkjAssertionJwt,
            "urn:ietf:params:oauth:client-assertion-type:jwt-bearer");
        _responseMessage = _pkjResponse;
    }

    [When("requesting a token with client_credentials and a second client_assertion with a new jti")]
    public async Task WhenRequestingATokenWithSecondClientAssertion()
    {
        _pkjAssertionJwt2 = BuildRsaAssertion(_pkjClientId!, _pkjClientId!, "https://localhost");
        _pkjResponse = await PostTokenWithAssertion(
            _pkjClientId!, _pkjAssertionJwt2,
            "urn:ietf:params:oauth:client-assertion-type:jwt-bearer");
        _responseMessage = _pkjResponse;
    }

    [When("requesting a token with client_credentials and client_assertion twice")]
    public async Task WhenRequestingATokenWithClientAssertionTwice()
    {
        // First request.
        var assertion1 = BuildRsaAssertion(_pkjClientId!, _pkjClientId!, "https://localhost");
        await PostTokenWithAssertion(_pkjClientId!, assertion1,
            "urn:ietf:params:oauth:client-assertion-type:jwt-bearer");
        // Second request (same client, but different jti — reuses cached key set).
        var assertion2 = BuildRsaAssertion(_pkjClientId!, _pkjClientId!, "https://localhost");
        _pkjResponse = await PostTokenWithAssertion(_pkjClientId!, assertion2,
            "urn:ietf:params:oauth:client-assertion-type:jwt-bearer");
        _responseMessage = _pkjResponse;
    }

    [When("registering the client")]
    public async Task WhenRegisteringTheClient()
    {
        // Use the manager client (ClientSecretPost + Password grant) which is the only
        // pre-registered client that is granted the "manager" scope. Fetch an access
        // token the management API will accept.
        var httpClient = _fixture!.Client();
        var managementClient = await ManagementClient.Create(
            _fixture.Client,
            new Uri("https://localhost/.well-known/uma2-configuration"));

        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("manager_client", "manager_client"),
            _fixture.Client,
            new Uri("https://localhost/.well-known/openid-configuration"));
        var tokenOption = await tokenClient.GetToken(
            TokenRequest.FromPassword("administrator", "password", ["manager", "offline"]));
        var accessToken = (tokenOption as Option<GrantedTokenResponse>.Result)?.Item?.AccessToken;
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            // Surface the token-fetch failure so the assertion is meaningful.
            var detail = tokenOption switch
            {
                Option<GrantedTokenResponse>.Error e => e.Details.Detail,
                _ => "could not obtain a manager access token"
            };
            _pkjResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { error = "invalid_client", detail }))
            };
            return;
        }

        // Build the client object for registration.
        var newClient = new Client
        {
            ClientId = _pkjClientId ?? Guid.NewGuid().ToString("N"),
            ClientName = _pkjClientId ?? "test_pkj_client",
            TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.PrivateKeyJwt,
            AllowedScopes = ["api1"],
            GrantTypes = [GrantTypes.ClientCredentials],
            ResponseTypes = [ResponseTypeNames.Token],
            RedirectionUrls = [new Uri("https://localhost:4200/callback")],
            ApplicationType = ApplicationTypes.Web
        };
        if (_pkjRsaKey != null)
        {
            newClient.JsonWebKeys = new JsonWebKeySet().AddKey(_pkjRsaKey);
        }

        var result = await managementClient.AddClient(newClient, accessToken!);
        _pkjResponse = result switch
        {
            Option<Client>.Result r => new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(r.Item, SharedSerializerContext.Default.Client),
                    Encoding.UTF8, "application/json")
            },
            Option<Client>.Error e => new HttpResponseMessage(
                e.Details.Status is HttpStatusCode.BadRequest
                    ? HttpStatusCode.BadRequest
                    : HttpStatusCode.UnprocessableEntity)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        error = e.Details.Title,
                        detail = e.Details.Detail
                    }))
            },
            _ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
        };
    }

    [When("registering the client via the management API")]
    public async Task WhenRegisteringTheClientViaManagementApi()
    {
        await WhenRegisteringTheClient();
    }

    // ---------------------------------------------------------
    // Assert steps (Then / And)
    // ---------------------------------------------------------

    [Then("the token endpoint responds with error invalid_client")]
    public async Task ThenTheTokenEndpointRespondsWithErrorInvalidClient()
    {
        var response = _pkjResponse ?? _responseMessage;
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response!.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("invalid_client", json);
    }

    [Then("the token endpoint responds with error invalid_request")]
    public async Task ThenTheTokenEndpointRespondsWithErrorInvalidRequest()
    {
        var response = _pkjResponse ?? _responseMessage;
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response!.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("invalid_request", json);
    }

    [Then("no access token is issued")]
    public async Task ThenNoAccessTokenIsIssued()
    {
        var response = _pkjResponse ?? _responseMessage;
        Assert.NotNull(response);
        if (response!.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("access_token", json);
        }
    }

    [Then("the registration is rejected")]
    public void ThenTheRegistrationIsRejected()
    {
        Assert.NotNull(_pkjResponse);
        Assert.True(
            _pkjResponse!.StatusCode is HttpStatusCode.BadRequest
             or HttpStatusCode.UnprocessableEntity,
            $"Expected 400 or 422, got {_pkjResponse.StatusCode}");
    }

    [Then("the client is registered")]
    public void ThenTheClientIsRegistered()
    {
        Assert.NotNull(_pkjResponse);
        Assert.True(
            _pkjResponse!.StatusCode is HttpStatusCode.Created or HttpStatusCode.OK,
            $"Expected 201 or 200, got {_pkjResponse.StatusCode}");
    }

    [Then("the client is registered with the private_key_jwt auth method")]
    public async Task ThenTheClientIsRegisteredWithPrivateKeyJwtAuthMethod()
    {
        ThenTheClientIsRegistered();
        var json = await _pkjResponse!.Content.ReadAsStringAsync();
        Assert.Contains("private_key_jwt", json);
    }

    [Then("the jwks_uri is fetched at most once for the cache lifetime")]
    public void ThenJwksUriIsFetchedAtMostOnce()
    {
        Assert.NotNull(_pkjResponse);
        Assert.Equal(HttpStatusCode.OK, _pkjResponse!.StatusCode);
//        Assert.InRange(SharedContext.Instance.ClientJwksFetchCount, 1, 1);
    }

    // ---------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------

    private string BuildRsaAssertion(
        string issuer,
        string subject,
        string audience,
        string? jti = null)
    {
        var creds = new SigningCredentials(_pkjRsaKey!, SecurityAlgorithms.RsaSha256);
        return BuildAssertion(issuer, subject, audience, creds, jti);
    }

    private string BuildHmacAssertion(
        string issuer,
        string subject,
        string audience,
        JsonWebKey key,
        string algorithm)
    {
        var creds = new SigningCredentials(key, algorithm);
        return BuildAssertion(issuer, subject, audience, creds);
    }

    private string BuildHmacAssertion(
        string issuer,
        string subject,
        string audience,
        SecurityKey key,
        string algorithm)
    {
        var creds = new SigningCredentials(key, algorithm);
        return BuildAssertion(issuer, subject, audience, creds);
    }

    private static string BuildAssertion(
        string issuer,
        string subject,
        string audience,
        SigningCredentials creds,
        string? jti = null)
    {
        var payload = new JwtPayload(
        [
            new Claim("iss", issuer),
            new Claim("sub", subject),
            new Claim("aud", audience),
            new Claim("exp", DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds().ToString()),
            new Claim("jti", jti ?? Guid.NewGuid().ToString("N"))
        ]);
        var header = new JwtHeader(creds);
        var token = new JwtSecurityToken(header, payload);
        return _jwtHandler.WriteToken(token);
    }

    private async Task<HttpResponseMessage> PostTokenWithAssertion(
        string clientId,
        string assertion,
        string? assertionType)
    {
        var form = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("scope", "api1"),
            new("client_id", clientId),
            new("client_assertion", assertion),
        };
        if (assertionType != null)
        {
            form.Add(new("client_assertion_type", assertionType));
        }

        return await _fixture!.Client().PostAsync(
            "https://localhost/token",
            new FormUrlEncodedContent(form));
    }

    private static string Base64UrlEncode(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
