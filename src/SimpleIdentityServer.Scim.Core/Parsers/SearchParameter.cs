namespace SimpleIdentityServer.Scim.Core.Parsers
{
    using System.Collections.Generic;

    public class SearchParameter
    {
        public SearchParameter()
        {
            Count = 100;
            StartIndex = 0;
            SortOrder = SortOrders.Ascending;
        }

        /// <summary>
        /// Names of resource attributes to return in the response.
        /// </summary>
        public IEnumerable<Filter> Attributes { get; set; }
        /// <summary>
        /// Names of resource attributes to be removed from the default set of attributes to return.
        /// </summary>
        public IEnumerable<Filter> ExcludedAttributes { get; set; }
        /// <summary>
        /// Filter used to request a subset of resources.
        /// </summary>
        public Filter Filter { get; set; }
        /// <summary>
        /// Indicate whose value SHALL be used to order the returned responses.
        /// </summary>
        public Filter SortBy { get; set; }
        /// <summary>
        /// In which the "sortBy" parameter is applied.
        /// </summary>
        public SortOrders SortOrder { get; set; }
        /// <summary>
        /// The 1-based index of the first query result.
        /// </summary>
        public int StartIndex { get; set; }
        /// <summary>
        /// The desired maximum number of query results per page.
        /// </summary>
        public int Count { get; set; }
    }
}