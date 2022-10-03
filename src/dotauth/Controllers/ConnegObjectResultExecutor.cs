namespace DotAuth.Controllers;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

internal sealed class ConnegObjectResultExecutor : ObjectResultExecutor
{
    /// <inheritdoc />
    public ConnegObjectResultExecutor(
        OutputFormatterSelector formatterSelector,
        IHttpResponseStreamWriterFactory writerFactory,
        ILoggerFactory loggerFactory,
        IOptions<MvcOptions> mvcOptions)
        : base(formatterSelector, writerFactory, loggerFactory, mvcOptions)
    {
    }

    /// <inheritdoc />
    public override Task ExecuteAsync(ActionContext context, ObjectResult result)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.HttpContext.Items["ModelState"] = context.ModelState;
        context.HttpContext.Items["RouteData"] = context.RouteData;
        context.HttpContext.Items["ActionDescriptor"] = context.ActionDescriptor;

        return base.ExecuteAsync(context, result);
    }
}