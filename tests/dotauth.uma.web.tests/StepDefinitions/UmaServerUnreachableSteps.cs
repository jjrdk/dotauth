namespace DotAuth.Uma.Web.Tests.StepDefinitions;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

[Binding]
public class UmaServerUnreachableSteps
{
    private UmaServerUnreachableResult? _result;
    private ActionContext? _actionContext;
    private DefaultHttpContext? _httpContext;

    [Given("a UmaServerUnreachableResult instance")]
    public void GivenAUmaServerUnreachableResult()
    {
        _result = new UmaServerUnreachableResult();
        _httpContext = new DefaultHttpContext();

        // Provide a minimal MVC service provider so the result can execute.
        var services = new ServiceCollection();
        services.AddMvc();
        services.AddLogging();
        _httpContext.RequestServices = services.BuildServiceProvider();
        _actionContext = new ActionContext(_httpContext, new RouteData(), new ActionDescriptor());
    }

    [When("ExecuteResultAsync is called")]
    public async Task WhenExecuteResultAsyncIsCalled()
    {
        await _result!.ExecuteResultAsync(_actionContext!);
    }

    [Then("the response status code is (\\d+)")]
    public void ThenResponseStatusIs(int code) =>
        Assert.Equal(code, _httpContext!.Response.StatusCode);

    [Then("the response contains a Warning header")]
    public void ThenResponseContainsWarningHeader() =>
        Assert.True(_httpContext!.Response.Headers.ContainsKey("Warning"),
            "Expected a 'Warning' response header");

    [Then("the response contains a Retry-After header with a positive integer value")]
    public void ThenResponseContainsRetryAfterHeader()
    {
        Assert.True(_httpContext!.Response.Headers.TryGetValue("Retry-After", out var values),
            "Expected a 'Retry-After' response header");
        Assert.True(int.TryParse(values.ToString(), out var seconds) && seconds > 0,
            $"Retry-After value '{values}' should be a positive integer");
    }
}
