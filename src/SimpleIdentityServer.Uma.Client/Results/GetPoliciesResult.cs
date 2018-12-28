namespace SimpleAuth.Uma.Client.Results
{
    using System.Collections.Generic;
    using SimpleAuth.Shared;

    public class GetPoliciesResult : BaseResponse
    {
        public IEnumerable<string> Content { get; set; }
    }
}
