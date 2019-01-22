using System.Collections.Generic;
using SimpleAuth.Services;

namespace SimpleAuth.Tests.Helpers
{
    using SimpleAuth.Helpers;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class ResourceOwnerAuthenticateHelperFixture
    {
        [Fact]
        public async Task When_Pass_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            IEnumerable<IAuthenticateResourceOwnerService> services = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => services.Authenticate(null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => services.Authenticate("login", null)).ConfigureAwait(false);
        }
    }
}
