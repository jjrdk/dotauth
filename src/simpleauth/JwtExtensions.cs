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
    using Converter;
    using Encrypt;
    using Encrypt.Encryption;
    using Microsoft.Extensions.DependencyInjection;
    using Signature;

    public static class JwtExtensions
    {
        public static IServiceCollection AddSimpleAuthJwt(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IJweGenerator, JweGenerator>();
            serviceCollection.AddTransient<IJweParser, JweParser>();
            serviceCollection.AddTransient<IAesEncryptionHelper, AesEncryptionHelper>();
            serviceCollection.AddTransient<IJweHelper, JweHelper>();
            serviceCollection.AddTransient<IJwsGenerator, JwsGenerator>();
            serviceCollection.AddTransient<ICreateJwsSignature, CreateJwsSignature>();
            serviceCollection.AddTransient<IJwsParser, JwsParser>();
            serviceCollection.AddTransient<IJsonWebKeyConverter, JsonWebKeyConverter>();
            return serviceCollection;
        }
    }
}
