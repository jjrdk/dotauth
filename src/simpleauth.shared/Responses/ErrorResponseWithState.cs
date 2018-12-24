namespace SimpleAuth.Shared.Responses
{
    using System.Runtime.Serialization;

    [DataContract]
    public class ErrorResponseWithState : ErrorResponse
    {
        [DataMember(Name = ErrorResponseWithStateNames.State)]
        public string State { get; set; }
    }
}
