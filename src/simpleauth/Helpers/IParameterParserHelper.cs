namespace SimpleAuth.Helpers
{
    using System.Collections.Generic;
    using Parameters;

    public interface IParameterParserHelper
    {
        /// <summary>
        /// Parse the parameter and returns a list of prompt parameter.
        /// </summary>
        /// <param name="parameter">List of prompts separated by whitespace</param>
        /// <returns>List of prompts.</returns>
        ICollection<PromptParameter> ParsePrompts(string parameter);
        /// <summary>
        /// Parse the parameter and returns a list of response types
        /// </summary>
        /// <param name="parameter">List of response types separated by whitespace</param>
        /// <returns>List of response types</returns>
        ICollection<string> ParseResponseTypes(string parameter);
        /// <summary>
        /// Parse the parameter and returns a list of scopes.
        /// </summary>
        /// <param name="scope">Parameter to parse.</param>
        /// <returns>list of scopes or null</returns>
        ICollection<string> ParseScopes(string parameter);
        // List<string> ParseScopeParametersAndGetAllScopes(string concatenateListOfScopes);
    }
}