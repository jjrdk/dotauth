namespace DotAuth.Shared;

/// <summary>
/// Defines the Open Id claim types.
/// </summary>
public static class OpenIdClaimTypes
{
    /// <summary>
    /// The open id scope.
    /// </summary>
    public const string OpenId = "openid";
    
    /// <summary>
    /// The offline access scope.
    /// </summary>
    public const string Offline = "offline";

    /// <summary>
    /// The subject scope.
    /// </summary>
    public const string Subject = "sub";

    /// <summary>
    /// The name scope.
    /// </summary>
    public const string Name = "name";

    /// <summary>
    /// The given name scope.
    /// </summary>
    public const string GivenName = "given_name";

    /// <summary>
    /// The family name scope.
    /// </summary>
    public const string FamilyName = "family_name";

    /// <summary>
    /// The middle name scope.
    /// </summary>
    public const string MiddleName = "middle_name";

    /// <summary>
    /// The nickname scope.
    /// </summary>
    public const string NickName = "nickname";

    /// <summary>
    /// The preferred user name scope.
    /// </summary>
    public const string PreferredUserName = "preferred_username";

    /// <summary>
    /// The profile scope.
    /// </summary>
    public const string Profile = "profile";

    /// <summary>
    /// The picture scope.
    /// </summary>
    public const string Picture = "picture";

    /// <summary>
    /// The web site scope.
    /// </summary>
    public const string WebSite = "website";

    /// <summary>
    /// The email scope.
    /// </summary>
    public const string Email = "email";

    /// <summary>
    /// The email verified scope.
    /// </summary>
    public const string EmailVerified = "email_verified";

    /// <summary>
    /// The gender scope.
    /// </summary>
    public const string Gender = "gender";

    /// <summary>
    /// The birth date scope.
    /// </summary>
    public const string BirthDate = "birthdate";

    /// <summary>
    /// The zone information scope.
    /// </summary>
    public const string ZoneInfo = "zoneinfo";

    /// <summary>
    /// The locale scope.
    /// </summary>
    public const string Locale = "locale";

    /// <summary>
    /// The phone number scope.
    /// </summary>
    public const string PhoneNumber = "phone_number";

    /// <summary>
    /// The phone number verified scope.
    /// </summary>
    public const string PhoneNumberVerified = "phone_number_verified";

    /// <summary>
    /// The address scope.
    /// </summary>
    public const string Address = "address";

    /// <summary>
    /// The updated at scope.
    /// </summary>
    public const string UpdatedAt = "updated_at";

    /// <summary>
    /// The role scope.
    /// </summary>
    public const string Role = "role";

    /// <summary>
    /// Gets all openid claim types.
    /// </summary>
    public static readonly string[] All =
    [
        Subject,
        Name,
        NickName,
        GivenName,
        FamilyName,
        MiddleName,
        Email,
        EmailVerified,
        Address,
        BirthDate,
        Gender,
        Locale,
        PhoneNumber,
        PhoneNumberVerified,
        Picture,
        PreferredUserName,
        Profile,
        Role,
        WebSite,
        ZoneInfo
    ];
}