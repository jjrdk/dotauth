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

namespace SimpleAuth.Shared.Models
{
    using System;
    using System.Linq;
    using System.Security.Claims;

    /// <summary>
    /// Defines the resource owner content.
    /// </summary>
    public class ResourceOwner
    {
        /// <summary>
        /// Get or sets the subject-identifier for the End-User at the issuer.
        /// </summary>
        public string Subject
        {
            get => Claims?.FirstOrDefault(x => x.Type == OpenIdClaimTypes.Subject)?.Value;
            set
            {
                var claim = new Claim(OpenIdClaimTypes.Subject, value);
                if (Claims == null)
                {
                    Claims = new[] { claim };
                }

                Claims = Claims.Where(x => x.Type != OpenIdClaimTypes.Subject).Concat(new[] { claim }).ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the list of claims.
        /// </summary>
        public Claim[] Claims { get; set; } = Array.Empty<Claim>();

        /// <summary>
        /// Gets or sets the two factor authentications
        /// </summary>
        public string TwoFactorAuthentication { get; set; }

        /// <summary>
        /// Gets or sets if the resource owner is local or external
        /// </summary>
        public bool IsLocalAccount { get; set; }

        /// <summary>
        /// Gets or sets the create datetime.
        /// </summary>
        public DateTimeOffset CreateDateTime { get; set; }

        /// <summary>
        /// Gets or sets the update datetime.
        /// </summary>
        public DateTimeOffset UpdateDateTime
        {
            get
            {
                var unix = Claims?.FirstOrDefault(x => x.Type == OpenIdClaimTypes.UpdatedAt)?.Value;
                return unix?.ConvertFromUnixTimestamp() ?? DateTime.MinValue;
            }
            set
            {
                var unix = value.ConvertToUnixTimestamp();
                var claim = new Claim(OpenIdClaimTypes.UpdatedAt, unix.ToString());
                if (Claims == null)
                {
                    Claims = new[] { claim };
                }

                Claims = Claims.Where(x => x.Type != OpenIdClaimTypes.UpdatedAt).Concat(new[] { claim }).ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the external logins.
        /// </summary>
        /// <value>
        /// The external logins.
        /// </value>
        public ExternalAccountLink[] ExternalLogins { get; set; } = Array.Empty<ExternalAccountLink>();
    }
}