namespace SimpleAuth.Shared.Requests
{
    using System.Runtime.Serialization;

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