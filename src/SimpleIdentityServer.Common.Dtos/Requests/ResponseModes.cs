namespace SimpleIdentityServer.Shared.Requests
{
    using System.Runtime.Serialization;

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