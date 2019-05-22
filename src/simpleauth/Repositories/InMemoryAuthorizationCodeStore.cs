namespace SimpleAuth.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    internal sealed class InMemoryAuthorizationCodeStore : IAuthorizationCodeStore
    {
        private static Dictionary<string, AuthorizationCode> _mappingStringToAuthCodes;

        public InMemoryAuthorizationCodeStore()
        {
            _mappingStringToAuthCodes = new Dictionary<string, AuthorizationCode>();
        }

        public Task<AuthorizationCode> Get(string code, CancellationToken cancellationToken)
        {
            return !_mappingStringToAuthCodes.ContainsKey(code)
                ? Task.FromResult((AuthorizationCode) null)
                : Task.FromResult(_mappingStringToAuthCodes[code]);
        }

        public Task<bool> Add(AuthorizationCode authorizationCode, CancellationToken cancellationToken)
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

        public Task<bool> Remove(string authorizationCode, CancellationToken cancellationToken)
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
