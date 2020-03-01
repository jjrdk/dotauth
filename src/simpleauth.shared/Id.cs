namespace SimpleAuth.Shared
{
    using System;
    using System.Linq;

    internal static class Id
    {
        public static string Create()
        {
            return string.Join(string.Empty, Guid.NewGuid().ToByteArray().Select(x => x.ToString("X")));
        }
    }
}