namespace SimpleAuth.Shared.Responses
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class SearchClaimsResponse
    {
        [DataMember(Name = SharedConstants.SearchResponseNames.Content)]
        public IEnumerable<ClaimResponse> Content { get; set; }

        [DataMember(Name = SharedConstants.SearchResponseNames.TotalResults)]
        public int TotalResults { get; set; }

        [DataMember(Name = SharedConstants.SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }
    }
}
