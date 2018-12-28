namespace SimpleAuth.Client.Results
{
    using Newtonsoft.Json.Linq;

    public class GetUserInfoResult : BaseSidResult
    {
        public JObject Content { get; set; }
        public string JwtToken { get; set; }
    }
}
