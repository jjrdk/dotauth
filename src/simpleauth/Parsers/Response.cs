namespace SimpleAuth.Parsers
{
    using Newtonsoft.Json.Linq;

    public class Response
    {
        public JObject Object { get; set; }
        public string Location { get; set; }
    }
}