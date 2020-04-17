namespace SimpleAuth.ResourceServer
{
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;

    public class UmaServerUnreachableResult : UmaResult<string>
    {
        private const string UmaAuthorizationServerUnreachable = "199 - \"UMA Authorization Server Unreachable\"";

        public UmaServerUnreachableResult() : base(UmaAuthorizationServerUnreachable)
        {
        }

        /// <inheritdoc />
        protected override Task ExecuteResult(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.StatusCode = (int)HttpStatusCode.Forbidden;
            response.Headers[HeaderNames.Warning] = UmaAuthorizationServerUnreachable;

            return Task.CompletedTask;
        }
    }
}
