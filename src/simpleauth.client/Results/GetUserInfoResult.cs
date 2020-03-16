namespace SimpleAuth.Client.Results
{
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Defines the get user info result.
    /// </summary>
    /// <seealso cref="SimpleAuth.Client.Results.BaseSidResult" />
    public class GetUserInfoResult : BaseSidResult
    {
        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public JObject Content { get; set; }

        /// <summary>
        /// Gets or sets the JWT token.
        /// </summary>
        /// <value>
        /// The JWT token.
        /// </value>
        public string JwtToken { get; set; }
    }
}
