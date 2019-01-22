namespace SimpleAuth.Controllers
{
    public class BulkOperationResponse
    {
        public string Method { get; set; }

        public int? Status { get; set; }

        public string BulkId { get; set; }

        public string Version { get; set; }

        public string Path { get; set; }

        public string Location { get; set; }

        public object Response { get; set; }
    }
}