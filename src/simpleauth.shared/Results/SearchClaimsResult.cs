namespace SimpleAuth.Shared.Results
{
    using System.Collections.Generic;
    using Models;

    public class SearchClaimsResult
    {
        public IEnumerable<ClaimAggregate> Content { get; set; }
        public int TotalResults { get; set; }
        public int StartIndex { get; set; }
    }
}
