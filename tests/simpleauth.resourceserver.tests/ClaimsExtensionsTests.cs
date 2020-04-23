namespace SimpleAuth.ResourceServer.Tests
{
    using System.Security.Claims;
    using Newtonsoft.Json;
    using SimpleAuth.Shared.Models;
    using Xunit;

    public class ClaimsExtensionsTests
    {
        [Fact]
        public void WhenNullClaimsArePassedThenReturnsFalse()
        {
            Claim[] claims = null;
            var result = claims.TryGetUmaTickets(out var permissions);

            Assert.False(result);
        }

        [Fact]
        public void WhenPrincipalClaimsHasUmaTicketThenRetrieves()
        {
            var json = JsonConvert.SerializeObject(new Permission { ResourceSetId = "test" });
            var principal = new ClaimsIdentity(new[] { new Claim("permissions", json) });

            var result = principal.TryGetUmaTickets(out var permissions);

            Assert.True(result);
            Assert.Single(permissions);
        }
    }
}
