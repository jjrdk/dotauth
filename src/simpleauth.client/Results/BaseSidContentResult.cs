namespace SimpleAuth.Client.Results
{
    public class BaseSidContentResult<T> : BaseSidResult
    {
        public T Content { get; set; }
    }
}