namespace SimpleIdentityServer.Core.Common.DTOs.Requests
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DisplayModes
    {
        [EnumMember(Value = PageNames.Page)]
        Page,
        [EnumMember(Value = PageNames.Popup)]
        Popup,
        [EnumMember(Value = PageNames.Touch)]
        Touch,
        [EnumMember(Value = PageNames.Wap)]
        Wap
    }
}