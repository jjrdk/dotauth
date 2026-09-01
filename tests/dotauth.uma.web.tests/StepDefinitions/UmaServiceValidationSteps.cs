namespace DotAuth.Uma.Web.Tests.StepDefinitions;

using System;
using System.Threading.Tasks;
using DotAuth.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Reqnroll;

[Binding]
public class UmaServiceValidationSteps : IAsyncDisposable
{
    private bool _registerResourceMap = true;
    private bool _registerPermissionClient = true;
    private bool _registerTokenClient = true;

    private IHost? _host;
    private Exception? _startupException;

    [Given("IResourceMap, IUmaPermissionClient, and ITokenClient are registered")]
    public void GivenAllServicesRegistered()
    {
        _registerResourceMap = true;
        _registerPermissionClient = true;
        _registerTokenClient = true;
    }

    [Given("AddUmaBearer has been called")]
    public void GivenAddUmaBearerCalled()
    {
        // Config is applied in the When step — this step just documents the intent.
    }

    [Given("IUmaPermissionClient and ITokenClient are registered but IResourceMap is not")]
    public void GivenResourceMapMissing()
    {
        _registerResourceMap = false;
        _registerPermissionClient = true;
        _registerTokenClient = true;
    }

    [Given("IResourceMap and IUmaPermissionClient are registered but ITokenClient is not")]
    public void GivenTokenClientMissing()
    {
        _registerResourceMap = true;
        _registerPermissionClient = true;
        _registerTokenClient = false;
    }

    [When("the host is built")]
    public async Task WhenHostIsBuilt()
    {
        try
        {
            _host = BuildHost();
            await _host.StartAsync();
        }
        catch (Exception ex)
        {
            _startupException = ex;
        }
    }

    [Then("no startup exception is thrown")]
    public void ThenNoException() => Assert.Null(_startupException);

    [Then("an InvalidOperationException is thrown mentioning {string}")]
    public void ThenInvalidOperationExceptionMentioning(string mention)
    {
        Assert.NotNull(_startupException);

        // The exception may be wrapped (e.g. in a TargetInvocationException or AggregateException).
        var message = _startupException!.ToString();
        Assert.Contains("InvalidOperationException", message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(mention, message, StringComparison.OrdinalIgnoreCase);
    }

    private IHost BuildHost()
    {
        return new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    if (_registerResourceMap)
                        services.AddSingleton(Substitute.For<IResourceMap>());

                    if (_registerPermissionClient)
                        services.AddSingleton(Substitute.For<IUmaPermissionClient>());

                    if (_registerTokenClient)
                        services.AddSingleton(Substitute.For<ITokenClient>());

                    services.AddAuthentication(UmaBearerDefaults.AuthenticationScheme)
                        .AddUmaBearer(_ => { });
                    services.AddRouting();
                });
                webHost.Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthentication();
                });
            })
            .Build();
    }

    public async ValueTask DisposeAsync()
    {
        if (_host is not null)
        {
            try { await _host.StopAsync(); } catch { /* ignore */ }
            _host.Dispose();
        }
    }
}
