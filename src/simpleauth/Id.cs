namespace SimpleAuth
{
    using System;

    internal static class Id
    {
        public static string Create()
        {
            return BitConverter.ToString(Guid.NewGuid().ToByteArray()).Replace("-", string.Empty);
        }
    }
}