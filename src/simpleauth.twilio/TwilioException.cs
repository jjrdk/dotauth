namespace SimpleAuth.Twilio
{
    using System;

    public class TwilioException : Exception
    {
        public TwilioException(string message) : base(message) { }
    }
}
