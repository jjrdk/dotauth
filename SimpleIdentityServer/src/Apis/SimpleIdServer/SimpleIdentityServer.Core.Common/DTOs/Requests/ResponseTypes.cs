namespace SimpleIdentityServer.Core.Common.DTOs.Requests
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResponseTypes
    {
        [EnumMember(Value = ResponseTypeNames.Code)]
        Code,
        [EnumMember(Value = ResponseTypeNames.Token)]
        Token,
        [EnumMember(Value = ResponseTypeNames.IdToken)]
        IdToken
    }
}