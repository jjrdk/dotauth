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

namespace SimpleAuth.AcceptanceTests;

using System.Net.Http;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using SimpleAuth;
using SimpleAuth.Extensions;

public sealed class SharedContext
{
    private static SharedContext ctx = null!;

    private SharedContext()
    {
        using var rsa = new RSACryptoServiceProvider(2048);
        SignatureKey = rsa.CreateSignatureJwk("1", true);
        ModelSignatureKey = rsa.CreateSignatureJwk("2", true);
        EncryptionKey = rsa.CreateEncryptionJwk("3", true);
        ModelEncryptionKey = rsa.CreateEncryptionJwk("4", true);
    }

    public static SharedContext Instance => ctx ??= new SharedContext();

    public JsonWebKey EncryptionKey { get; }
    public JsonWebKey ModelEncryptionKey { get; }
    public JsonWebKey SignatureKey { get; }
    public JsonWebKey ModelSignatureKey { get; }
    public HttpClient Client { get; set; }
    public HttpMessageHandler Handler { get; set; }
}