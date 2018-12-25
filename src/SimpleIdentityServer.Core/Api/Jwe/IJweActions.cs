namespace SimpleAuth.Api.Jwe
{
    using System.Threading.Tasks;
    using Parameters;
    using Results;

    public interface IJweActions
    {
        Task<JweInformationResult> GetJweInformation(GetJweParameter getJweParameter);
        Task<string> CreateJwe(CreateJweParameter createJweParameter);
    }
}