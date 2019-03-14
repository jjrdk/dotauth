namespace SimpleAuth.Sms
{
    using System;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.Runtime;
    using Amazon.SimpleNotificationService;
    using Amazon.SimpleNotificationService.Model;

    public class AwsSmsClient : ISmsClient
    {
        private readonly string _sender;
        private readonly AmazonSimpleNotificationServiceClient _client;

        public AwsSmsClient(AWSCredentials credentials, RegionEndpoint region, string sender)
        {
            _client = new AmazonSimpleNotificationServiceClient(credentials, region);
            _sender = sender;
        }

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
            var pubResponse = await _client.PublishAsync(pubRequest).ConfigureAwait(false);

            return (int)pubResponse.HttpStatusCode < 400;
        }
    }
}