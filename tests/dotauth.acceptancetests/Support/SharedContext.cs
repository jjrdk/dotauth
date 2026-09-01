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

namespace DotAuth.AcceptanceTests.Support;

using System;
using System.Net.Http;
using System.Security.Cryptography;
using DotAuth.Extensions;
using Microsoft.IdentityModel.Tokens;

public sealed class SharedContext
{
    private static readonly SharedContext ctx = new();

    private SharedContext()
    {
        using var rsa = new RSACryptoServiceProvider(2048);
        SignatureKey = rsa.CreateSignatureJwk("1", true);
        ModelSignatureKey = rsa.CreateSignatureJwk("2", true);
        EncryptionKey = rsa.CreateEncryptionJwk("3", true);
        ModelEncryptionKey = rsa.CreateEncryptionJwk("4", true);
        // Dedicated RS256 key pair for private_key_client acceptance tests.
        using var clientRsa = new RSACryptoServiceProvider(2048);
        PrivateKeyClientSigningKey = clientRsa.CreateSignatureJwk("pkc", true);
        // HMAC key for jwt_client (client_secret_jwt) acceptance tests.
        JwtClientHmacKey = "jwt_client_secret_long_enough_key".CreateSignatureJwk();
    }

    public static SharedContext Instance
    {
        get { return ctx; }
    }

    public JsonWebKey EncryptionKey { get; }
    public JsonWebKey ModelEncryptionKey { get; }
    public JsonWebKey SignatureKey { get; }
    public JsonWebKey ModelSignatureKey { get; }
    /// <summary>RS256 key pair (includes private) for private_key_client acceptance tests.</summary>
    public JsonWebKey PrivateKeyClientSigningKey { get; }
    /// <summary>HMAC key for jwt_client (client_secret_jwt) acceptance tests.</summary>
    public JsonWebKey JwtClientHmacKey { get; }
    public HttpClient? Client { get; set; }
    public Uri? ClientJwksUri { get; set; }
    public int ClientJwksFetchCount { get; set; }
    public JsonWebKey? RotatedPrivateKeyClientSigningKey { get; set; }
    public bool BlockClientJwksUriFetch { get; set; }
    //    public HttpMessageHandler? Handler { get; set; }
}
