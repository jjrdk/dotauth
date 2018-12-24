namespace SimpleAuth.Shared.Parameters
{
    using System.Collections.Generic;

    public class SearchClaimsParameter
    {
        public SearchClaimsParameter()
        {
            ClaimKeys = new List<string>();
            IsPagingEnabled = true;
            StartIndex = 0;
            Count = 500;
        }

        public OrderParameter Order { get; set; }
        public IEnumerable<string> ClaimKeys { get; set; }
        public bool IsPagingEnabled { get; set; }
        public int StartIndex { get; set; }
        public int Count { get; set; }
    }
}
