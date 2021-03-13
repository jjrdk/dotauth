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
    using System.IdentityModel.Tokens.Jwt;

    /// <summary>
    /// Defines the authorization code.
    /// </summary>
    public record AuthorizationCode
    {
        /// <summary>
        /// Gets or sets the authorization code.
        /// </summary>
        public string Code { get; init; } = null!;

        /// <summary>
        /// Gets or sets the redirection uri.
        /// </summary>
        public Uri RedirectUri { get; init; } = null!;

        /// <summary>
        /// Gets or sets the creation date time.
        /// </summary>
        public DateTimeOffset CreateDateTime { get; init; }

        /// <summary>
        /// Gets or sets the client id.
        /// </summary>
        public string ClientId { get; init; } = null!;

        /// <summary>
        /// Gets or sets the id token payload.
        /// </summary>
        public JwtPayload? IdTokenPayload { get; init; }

        /// <summary>
        /// Gets or sets the user information payload.
        /// </summary>
        public JwtPayload? UserInfoPayLoad { get; init; }

        /// <summary>
        /// Gets or sets the concatenated list of scopes.
        /// </summary>
        public string Scopes { get; init; } = null!;

        /// <summary>
        /// Code challenge.
        /// </summary>
        public string CodeChallenge { get; init; } = null!;

        /// <summary>
        /// Code challenge method.
        /// </summary>
        public string CodeChallengeMethod { get; init; } = null!;
    }
}
