namespace SimpleAuth.ResourceServer
{
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
    public abstract class UmaResult<T> : IActionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmaResult{T}"/> class.
        /// </summary>
        /// <param name="value"></param>
        protected UmaResult(T value = default)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the result value.
        /// </summary>
        protected T Value { get; }

        /// <summary>
        /// Executes the result processing.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected abstract Task ExecuteResult(ActionContext context);

        /// <inheritdoc />
        public async Task ExecuteResultAsync(ActionContext context)
        {
            await ExecuteResult(context).ConfigureAwait(false);
            if (!Value.Equals(default(T)))
            {
                var formatters = context.HttpContext.RequestServices.GetServices<IOutputFormatter>();
                var formatterSelector = context.HttpContext.RequestServices.GetRequiredService<OutputFormatterSelector>();
                var writerFactory =
                    context.HttpContext.RequestServices.GetRequiredService<IHttpResponseStreamWriterFactory>();
                var formatterContext = new OutputFormatterWriteContext(
                    context.HttpContext,
                    writerFactory.CreateWriter,
                    typeof(T),
                    Value);

                var selectedFormatter = formatterSelector.SelectFormatter(formatterContext, formatters.ToList(), new MediaTypeCollection());
                if (selectedFormatter == null)
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status406NotAcceptable;
                    return;
                }

                await selectedFormatter.WriteAsync(formatterContext).ConfigureAwait(false);
            }
        }
    }
}