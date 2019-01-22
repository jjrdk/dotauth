namespace SimpleAuth.Shared.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AccountFiltering;

    public interface IFilterStore
    {
        Task<IEnumerable<Filter>> GetAll();
    }
}
