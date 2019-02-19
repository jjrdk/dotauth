namespace SimpleAuth.Stores.Marten
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    internal static class Extensions
    {
        public static string ToSha256Hash(this string entry)
        {
            using (var sha256 = SHA256.Create())
            {
                var entryBytes = Encoding.UTF8.GetBytes(entry);
                var hash = sha256.ComputeHash(entryBytes);
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }
    }
}