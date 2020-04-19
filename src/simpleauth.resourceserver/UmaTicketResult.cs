namespace SimpleAuth.ResourceServer
{
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;

    /// <summary>
    /// Defines the UMA ticket result class.
    /// </summary>
    public class UmaTicketResult : UmaResult<UmaTicketResult.UmaTicketInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmaTicketResult"/> class.
        /// </summary>
        /// <param name="info"></param>
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

        /// <summary>
        /// Defines the UMA ticket info class.
        /// </summary>
        public class UmaTicketInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UmaTicketInfo"/> class.
            /// </summary>
            /// <param name="ticketId">The ticket id.</param>
            /// <param name="umaAuthority">The UMA authority.</param>
            /// <param name="realm">The application realm.</param>
            public UmaTicketInfo(string ticketId, string umaAuthority, string realm = null)
            {
                TicketId = ticketId;
                UmaAuthority = umaAuthority;
                Realm = realm;
            }

            /// <summary>
            /// Gets the ticket id.
            /// </summary>
            public string TicketId { get; }

            /// <summary>
            /// Gets the UMA authority.
            /// </summary>
            public string UmaAuthority { get; }

            /// <summary>
            /// Gets the application realm.
            /// </summary>
            public string Realm { get; }
        }
    }
}