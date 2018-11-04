namespace SimpleIdentityServer.Core.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ScopeValidationResult
    {
        public ScopeValidationResult(IEnumerable<string> scopes)
        {
            IsValid = true;
            Scopes = scopes.ToArray();
        }

        public ScopeValidationResult(string errorMessage)
        {
            IsValid = false;
            ErrorMessage = errorMessage;
            Scopes = Array.Empty<string>();
        }

        public bool IsValid { get; }
        public string ErrorMessage { get; }
        public ICollection<string> Scopes { get; }
    }
}