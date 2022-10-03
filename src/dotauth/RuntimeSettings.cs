namespace DotAuth;

using System;
using DotAuth.Shared.Models;

/// <summary>
/// Defines the runtime settings.
/// </summary>
public sealed class RuntimeSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeSettings"/> class.
    /// </summary>
    /// <param name="salt">The hashing salt.</param>
    /// <param name="onResourceOwnerCreated"></param>
    /// <param name="authorizationCodeValidityPeriod">The authorization code validity period.</param>
    /// <param name="claimsIncludedInUserCreation">The claims included in user creation.</param>
    /// <param name="rptLifeTime">The RPT life time.</param>
    /// <param name="patLifeTime">The PAT life time.</param>
    /// <param name="ticketLifeTime">The ticket life time.</param>
    /// <param name="devicePollingInterval">The device polling interval.</param>
    /// <param name="deviceAuthorizationLifetime">The device authorization request lifetime.</param>
    /// <param name="allowHttp">Sets whether to allow insecure requests.</param>
    /// <param name="redirectToLogin">Flag to determine whether to redirect home screen to login screen.</param>
    public RuntimeSettings(
        string salt = "",
        Action<ResourceOwner>? onResourceOwnerCreated = null,
        TimeSpan authorizationCodeValidityPeriod = default,
        string[]? claimsIncludedInUserCreation = null,
        TimeSpan rptLifeTime = default,
        TimeSpan patLifeTime = default,
        TimeSpan ticketLifeTime = default,
        TimeSpan devicePollingInterval = default,
        TimeSpan deviceAuthorizationLifetime = default,
        bool allowHttp = false,
        bool redirectToLogin = false)
    {
        DevicePollingInterval = devicePollingInterval == default ? TimeSpan.FromSeconds(5) : devicePollingInterval;
        DeviceAuthorizationLifetime = deviceAuthorizationLifetime == default ? TimeSpan.FromSeconds(1800) : deviceAuthorizationLifetime;
        AllowHttp = allowHttp;
        Salt = salt;
        PatLifeTime = patLifeTime;
        RedirectToLogin = redirectToLogin;
        OnResourceOwnerCreated = onResourceOwnerCreated ?? (r => { });
        AuthorizationCodeValidityPeriod = authorizationCodeValidityPeriod == default
            ? TimeSpan.FromHours(1)
            : authorizationCodeValidityPeriod;
        RptLifeTime = rptLifeTime == default ? TimeSpan.FromHours(1) : rptLifeTime;
        TicketLifeTime = ticketLifeTime == default ? TimeSpan.FromHours(1) : ticketLifeTime;
        ClaimsIncludedInUserCreation = claimsIncludedInUserCreation ?? Array.Empty<string>();
    }

    /// <summary>
    /// Gets the delegate to run when resource owner created.
    /// </summary>
    /// <value>
    /// The on resource owner created.
    /// </value>
    public Action<ResourceOwner> OnResourceOwnerCreated { get; }

    /// <summary>
    /// Gets the authorization code validity period.
    /// </summary>
    /// <value>
    /// The authorization code validity period.
    /// </value>
    public TimeSpan AuthorizationCodeValidityPeriod { get; }

    /// <summary>
    /// Gets an array of claims include when the resource owner is created.
    /// If the list is empty then all the claims are included.
    /// </summary>
    public string[] ClaimsIncludedInUserCreation { get; }

    /// <summary>
    /// Gets the RPT lifetime.
    /// </summary>
    public TimeSpan RptLifeTime { get; }

    /// <summary>
    /// Gets the hashing salt.
    /// </summary>
    public string Salt { get; }

    /// <summary>
    /// Gets the PAT lifetime.
    /// </summary>
    public TimeSpan PatLifeTime { get; }

    /// <summary>
    /// Gets whether home screen redirects to login.
    /// </summary>
    public bool RedirectToLogin { get; }

    /// <summary>
    /// Gets the ticket lifetime (seconds).
    /// </summary>
    public TimeSpan TicketLifeTime { get; }

    /// <summary>
    /// Gets whether to allow insecure requests.
    /// </summary>
    public bool AllowHttp { get; }

    /// <summary>
    /// Gets the polling interval for device authorizations.
    /// </summary>
    public TimeSpan DevicePollingInterval { get; }

    /// <summary>
    /// Gets the device authorization request lifetime.
    /// </summary>
    public TimeSpan DeviceAuthorizationLifetime { get; }
}