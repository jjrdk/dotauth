namespace DotAuth.Tests.Helpers;

using System;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Extensions;
using DotAuth.Services;
using Xunit;

public sealed class ResourceOwnerAuthenticateHelperFixture
{
    [Fact]
    public async Task When_Pass_Null_Parameters_Then_Exceptions_Are_Thrown()
    {
        IAuthenticateResourceOwnerService[] services = null;
        await Assert
            .ThrowsAsync<ArgumentNullException>(() => services.Authenticate(null, null, CancellationToken.None))
            ;
        await Assert
            .ThrowsAsync<ArgumentNullException>(() => services.Authenticate("login", null, CancellationToken.None))
            ;
    }
}
