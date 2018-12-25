namespace SimpleAuth.Api.Jwe.Actions
{
    using System.Threading.Tasks;
    using Parameters;

    public interface ICreateJweAction
    {
        Task<string> ExecuteAsync(CreateJweParameter createJweParameter);
    }
}