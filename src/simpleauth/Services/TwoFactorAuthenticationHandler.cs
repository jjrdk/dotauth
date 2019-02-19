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

namespace SimpleAuth.Services
{
    using Shared;
    using Shared.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal class TwoFactorAuthenticationHandler : ITwoFactorAuthenticationHandler
    {
        private readonly ITwoFactorAuthenticationService[] _twoFactorServices;

        public TwoFactorAuthenticationHandler(IEnumerable<ITwoFactorAuthenticationService> twoFactorServices)
        {
            _twoFactorServices = twoFactorServices == null
                ? Array.Empty<ITwoFactorAuthenticationService>()
                : twoFactorServices.ToArray();
        }

        public ITwoFactorAuthenticationService Get(string twoFactorAuthType)
        {
            if (string.IsNullOrWhiteSpace(twoFactorAuthType))
            {
                throw new ArgumentNullException(nameof(twoFactorAuthType));
            }

            return _twoFactorServices?.FirstOrDefault(s => s.Name == twoFactorAuthType);
        }

        public ITwoFactorAuthenticationService[] GetAll()
        {
            return _twoFactorServices;
        }

        public async Task<bool> SendCode(string code, string twoFactorAuthType, ResourceOwner user)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (string.IsNullOrWhiteSpace(twoFactorAuthType))
            {
                throw new ArgumentNullException(nameof(twoFactorAuthType));
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var service = Get(twoFactorAuthType);
            if (service == null)
            {
                return false;
            }

            await service.Send(code, user).ConfigureAwait(false);
            return true;
        }
    }
}
