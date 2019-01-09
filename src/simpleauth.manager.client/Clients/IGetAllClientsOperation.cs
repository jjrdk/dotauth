namespace SimpleAuth.Manager.Client.Clients
{
    using System;
    using System.Threading.Tasks;
    using Results;

    public interface IGetAllClientsOperation
    {
        Task<GetAllClientResult> ExecuteAsync(Uri clientsUri, string authorizationHeaderValue = null);
    }
}