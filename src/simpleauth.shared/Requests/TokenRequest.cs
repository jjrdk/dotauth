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

namespace SimpleAuth.Shared.Requests
{
    using System;

    public class TokenRequest
    {
        public GrantTypes? grant_type { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string scope { get; set; }
        public string code { get; set; }
        public Uri redirect_uri { get; set; }
        public string refresh_token { get; set; }
        public string code_verifier { get; set; }
        public string amr_values { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string client_assertion_type { get; set; }
        public string client_assertion { get; set; }
        public string ticket { get; set; }
        public string claim_token { get; set; }
        public string claim_token_format { get; set; }
        public string pct { get; set; }
        public string rpt { get; set; }
    }

    //[DataContract]
    //public class TokenRequest
    //{
    //    [DataMember(Name = RequestTokenNames.GrantType)]
    //    public GrantTypes? grant_type { get; set; }
    //    [DataMember(Name = RequestTokenNames.Username)]
    //    public string Username { get; set; }
    //    [DataMember(Name = RequestTokenNames.Password)]
    //    public string Password { get; set; }
    //    [DataMember(Name = RequestTokenNames.Scope)]
    //    public string Scope { get; set; }
    //    [DataMember(Name = RequestTokenNames.Code)]
    //    public string Code { get; set; }
    //    [DataMember(Name = RequestTokenNames.RedirectUri)]
    //    public Uri RedirectUri { get; set; }
    //    [DataMember(Name = RequestTokenNames.RefreshToken)]
    //    public string RefreshToken { get; set; }
    //    [DataMember(Name = RequestTokenNames.CodeVerifier)]
    //    public string CodeVerifier { get; set; }
    //    [DataMember(Name = RequestTokenNames.AmrValues)]
    //    public string AmrValues { get; set; }
    //    [DataMember(Name = ClientAuthNames.ClientId)]
    //    public string ClientId { get; set; }
    //    [DataMember(Name = ClientAuthNames.ClientSecret)]
    //    public string ClientSecret { get; set; }
    //    [DataMember(Name = ClientAuthNames.ClientAssertionType)]
    //    public string ClientAssertionType { get; set; }
    //    [DataMember(Name = ClientAuthNames.ClientAssertion)]
    //    public string ClientAssertion { get; set; }
    //    [DataMember(Name = RequestTokenUma.Ticket)]
    //    public string Ticket { get; set; }
    //    [DataMember(Name = RequestTokenUma.ClaimToken)]
    //    public string ClaimToken { get; set; }
    //    [DataMember(Name = RequestTokenUma.ClaimTokenFormat)]
    //    public string ClaimTokenFormat { get; set; }
    //    [DataMember(Name = RequestTokenUma.Pct)]
    //    public string Pct { get; set; }
    //    [DataMember(Name = RequestTokenUma.Rpt)]
    //    public string Rpt { get; set; }
    //}
}