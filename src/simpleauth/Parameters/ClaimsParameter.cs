namespace SimpleAuth.Parameters
{
    using System.Collections.Generic;

    internal class ClaimsParameter
    {
        public List<ClaimParameter> UserInfo { get; set; } = new List<ClaimParameter>();

        public List<ClaimParameter> IdToken { get; set; } = new List<ClaimParameter>();
    }
}