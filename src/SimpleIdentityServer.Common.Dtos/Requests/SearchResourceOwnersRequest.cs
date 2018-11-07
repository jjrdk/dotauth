namespace SimpleIdentityServer.Manager.Common.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Shared;

    [DataContract]
    public class SearchResourceOwnersRequest
    {
        [DataMember(Name = Constants.SearchResourceOwnerNames.Subjects)]
        public IEnumerable<string> Subjects { get; set; }

        [DataMember(Name = Constants.SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }

        [DataMember(Name = Constants.SearchResponseNames.TotalResults)]
        public int NbResults { get; set; }

        [DataMember(Name = Constants.SearchResourceOwnerNames.Order)]
        public OrderRequest Order { get; set; }
    }
}
