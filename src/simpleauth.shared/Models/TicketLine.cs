namespace SimpleAuth.Shared.Models
{
    using System.Collections.Generic;

    public class TicketLine
    {
        public string Id { get; set; }
        public IEnumerable<string> Scopes { get; set; }
        public string ResourceSetId { get; set; }
    }
}
