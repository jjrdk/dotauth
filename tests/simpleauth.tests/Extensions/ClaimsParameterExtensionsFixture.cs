namespace SimpleAuth.Tests.Extensions;

using Parameters;
using Shared;
using SimpleAuth.Extensions;
using Xunit;

public sealed class ClaimsParameterExtensionsFixture
{
    [Fact]
    public void When_Trying_To_Retrieve_Standard_Claim_Names_From_EmptyList_Then_Empty_List_Is_Returned()
    {
        var claimsParameter = new ClaimsParameter();

        var claimNames = claimsParameter.GetClaimNames();

        Assert.Empty(claimNames);
    }

    [Fact]
    public void When_Passing_Standard_Claims_In_UserInfo_And_Trying_To_Retrieve_The_Names_Then_Names_Are_Returned()
    {
        const string notStandardClaimName = "not_standard";
        var claimsParameter = new ClaimsParameter
        {
            UserInfo = new[]
            {
                new ClaimParameter {Name = OpenIdClaimTypes.Subject},
                new ClaimParameter {Name = notStandardClaimName}
            }
        };

        var claimNames = claimsParameter.GetClaimNames();

        Assert.Contains(OpenIdClaimTypes.Subject, claimNames);
        Assert.DoesNotContain(notStandardClaimName, claimNames);
    }
}