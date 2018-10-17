namespace SimpleIdentityServer.AccountFilter.Basic.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class UpdateFilterRequest
    {
        [DataMember(Name = Constants.FilterResponseNames.Id)]
        public string Id { get; set; }
        [DataMember(Name = Constants.FilterResponseNames.Name)]
        public string Name { get; set; }
        [DataMember(Name = Constants.FilterResponseNames.Rules)]
        public IEnumerable<UpdateFilterRuleRequest> Rules { get; set; }
    }
}