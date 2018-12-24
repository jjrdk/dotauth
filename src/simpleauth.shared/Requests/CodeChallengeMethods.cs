namespace SimpleIdentityServer.Shared.Requests
{
    using System.Runtime.Serialization;

    public enum CodeChallengeMethods
    {
        [EnumMember(Value = CodeChallenges.Plain)]
        Plain,
        [EnumMember(Value = CodeChallenges.S256)]
        S256
    }
}