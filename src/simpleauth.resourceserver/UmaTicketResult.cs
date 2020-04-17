namespace SimpleAuth.ResourceServer
{
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;

    public class UmaTicketResult : UmaResult<UmaTicketResult.UmaTicketInfo>
    {
        public UmaTicketResult(UmaTicketInfo info) : base(info)
        {
        }

        /// <inheritdoc />
        protected override Task ExecuteResult(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.StatusCode = (int)HttpStatusCode.Unauthorized;
            var s = string.IsNullOrWhiteSpace(Value.Realm) ? string.Empty : $"realm=\"{Value.Realm}\", ";
            response.Headers[HeaderNames.WWWAuthenticate] =
                $"UMA {s}as_uri=\"{Value.UmaAuthority}\", ticket=\"{Value.TicketId}\"";

            return Task.CompletedTask;
        }

        public class UmaTicketInfo
        {
            public UmaTicketInfo(string ticketId, string umaAuthority, string realm = null)
            {
                TicketId = ticketId;
                UmaAuthority = umaAuthority;
                Realm = realm;
            }

            public string TicketId { get; }

            public string UmaAuthority { get; }

            public string Realm { get; }
        }
    }
}