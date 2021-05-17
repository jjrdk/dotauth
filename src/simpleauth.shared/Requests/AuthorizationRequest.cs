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

// ReSharper disable InconsistentNaming
namespace SimpleAuth.Shared.Requests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the authorization request.
    /// </summary>
    [DataContract]
    public record AuthorizationRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationRequest"/> class.
        /// </summary>
        public AuthorizationRequest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationRequest"/> class.
        /// </summary>
        /// <param name="scopes">The scopes.</param>
        /// <param name="responseTypes">The response types.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="redirectUri">The redirect URI.</param>
        /// <param name="requestState">State of the request.</param>
        /// <param name="codeChallenge">The PKCE code challenge.</param>
        /// <param name="codeChallengeMethod">The PKCE code challenge method.</param>
        public AuthorizationRequest(
            IEnumerable<string> scopes,
            IEnumerable<string> responseTypes,
            string clientId,
            Uri redirectUri,
            string? codeChallenge,
            string? codeChallengeMethod,
            string? requestState)
        {
            scope = string.Join(" ", scopes);
            response_type = string.Join(" ", responseTypes);
            client_id = clientId;
            redirect_uri = redirectUri;
            code_challenge = codeChallenge;
            code_challenge_method = codeChallengeMethod;
            state = requestState;
        }
        
#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// Gets or sets the scope.
        /// </summary>
        /// <value>
        /// The scope.
        /// </value>
        [DataMember(Name = "scope")]
        public string? scope { get; set; }

        /// <summary>
        /// Gets or sets the type of the response.
        /// </summary>
        /// <value>
        /// The type of the response.
        /// </value>
        [DataMember(Name = "response_type")]
        public string? response_type { get; set; }

        /// <summary>
        /// Gets or sets the redirect URI.
        /// </summary>
        /// <value>
        /// The redirect URI.
        /// </value>
        [DataMember(Name = "redirect_uri")]
        public Uri? redirect_uri { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        [DataMember(Name = "state")]
        public string? state { get; set; }

        /// <summary>
        /// Gets or sets the response mode.
        /// </summary>
        /// <value>
        /// The response mode.
        /// </value>
        [DataMember(Name = "response_mode")]
        public string? response_mode { get; set; }

        /// <summary>
        /// Gets or sets the nonce.
        /// </summary>
        /// <value>
        /// The nonce.
        /// </value>
        [DataMember(Name = "nonce")]
        public string? nonce { get; set; }

        /// <summary>
        /// Gets or sets the display.
        /// </summary>
        /// <value>
        /// The display.
        /// </value>
        [DataMember(Name = "display")]
        public DisplayModes? display { get; set; }

        /// <summary>
        /// The possible values are : none, login, consent, select_account
        /// </summary>
        [DataMember(Name = "prompt")]
        public string? prompt { get; set; }

        /// <summary>
        /// Maximum authentication age.
        /// Specifies allowable elapsed time in seconds since the last time the end-user
        ///  was actively authenticated by the OP.
        /// </summary>
        [DataMember(Name = "max_age")]
        public double max_age { get; set; }

        /// <summary>
        /// End-User's preferred languages
        /// </summary>
        [DataMember(Name = "ui_locales")]
        public string? ui_locales { get; set; }

        /// <summary>
        /// Token previousely issued by the Authorization Server.
        /// </summary>
        [DataMember(Name = "id_token_hint")]
        public string? id_token_hint { get; set; }

        /// <summary>
        /// Hint to the authorization server about the login identifier the end-user might use to log in.
        /// </summary>
        [DataMember(Name = "login_hint")]
        public string? login_hint { get; set; }

        /// <summary>
        /// Request that specific Claims be returned from the UserInfo endpoint and/or in the id token.
        /// </summary>
        [DataMember(Name = "claims")]
        public string? claims { get; set; }

        /// <summary>
        /// Requested Authentication Context Class References values.
        /// </summary>
        [DataMember(Name = "acr_values")]
        public string? acr_values { get; set; }

        /// <summary>
        /// Self-contained parameter and can be optionally be signed and / or encrypted
        /// </summary>
        [DataMember(Name = "request")]
        public string? request { get; set; }

        /// <summary>
        /// Enables OpenID connect requests to be passed by reference rather than by value.
        /// </summary>
        [DataMember(Name = "request_uri")]
        public Uri? request_uri { get; set; }

        /// <summary>
        /// Code challenge.
        /// </summary>
        [DataMember(Name = "code_challenge")]
        public string? code_challenge { get; set; }

        /// <summary>
        /// Code challenge method.
        /// </summary>
        [DataMember(Name = "code_challenge_method")]
        public string? code_challenge_method { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        [DataMember(Name = "client_id")]
        public string? client_id { get; set; }

        /// <summary>
        /// Gets or sets the aggregate identifier.
        /// </summary>
        /// <value>
        /// The aggregate identifier.
        /// </value>
        [DataMember(Name = "aggregate_id")]
        public string? aggregate_id { get; set; }

        /// <summary>
        /// Gets or sets the origin URL.
        /// </summary>
        /// <value>
        /// The origin URL.
        /// </value>
        [DataMember(Name = "origin_url")]
        public string? origin_url { get; set; }

        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        [DataMember(Name = "session_id")]
        public string? session_id { get; set; }

        /// <summary>
        /// Gets or sets the amr values.
        /// </summary>
        /// <value>
        /// The amr values.
        /// </value>
        [DataMember(Name = "amr_values")]
        public string? amr_values { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Converts to request string.
        /// </summary>
        /// <returns>The request as a <see cref="string"/>.</returns>
        public string ToRequest()
        {
            var properties = typeof(AuthorizationRequest).GetProperties()
                .Where(x => x.GetValue(this) != null)
                .Select(x => x.GetCustomAttribute<DataMemberAttribute>()!.Name + "=" + x.GetValue(this));

            return string.Join("&", properties);
        }
    }
}
