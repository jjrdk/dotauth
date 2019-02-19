namespace SimpleAuth.Shared
{
    using System;

    internal static class Id
    {
        public static string Create()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}