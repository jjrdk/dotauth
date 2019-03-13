namespace SimpleAuth.Shared.DTOs
{
    using System;

    /// <summary>
    /// Defines the confirmation code.
    /// </summary>
    public class ConfirmationCode
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the issue at.
        /// </summary>
        /// <value>
        /// The issue at.
        /// </value>
        public DateTime IssueAt { get; set; }

        /// <summary>
        /// Gets or sets the expires in.
        /// </summary>
        /// <value>
        /// The expires in.
        /// </value>
        public double ExpiresIn { get; set; }
    }
}
