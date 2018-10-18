using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Common.Responses
{
    [DataContract]
    public class PagedResponse<T>
    {
        [DataMember(Name = Constants.SearchResponseNames.TotalResults)]
        public int TotalResults { get; set; }
        [DataMember(Name = Constants.SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }
        [DataMember(Name = Constants.SearchResponseNames.Content)]
        public IEnumerable<T> Content { get; set; }
    }
}