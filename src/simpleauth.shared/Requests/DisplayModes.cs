namespace SimpleAuth.Shared.Requests
{
    using System.Runtime.Serialization;

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