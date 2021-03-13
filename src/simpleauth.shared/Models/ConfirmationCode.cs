namespace SimpleAuth.Shared.Models
{
    using System;

    /// <summary>
    /// Defines the confirmation code.
    /// </summary>
    public record ConfirmationCode
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; init; } = null!;

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public string? Subject { get; init; }

        /// <summary>
        /// Gets or sets the issue at.
        /// </summary>
        /// <value>
        /// The issue at.
        /// </value>
        public DateTimeOffset IssueAt { get; init; }

        /// <summary>
        /// Gets or sets the expires in.
        /// </summary>
        /// <value>
        /// The expires in.
        /// </value>
        public double ExpiresIn { get; init; }
    }
}
