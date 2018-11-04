﻿namespace SimpleIdentityServer.Uma.Client.Policy
{
    using System.Threading.Tasks;
    using Common.DTOs;
    using Shared;

    public interface IUpdatePolicyOperation
    {
        Task<BaseResponse> ExecuteAsync(PutPolicy request, string url, string token);
    }
}