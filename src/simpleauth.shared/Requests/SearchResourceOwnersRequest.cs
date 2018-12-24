namespace SimpleAuth.Shared.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class SearchResourceOwnersRequest
    {
        [DataMember(Name = SharedConstants.SearchResourceOwnerNames.Subjects)]
        public IEnumerable<string> Subjects { get; set; }

        [DataMember(Name = SharedConstants.SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }

        [DataMember(Name = SharedConstants.SearchResponseNames.TotalResults)]
        public int NbResults { get; set; }

        [DataMember(Name = SharedConstants.SearchResourceOwnerNames.Order)]
        public OrderRequest Order { get; set; }
    }
}
