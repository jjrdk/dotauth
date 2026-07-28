namespace DotAuth.Uma.Web;

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Defines the UMA result base class.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class UmaResult<T> : IResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmaResult{T}"/> class.
    /// </summary>
    /// <param name="value"></param>
    protected UmaResult(T? value = default)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the result value.
    /// </summary>
    [AllowNull]
    protected T Value { get; }

    /// <inheritdoc />
    public virtual async Task ExecuteAsync(HttpContext context)
    {
        if (Value?.Equals(default(T)) != true)
        {
            var formatters = context.RequestServices.GetServices<IOutputFormatter>();
            var formatterSelector = context.RequestServices.GetRequiredService<OutputFormatterSelector>();
            var writerFactory =
                context.RequestServices.GetRequiredService<IHttpResponseStreamWriterFactory>();
            var formatterContext = new OutputFormatterWriteContext(
                context,
                writerFactory.CreateWriter,
                typeof(T),
                Value!);

            var selectedFormatter = formatterSelector.SelectFormatter(
                formatterContext,
                formatters.ToArray(),
                new MediaTypeCollection());
            if (selectedFormatter == null)
            {
                context.Response.StatusCode = StatusCodes.Status406NotAcceptable;
                return;
            }

            await selectedFormatter.WriteAsync(formatterContext).ConfigureAwait(false);
        }
    }
}
