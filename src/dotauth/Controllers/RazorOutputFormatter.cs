namespace DotAuth.Controllers;

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DotAuth.Shared.Models;
using DotAuth.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

internal sealed class RazorOutputFormatter : TextOutputFormatter
{
    public RazorOutputFormatter()
    {
        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.ASCII);
        SupportedEncodings.Add(Encoding.UTF32);
        SupportedEncodings.Add(Encoding.Unicode);
        SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
        SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/xhtml"));
        SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xhtml"));
        SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xhtml+xml"));
    }

    /// <inheritdoc />
    public override bool CanWriteResult(OutputFormatterCanWriteContext context)
    {
        if (!base.CanWriteResult(context))
        {
            return false;
        }

        var httpContext = context.HttpContext;
        var viewEngine = httpContext.RequestServices.GetRequiredService<IRazorViewEngine>();

        httpContext.Items.TryGetValue("ModelState", out var modelState);
        httpContext.Items.TryGetValue("RouteData", out var routeData);
        httpContext.Items.TryGetValue("ActionDescriptor", out var actionDescriptor);
        var actionContext = new ActionContext(
            httpContext,
            routeData as RouteData ?? new RouteData(),
            actionDescriptor as ActionDescriptor ?? new ActionDescriptor(),
            modelState as ModelStateDictionary ?? new ModelStateDictionary());
        if (!actionContext.RouteData.Values.TryGetValue("view", out var viewObject) || viewObject is not string viewName)
        {
            viewName = actionContext.RouteData.Values["action"]?.ToString() ?? string.Empty;
        }

        var result = viewEngine.FindView(actionContext, viewName, false);

        return result.Success;
    }

    /// <inheritdoc />
    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        var httpContext = context.HttpContext;
        var serviceProvider = httpContext.RequestServices;
        var viewEngine = serviceProvider.GetRequiredService<IRazorViewEngine>();
        httpContext.Items.TryGetValue("ModelState", out var modelState);
        httpContext.Items.TryGetValue("RouteData", out var routeData);
        httpContext.Items.TryGetValue("ActionDescriptor", out var actionDescriptor);
        var actionContext = new ActionContext(
            httpContext,
            routeData as RouteData ?? new RouteData(),
            actionDescriptor as ActionDescriptor ?? new ActionDescriptor(),
            modelState as ModelStateDictionary ?? new ModelStateDictionary());
        try
        {
            if (!actionContext.RouteData.Values.TryGetValue("view", out var viewObject) || viewObject is not string viewName)
            {
                viewName = actionContext.RouteData.Values["action"]?.ToString() ?? string.Empty;
            }

            var viewEngineResult = viewEngine.FindView(actionContext, viewName, isMainPage: false);

            if (!viewEngineResult.Success)
            {
                throw new InvalidOperationException($"Couldn't find view '{viewName}'");
            }
            var model = context.Object is ErrorDetails details
                ? new ErrorViewModel { Code = (int)details.Status, Title = details.Title, Message = details.Detail }
                : context.Object;
            var view = viewEngineResult.View;
            viewEngineResult.EnsureSuccessful(Array.Empty<string>());
            var output = new StreamWriter(httpContext.Response.Body);
            await using var _ = output.ConfigureAwait(false);

            var viewContext = new ViewContext(
                actionContext,
                view,
                new ViewDataDictionary(
                    metadataProvider: new EmptyModelMetadataProvider(),
                    modelState: actionContext.ModelState)
                {
                    Model = model
                },
                new TempDataDictionary(actionContext.HttpContext, serviceProvider.GetRequiredService<ITempDataProvider>()),
                output,
                new HtmlHelperOptions());

            await view.RenderAsync(viewContext).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Trace.TraceError(e.Message);
            throw;
        }
    }
}