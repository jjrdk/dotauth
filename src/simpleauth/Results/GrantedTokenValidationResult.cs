namespace SimpleAuth.Results
{
    internal record GrantedTokenValidationResult
    {
        public bool IsValid { get; init; }

        public string? MessageErrorCode { get; init; }

        public string? MessageErrorDescription { get; init; }
    }
}