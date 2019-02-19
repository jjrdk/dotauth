namespace SimpleAuth.Shared.Requests
{
    using System.Runtime.Serialization;

    public enum DisplayModes
    {
        [EnumMember(Value = "page")]
        Page,
        [EnumMember(Value = "popup")]
        Popup,
        [EnumMember(Value = "touch")]
        Touch,
        [EnumMember(Value = "wap")]
        Wap
    }
}