namespace SimpleAuth.Shared.Responses
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class PagedResponse<T>
    {
        [DataMember(Name = SharedConstants.SearchResponseNames.TotalResults)]
        public int TotalResults { get; set; }
        [DataMember(Name = SharedConstants.SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }
        [DataMember(Name = SharedConstants.SearchResponseNames.Content)]
        public IEnumerable<T> Content { get; set; }
    }
}