namespace SimpleAuth.ResourceServer
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;

    public abstract class UmaResult<T> : IActionResult
    {
        protected UmaResult(T value = default)
        {
            Value = value;
        }

        protected T Value { get; }

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