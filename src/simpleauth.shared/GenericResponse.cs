namespace SimpleAuth.Shared
{
    using System.Net;
    using Responses;

    public class GenericResponse<T>
    {
        public T Content { get; set; }
        public HttpStatusCode HttpStatus { get; set; }
        public bool ContainsError { get; set; }
        public ErrorResponse Error { get; set; }
    }
}