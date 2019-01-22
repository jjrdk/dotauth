namespace SimpleAuth.Client.Results
{
    using Shared.Models;

    public class GetRegisterClientResult : BaseSidResult
    {
        public Client Content { get; set; }
    }
}
