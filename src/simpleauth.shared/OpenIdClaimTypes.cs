namespace SimpleAuth.Shared
{
    /// <summary>
    /// Defines the Open Id claim types.
    /// </summary>
    public static class OpenIdClaimTypes
    {
        /// <summary>
        /// The subject
        /// </summary>
        public const string Subject = "sub";

        /// <summary>
        /// The name
        /// </summary>
        public const string Name = "name";

        /// <summary>
        /// The given name
        /// </summary>
        public const string GivenName = "given_name";

        /// <summary>
        /// The family name
        /// </summary>
        public const string FamilyName = "family_name";

        /// <summary>
        /// The middle name
        /// </summary>
        public const string MiddleName = "middle_name";

        /// <summary>
        /// The nick name
        /// </summary>
        public const string NickName = "nickname";

        /// <summary>
        /// The preferred user name
        /// </summary>
        public const string PreferredUserName = "preferred_username";

        /// <summary>
        /// The profile
        /// </summary>
        public const string Profile = "profile";

        /// <summary>
        /// The picture
        /// </summary>
        public const string Picture = "picture";

        /// <summary>
        /// The web site
        /// </summary>
        public const string WebSite = "website";

        /// <summary>
        /// The email
        /// </summary>
        public const string Email = "email";

        /// <summary>
        /// The email verified
        /// </summary>
        public const string EmailVerified = "email_verified";

        /// <summary>
        /// The gender
        /// </summary>
        public const string Gender = "gender";

        /// <summary>
        /// The birth date
        /// </summary>
        public const string BirthDate = "birthdate";

        /// <summary>
        /// The zone information
        /// </summary>
        public const string ZoneInfo = "zoneinfo";

        /// <summary>
        /// The locale
        /// </summary>
        public const string Locale = "locale";

        /// <summary>
        /// The phone number
        /// </summary>
        public const string PhoneNumber = "phone_number";

        /// <summary>
        /// The phone number verified
        /// </summary>
        public const string PhoneNumberVerified = "phone_number_verified";

        /// <summary>
        /// The address
        /// </summary>
        public const string Address = "address";

        /// <summary>
        /// The updated at
        /// </summary>
        public const string UpdatedAt = "updated_at";

        /// <summary>
        /// The role
        /// </summary>
        public const string Role = "role";

        public static readonly string[] All = new[]
        {
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
        };
    }
}