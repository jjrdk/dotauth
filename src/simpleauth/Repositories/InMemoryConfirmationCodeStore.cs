namespace SimpleAuth.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Repositories;

    internal sealed class InMemoryConfirmationCodeStore : IConfirmationCodeStore
    {
        private readonly ICollection<ConfirmationCode> _confirmationCodes;

        public InMemoryConfirmationCodeStore()
        {
            _confirmationCodes = new List<ConfirmationCode>();
        }

        public Task<bool> Add(ConfirmationCode confirmationCode, CancellationToken cancellationToken)
        {
            if (confirmationCode == null)
            {
                throw new ArgumentNullException(nameof(confirmationCode));
            }

            _confirmationCodes.Add(confirmationCode);
            return Task.FromResult(true);
        }

        public Task<ConfirmationCode> Get(string code, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            return Task.FromResult(_confirmationCodes.FirstOrDefault(c => c.Value == code));
        }

        public Task<bool> Remove(string code, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            var confirmationCode = _confirmationCodes.FirstOrDefault(c => c.Value == code);
            if (confirmationCode == null)
            {
                return Task.FromResult(false);
            }

            _confirmationCodes.Remove(confirmationCode);
            return Task.FromResult(true);
        }
    }
}
