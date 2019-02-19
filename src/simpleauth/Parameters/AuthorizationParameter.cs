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

namespace SimpleAuth.Parameters
{
    using System;

    internal sealed class AuthorizationParameter
    {
        public string ClientId { get; set; }
        public string Scope { get; set; }
        public string[] AmrValues { get; set; }
        public string ResponseType { get; set; }
        public Uri RedirectUrl { get; set; }
        public string State { get; set; }
        public string ResponseMode { get; set; }
        public string Nonce { get; set; }
        public string Prompt { get; set; }
        public double MaxAge { get; set; }
        public string UiLocales { get; set; }
        public string IdTokenHint { get; set; }
        public string LoginHint { get; set; }
        public string AcrValues { get; set; }
        public ClaimsParameter Claims { get; set; }
        public string CodeChallenge { get; set; }
        public string CodeChallengeMethod { get; set; }
        public string ProcessId { get; set; }
        public string OriginUrl { get; set; }
        public string SessionId { get; set; }
    }
}
