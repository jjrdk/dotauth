namespace SimpleAuth.Uma.Models
{
    using System.Collections.Generic;

    public class SearchAuthPoliciesResult
    {
        public int TotalResults { get; set; }
        public int StartIndex { get; set; }
        public IEnumerable<Policy> Content { get; set; }
    }
}
