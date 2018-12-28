namespace SimpleAuth.Uma.Models
{
    using System.Collections.Generic;

    public class SearchResourceSetResult
    {
        public int TotalResults { get; set; }
        public int StartIndex { get; set; }
        public IEnumerable<ResourceSet> Content { get; set; }
    }
}
