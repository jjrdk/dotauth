namespace SimpleIdentityServer.Uma.Core.Helpers
{
    using System;
    using System.Threading.Tasks;

    public interface IRepositoryExceptionHelper
    {
        Task<T> HandleException<T>(string message, Func<Task<T>> callback);
    }
}