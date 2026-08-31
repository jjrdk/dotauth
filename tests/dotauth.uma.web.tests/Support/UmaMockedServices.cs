namespace DotAuth.Uma.Web.Tests.Support;

using System;
using System.Threading;
using DotAuth.Client;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using DotAuth.Shared;
using Microsoft.AspNetCore.Authentication;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

/// <summary>
/// Scenario-scoped bag of shared mocked UMA services.
/// Injected into all step definition classes that share background steps.
/// </summary>
public class UmaMockedServices
{
    public IResourceMap ResourceMap { get; } = Substitute.For<IResourceMap>();
    public IUmaPermissionClient PermissionClient { get; } = Substitute.For<IUmaPermissionClient>();
    public ITokenClient TokenClient { get; } = Substitute.For<ITokenClient>();

    public UmaMockedServices()
    {
        PermissionClient.Authority.Returns(new Uri("https://as.example.com"));
    }
}
