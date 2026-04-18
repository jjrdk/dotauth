namespace DotAuth.Endpoints;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

internal sealed class RazorViewResult : IResult
{
    private readonly string _viewPath;
    private readonly object? _model;
    private readonly int _statusCode;
    private readonly IReadOnlyDictionary<string, object?>? _viewData;
    private readonly ModelStateDictionary _modelState;

    public RazorViewResult(
        string viewPath,
        object? model,
        int statusCode = StatusCodes.Status200OK,
        IReadOnlyDictionary<string, object?>? viewData = null,
        ModelStateDictionary? modelState = null)
    {
        _viewPath = viewPath;
        _model = model;
        _statusCode = statusCode;
        _viewData = viewData;
        _modelState = modelState ?? new ModelStateDictionary();
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = _statusCode;
        httpContext.Response.ContentType = "text/html; charset=utf-8";
        var serviceProvider = httpContext.RequestServices;
        var viewEngine = serviceProvider.GetRequiredService<IRazorViewEngine>();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), _modelState);
        var viewEngineResult = viewEngine.GetView(executingFilePath: null, _viewPath, isMainPage: true);
        if (!viewEngineResult.Success)
        {
            viewEngineResult = viewEngine.FindView(actionContext, _viewPath, isMainPage: true);
        }

        if (!viewEngineResult.Success)
        {
            throw new InvalidOperationException($"Couldn't find view '{_viewPath}'");
        }

        await using var output = new StreamWriter(httpContext.Response.Body);
        var viewDataDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), _modelState)
        {
            Model = _model
        };
        if (_viewData != null)
        {
            foreach (var item in _viewData)
            {
                viewDataDictionary[item.Key] = item.Value;
            }
        }

        var viewContext = new ViewContext(
            actionContext,
            viewEngineResult.View,
            viewDataDictionary,
            new TempDataDictionary(httpContext, serviceProvider.GetRequiredService<ITempDataProvider>()),
            output,
            new HtmlHelperOptions());

        await viewEngineResult.View.RenderAsync(viewContext).ConfigureAwait(false);
    }
}


