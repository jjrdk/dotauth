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

namespace SimpleAuth.Common
{
    using System;

    /// <summary>
    /// Defines the token request content.
    /// </summary>
    public class TokenRequest
    {
        /// <summary>
        /// Gets or sets the type of the grant.
        /// </summary>
        /// <value>
        /// The type of the grant.
        /// </value>
        public string grant_type { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        public string username { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string password { get; set; }

        /// <summary>
        /// Gets or sets the scope.
        /// </summary>
        /// <value>
        /// The scope.
        /// </value>
        public string scope { get; set; }

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        public string code { get; set; }

        /// <summary>
        /// Gets or sets the redirect URI.
        /// </summary>
        /// <value>
        /// The redirect URI.
        /// </value>
        public Uri redirect_uri { get; set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        /// <value>
        /// The refresh token.
        /// </value>
        public string refresh_token { get; set; }

        /// <summary>
        /// Gets or sets the code verifier.
        /// </summary>
        /// <value>
        /// The code verifier.
        /// </value>
        public string code_verifier { get; set; }

        /// <summary>
        /// Gets or sets the amr values.
        /// </summary>
        /// <value>
        /// The amr values.
        /// </value>
        public string amr_values { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public string client_id { get; set; }

        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        /// <value>
        /// The client secret.
        /// </value>
        public string client_secret { get; set; }

        /// <summary>
        /// Gets or sets the type of the client assertion.
        /// </summary>
        /// <value>
        /// The type of the client assertion.
        /// </value>
        public string client_assertion_type { get; set; }

        /// <summary>
        /// Gets or sets the client assertion.
        /// </summary>
        /// <value>
        /// The client assertion.
        /// </value>
        public string client_assertion { get; set; }

        /// <summary>
        /// Gets or sets the ticket.
        /// </summary>
        /// <value>
        /// The ticket.
        /// </value>
        public string ticket { get; set; }

        /// <summary>
        /// Gets or sets the claim token.
        /// </summary>
        /// <value>
        /// The claim token.
        /// </value>
        public string claim_token { get; set; }

        /// <summary>
        /// Gets or sets the claim token format.
        /// </summary>
        /// <value>
        /// The claim token format.
        /// </value>
        public string claim_token_format { get; set; }

        /// <summary>
        /// Gets or sets the PCT.
        /// </summary>
        /// <value>
        /// The PCT.
        /// </value>
        public string pct { get; set; }

        /// <summary>
        /// Gets or sets the RPT.
        /// </summary>
        /// <value>
        /// The RPT.
        /// </value>
        public string rpt { get; set; }
    }
}
