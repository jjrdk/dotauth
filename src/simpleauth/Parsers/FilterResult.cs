namespace SimpleAuth.Parsers
{
    using Newtonsoft.Json.Linq;

    public class FilterResult
    {
        public int? ItemsPerPage { get; set; }
        public int? StartIndex { get; set; }
        public int TotalNumbers { get; set; }
        public JArray Values { get; set; }
    }
}