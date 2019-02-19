namespace SimpleAuth.Sms
{
    using System;

    /// <summary>
    /// Defines the SMS exception.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class SmsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public SmsException(string message, Exception innerException) : base(message, innerException) { }
    }
}
