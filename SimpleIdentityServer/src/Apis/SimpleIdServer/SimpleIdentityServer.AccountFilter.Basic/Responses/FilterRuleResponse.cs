namespace SimpleIdentityServer.AccountFilter.Basic.Responses
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [DataContract]
    public class FilterRuleResponse
    {
        [DataMember(Name = Constants.FilterRuleResponseNames.Id)]
        public string Id { get; set; }
        [DataMember(Name = Constants.FilterRuleResponseNames.ClaimKey)]
        public string ClaimKey { get; set; }
        [DataMember(Name = Constants.FilterRuleResponseNames.ClaimValue)]
        public string ClaimValue { get; set; }
        [DataMember(Name = Constants.FilterRuleResponseNames.Operation)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ComparisonOperationsDto Operation { get; set; }
    }
}