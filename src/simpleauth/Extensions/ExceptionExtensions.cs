namespace SimpleAuth.Extensions
{
    using System;
    using Errors;
    using Exceptions;

    public static class ExceptionExtensions
    {
        public static void HandleException(this Exception exception, string message)
        {
            throw new SimpleAuthException(
                ErrorCodes.InternalError,
                message,
                exception);
        }
    }
}