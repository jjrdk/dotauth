namespace DotAuth.Results;

/// <summary>
/// Defines the types of action results.
/// </summary>
public enum ActionResultType
{

    /// <summary>
    /// None
    /// </summary>
    None = 0,

    /// <summary>
    /// Redirect to action
    /// </summary>
    RedirectToAction = 1,

    /// <summary>
    /// Redirect to call back URL
    /// </summary>
    RedirectToCallBackUrl = 2,

    /// <summary>
    /// Output
    /// </summary>
    Output = 3,

    /// <summary>
    /// Bad request
    /// </summary>
    BadRequest = 4
}