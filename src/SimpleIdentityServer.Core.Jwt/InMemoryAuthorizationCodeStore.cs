namespace SimpleIdentityServer.Core.Jwt
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    internal sealed class InMemoryAuthorizationCodeStore : IAuthorizationCodeStore
    {
        private static Dictionary<string, AuthorizationCode> _mappingStringToAuthCodes;

        public InMemoryAuthorizationCodeStore()
        {
            _mappingStringToAuthCodes = new Dictionary<string, AuthorizationCode>();
        }

        public Task<AuthorizationCode> GetAuthorizationCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (!_mappingStringToAuthCodes.ContainsKey(code))
            {
                return Task.FromResult((AuthorizationCode)null);
            }

            return Task.FromResult(_mappingStringToAuthCodes[code]);
        }

        public Task<bool> AddAuthorizationCode(AuthorizationCode authorizationCode)
        {
            if (authorizationCode == null)
            {
                throw new ArgumentNullException(nameof(authorizationCode));
            }

            if (_mappingStringToAuthCodes.ContainsKey(authorizationCode.Code))
            {
                return Task.FromResult(false);
            }

            _mappingStringToAuthCodes.Add(authorizationCode.Code, authorizationCode);
            return Task.FromResult(true);
        }

        public Task<bool> RemoveAuthorizationCode(string authorizationCode)
        {
            if (authorizationCode == null)
            {
                throw new ArgumentNullException(nameof(authorizationCode));
            }

            if (!_mappingStringToAuthCodes.ContainsKey(authorizationCode))
            {
                return Task.FromResult(false);
            }

            _mappingStringToAuthCodes.Remove(authorizationCode);
            return Task.FromResult(true);
        }
    }
}
