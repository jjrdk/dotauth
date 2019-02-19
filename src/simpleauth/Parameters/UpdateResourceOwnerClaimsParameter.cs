namespace SimpleAuth.Parameters
{
    using System.Collections.Generic;

    public class UpdateResourceOwnerClaimsParameter
    {
        public string Login { get; set; }

        public List<KeyValuePair<string, string>> Claims { get; set; }
    }
}
