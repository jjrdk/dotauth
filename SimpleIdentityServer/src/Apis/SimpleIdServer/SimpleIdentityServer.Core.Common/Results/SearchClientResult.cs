namespace SimpleIdentityServer.Core.Common.Results
{
    using System.Collections.Generic;
    using Models;

    public class SearchClientResult
    {
        public IEnumerable<Client> Content { get; set; }
        public int TotalResults { get; set; }
        public int StartIndex { get; set; }
    }
}
