namespace SimpleAuth.Sms
{
    using System;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.Runtime;
    using Amazon.SimpleNotificationService;
    using Amazon.SimpleNotificationService.Model;

    /// <summary>
    /// Defines the AWS SMS client.
    /// </summary>
    /// <seealso cref="SimpleAuth.Sms.ISmsClient" />
    public class AwsSmsClient : ISmsClient
    {
        private readonly string _sender;
        private readonly AmazonSimpleNotificationServiceClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="AwsSmsClient"/> class.
        /// </summary>
        /// <param name="credentials">The credentials.</param>
        /// <param name="region">The region.</param>
        /// <param name="sender">The sender.</param>
        public AwsSmsClient(AWSCredentials credentials, RegionEndpoint region, string sender)
        {
            _client = new AmazonSimpleNotificationServiceClient(credentials, region);
            _sender = sender;
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="toPhoneNumber">To phone number.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// toPhoneNumber
        /// or
        /// message
        /// </exception>
        public async Task<bool> SendMessage(string toPhoneNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(toPhoneNumber))
            {
                throw new ArgumentException(nameof(toPhoneNumber));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException(nameof(message));
            }

            var pubRequest = new PublishRequest
            {
                Message = message,
                PhoneNumber = toPhoneNumber,
                MessageAttributes =
                {
                    ["AWS.SNS.SMS.SenderID"] = new MessageAttributeValue {StringValue = _sender, DataType = "String"},
                    ["AWS.SNS.SMS.SMSType"] = new MessageAttributeValue {StringValue = "Transactional", DataType = "String"}
                }
            };

            return true;
            //var pubResponse = await _client.PublishAsync(pubRequest).ConfigureAwait(false);

            //return (int)pubResponse.HttpStatusCode < 400;
        }
    }
}