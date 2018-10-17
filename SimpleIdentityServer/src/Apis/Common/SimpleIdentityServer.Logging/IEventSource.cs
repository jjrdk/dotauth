namespace SimpleIdentityServer.Logging
{
    using System;

    public interface IEventSource
    {
        void Info(string message);
        void Failure(string message);
        void Failure(Exception exception);
    }
}