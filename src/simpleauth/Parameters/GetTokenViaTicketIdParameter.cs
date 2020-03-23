namespace SimpleAuth.Parameters
{
    internal class GetTokenViaTicketIdParameter : GrantTypeParameter
    {
        public string Ticket { get; set; }
        public ClaimTokenParameter ClaimToken { get; set; }
        public string Pct { get; set; }
        public string Rpt { get; set; }
    }
}
