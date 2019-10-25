namespace SimpleAuth.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    internal static class Id
    {
        public static string Create()
        {
            return BitConverter.ToString(Guid.NewGuid().ToByteArray()).Replace("-", string.Empty);
        }
    }
}