namespace SimpleIdentityServer.Client.Builders
{
    using System.Security.Cryptography;
    using System.Text;
    using Shared;

    public static class PasswordHelper
    {
        public static string ToSha256SimplifiedBase64(this string entry, Encoding encoding = null)
        {
            var enc = encoding ?? Encoding.UTF8;
            using (var sha256 = SHA256.Create())
            {
                var entryBytes = enc.GetBytes(entry);
                var hash = sha256.ComputeHash(entryBytes);
                return hash.ToBase64Simplified();
            }
        }
    }
}