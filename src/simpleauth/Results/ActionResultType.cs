namespace SimpleAuth.Results
{
    /// <summary>
    /// Defines the types of action results.
    /// </summary>
    public enum ActionResultType
    {
        /// <summary>
        /// Redirect to action
        /// </summary>
        RedirectToAction = 0,
        /// <summary>
        /// Redirect to call back URL
        /// </summary>
        RedirectToCallBackUrl,
        /// <summary>
        /// Output
        /// </summary>
        Output,
        /// <summary>
        /// None
        /// </summary>
        None
    }
}