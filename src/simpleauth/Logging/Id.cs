namespace SimpleAuth.Logging
{
    using System;

    public static class Id
    {
        public static string Create()
        {
            return Guid.NewGuid().ToString("");
        }
    }
}