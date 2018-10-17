namespace SimpleIdentityServer.Core.Common.DTOs.Requests
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResponseModes
    {
        [EnumMember(Value = ResponseModeNames.None)]
        None,
        [EnumMember(Value = ResponseModeNames.Query)]
        Query,
        [EnumMember(Value = ResponseModeNames.Fragment)]
        Fragment,
        [EnumMember(Value = ResponseModeNames.FormPost)]
        FormPost
    }
}