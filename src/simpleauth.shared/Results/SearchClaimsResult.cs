namespace SimpleAuth.Shared.Results
{
    using System.Collections.Generic;
    using System.Security.Claims;

    public class SearchClaimsResult
    {
        public IEnumerable<Claim> Content { get; set; }
        public int TotalResults { get; set; }
        public int StartIndex { get; set; }
    }
}
