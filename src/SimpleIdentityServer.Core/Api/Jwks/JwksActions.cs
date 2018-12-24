﻿// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

using System.Collections.Generic;
using SimpleIdentityServer.Core.Api.Jwks.Actions;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Jwks
{
    using Shared.Requests;

    public class JwksActions : IJwksActions
    {
        private readonly IGetSetOfPublicKeysUsedToValidateJwsAction _getSetOfPublicKeysUsedToValidateJwsAction;

        private readonly IGetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction
            _getSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction;
        private readonly IRotateJsonWebKeysOperation _rotateJsonWebKeysOperation;

        public JwksActions(
            IGetSetOfPublicKeysUsedToValidateJwsAction getSetOfPublicKeysUsedToValidateJwsAction,
            IGetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction getSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction,
            IRotateJsonWebKeysOperation rotateJsonWebKeysOperation)
        {
            _getSetOfPublicKeysUsedToValidateJwsAction = getSetOfPublicKeysUsedToValidateJwsAction;
            _getSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction =
                getSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction;
            _rotateJsonWebKeysOperation = rotateJsonWebKeysOperation;
        }

        public async Task<JsonWebKeySet> GetJwks()
        {
            var publicKeysUsedToValidateSignature = await _getSetOfPublicKeysUsedToValidateJwsAction.Execute().ConfigureAwait(false);
            var publicKeysUsedForClientEncryption = await _getSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction.Execute().ConfigureAwait(false);
            var result = new JsonWebKeySet
            {
                Keys = new List<Dictionary<string, object>>()
            };

            result.Keys.AddRange(publicKeysUsedToValidateSignature);
            result.Keys.AddRange(publicKeysUsedForClientEncryption);
            return result;
        }

        public async Task<bool> RotateJwks()
        {
            return await _rotateJsonWebKeysOperation.Execute().ConfigureAwait(false);
        }
    }
}
