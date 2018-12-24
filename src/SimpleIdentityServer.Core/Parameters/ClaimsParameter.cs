namespace SimpleIdentityServer.Core.Parameters
{
    using System.Collections.Generic;
    using SimpleAuth.Json;

    public class ClaimsParameter
    {
        public List<ClaimParameter> UserInfo { get; set; }

        public List<ClaimParameter> IdToken { get; set; }

        public override string ToString()
        {
            return this.SerializeWithJavascript();
        }
    }
}