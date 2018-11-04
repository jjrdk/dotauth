namespace SimpleIdentityServer.Shared.Parameters
{
    using System.Collections.Generic;

    public class SearchScopesParameter
    {
        public SearchScopesParameter()
        {
            ScopeNames = new List<string>();
            IsPagingEnabled = true;
            StartIndex = 0;
            Count = 500;
        }

        public OrderParameter Order { get; set; }
        public IEnumerable<string> ScopeNames { get; set; }
        public IList<int> Types { get; set; }
        public bool IsPagingEnabled { get; set; }
        public int StartIndex { get; set; }
        public int Count { get; set; }
    }
}
