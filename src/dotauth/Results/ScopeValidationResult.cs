namespace DotAuth.Results;

using System.Collections.Generic;
using System.Linq;

internal sealed record ScopeValidationResult
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
        Scopes = [];
    }

    public bool IsValid { get; }

    public string? ErrorMessage { get; }

    public string[] Scopes { get; }
}