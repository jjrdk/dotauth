namespace SimpleAuth.Tests.Extensions
{
    using Shared;
    using SimpleAuth.Extensions;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Xunit;

    public sealed class ClaimPrincipalExtensionsFixture
    {
        [Fact]
        public void When_Passing_Entity_With_No_Identity_And_Called_GetSubject_Then_Null_Is_Returned()
        {
            var claimsPrincipal = new ClaimsPrincipal();

            var result = claimsPrincipal.GetSubject();

            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_ClaimsPrincipal_With_No_Subject_And_Calling_GetSubject_Then_Null_Is_Returned()
        {
            var claims = new List<Claim>();
            var claimsIdentity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var result = claimsPrincipal.GetSubject();

            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_ClaimsPrincipal_With_NameIdentifier_And_Calling_GetSubject_Then_NameIdentitifer_Is_Returned()
        {
            const string subject = "subject"; var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, subject)
            };
            var claimsIdentity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var result = claimsPrincipal.GetSubject();

            Assert.Equal(subject, result);
        }

        [Fact]
        public void When_Passing_ClaimsPrincipal_With_Subject_And_Calling_GetSubject_Then_Subject_Is_Returned()
        {
            const string subject = "subject"; var claims = new List<Claim>
            {
                new Claim(JwtConstants.OpenIdClaimTypes.Subject, subject)
            };
            var claimsIdentity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var result = claimsPrincipal.GetSubject();

            Assert.Equal(subject, result);
        }

        [Fact]
        public void When_Passing_No_Claims_Principal_And_Calling_IsAuthenticated_Then_False_Is_Returned()
        {
            var claimsPrincipal = new ClaimsPrincipal();

            var result = claimsPrincipal.IsAuthenticated();

            Assert.False(result);
        }

        [Fact]
        public void When_Passing_Claims_Principal_And_Calling_IsAuthenticated_Then_True_Is_Returned()
        {
            var claimsIdentity = new ClaimsIdentity("simpleAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var result = claimsPrincipal.IsAuthenticated();

            Assert.True(result);
        }
    }
}
