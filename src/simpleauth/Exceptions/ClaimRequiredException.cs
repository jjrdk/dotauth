namespace SimpleAuth.Exceptions
{
    public class ClaimRequiredException : SimpleAuthException
    {
        public ClaimRequiredException(string claim)
        {
            Claim = claim;
        }

        public string Claim { get; }
    }
}
