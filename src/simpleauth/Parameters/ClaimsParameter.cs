namespace SimpleAuth.Parameters
{
    using System.Collections.Generic;

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