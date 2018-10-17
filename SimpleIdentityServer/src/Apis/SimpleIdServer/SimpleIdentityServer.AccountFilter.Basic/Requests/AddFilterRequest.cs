namespace SimpleIdentityServer.AccountFilter.Basic.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class AddFilterRequest
    {
        [DataMember(Name = Constants.FilterResponseNames.Name)]
        public string Name { get; set; }
        [DataMember(Name = Constants.FilterResponseNames.Rules)]
        public IEnumerable<AddFilterRuleRequest> Rules { get; set; }
    }
}
