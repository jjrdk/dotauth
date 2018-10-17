namespace SimpleIdentityServer.Core.Common.DTOs.Requests
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum CodeChallengeMethods
    {
        [EnumMember(Value = CodeChallenges.Plain)]
        Plain,
        [EnumMember(Value = CodeChallenges.S256)]
        S256
    }
}