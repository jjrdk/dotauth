namespace SimpleAuth.Results
{
    using System.Collections.Generic;

    public class PaginatedResult<T>
    {
        public int StartIndex { get; set; }
        public int Count { get; set; }
        public IEnumerable<T> Content { get; set; }
    }
}