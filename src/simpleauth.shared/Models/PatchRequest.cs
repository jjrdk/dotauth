namespace SimpleIdentityServer.Shared.Models
{
    public class PatchRequest
    {
        public PatchRequest()
        {
            Schemas = new[] { ScimConstants.Messages.PatchOp };
        }

        public string[] Schemas { get; }

        public PatchOperation[] Operations { get; set; }
    }
}