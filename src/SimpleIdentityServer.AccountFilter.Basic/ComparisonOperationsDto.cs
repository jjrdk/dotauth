namespace SimpleIdentityServer.AccountFilter.Basic
{
    using System.Runtime.Serialization;

    [DataContract]
    public enum ComparisonOperationsDto
    {
        [EnumMember(Value = Constants.ComparisonOperationsDtoNames.Equal)]
        Equal,
        [EnumMember(Value = Constants.ComparisonOperationsDtoNames.NotEqual)]
        NotEqual,
        [EnumMember(Value = Constants.ComparisonOperationsDtoNames.RegularExpression)]
        RegularExpression
    }
}
