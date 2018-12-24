namespace SimpleAuth.Shared.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;

    public interface IClientStore
    {
        Task<Client> GetById(string clientId);
        Task<IEnumerable<Client>> GetAllAsync();
    }
}