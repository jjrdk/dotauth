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

namespace SimpleAuth.Sms
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;

    internal class DefaultSmsService : ITwoFactorAuthenticationService
    {
        private readonly TwoFactorSmsOptions _options;
        private readonly ISmsClient _smsClient;

        public DefaultSmsService(ISmsClient smsClient, TwoFactorSmsOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _smsClient = smsClient;
        }

        public string RequiredClaim => OpenIdClaimTypes.PhoneNumber;

        public string Name => "SMS";

        public async Task Send(string code, ResourceOwner user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user.Claims == null)
            {
                throw new ArgumentNullException(nameof(user.Claims));
            }

            var phoneNumberClaim = user.Claims.FirstOrDefault(c => c.Type == RequiredClaim);
            if (phoneNumberClaim == null)
            {
                throw new ArgumentException("the phone number is missing");
            }

            await _smsClient.SendMessage(
                    phoneNumberClaim.Value,
                    string.Format(_options.SmsMessage, code))
                .ConfigureAwait(false);
        }
    }
}
