namespace SimpleAuth.Uma.Client.Results
{
    using System.Collections.Generic;
    using SimpleAuth.Shared;

    public class GetResourcesResult : BaseResponse
    {
        public IEnumerable<string> Content { get; set; }
    }
}