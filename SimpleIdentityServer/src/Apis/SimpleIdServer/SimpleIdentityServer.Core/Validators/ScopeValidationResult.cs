namespace SimpleIdentityServer.Core.Validators
{
    using System.Collections.Generic;

    public class ScopeValidationResult
    {
        public ScopeValidationResult(bool isValid)
        {
            IsValid = isValid;
        }

        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public ICollection<string> Scopes { get; set; }
    }
}