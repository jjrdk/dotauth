namespace SimpleAuth.Api.Jwe.Actions
{
    using System.Threading.Tasks;
    using Parameters;
    using Results;

    public interface IGetJweInformationAction
    {
        Task<JweInformationResult> ExecuteAsync(GetJweParameter getJweParameter);
    }
}