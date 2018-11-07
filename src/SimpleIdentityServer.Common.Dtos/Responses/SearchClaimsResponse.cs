namespace SimpleIdentityServer.Shared.Responses
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Shared;

    [DataContract]
    public class SearchClaimsResponse
    {
        [DataMember(Name = Constants.SearchResponseNames.Content)]
        public IEnumerable<ClaimResponse> Content { get; set; }

        [DataMember(Name = Constants.SearchResponseNames.TotalResults)]
        public int TotalResults { get; set; }

        [DataMember(Name = Constants.SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }
    }
}
