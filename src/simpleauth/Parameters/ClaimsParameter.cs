namespace SimpleAuth.Parameters
{
    using System.Collections.Generic;

    internal class ClaimsParameter
    {
        public List<ClaimParameter> UserInfo { get; set; }

        public List<ClaimParameter> IdToken { get; set; }
    }
}