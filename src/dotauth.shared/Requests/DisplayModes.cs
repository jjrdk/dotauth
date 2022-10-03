namespace DotAuth.Shared.Requests;

using System.Runtime.Serialization;

/// <summary>
/// Defines the display modes
/// </summary>
public enum DisplayModes
{
    /// <summary>
    /// Page
    /// </summary>
    [EnumMember(Value = "page")]
    Page,

    /// <summary>
    /// Popup
    /// </summary>
    [EnumMember(Value = "popup")]
    Popup,

    /// <summary>
    /// Touch
    /// </summary>
    [EnumMember(Value = "touch")]
    Touch,

    /// <summary>
    /// Wap
    /// </summary>
    [EnumMember(Value = "wap")]
    Wap
}