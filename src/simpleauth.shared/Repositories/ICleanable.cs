namespace SimpleAuth.Shared.Repositories
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICleanable
    {
        Task Clean(CancellationToken cancellationToken);
    }
}