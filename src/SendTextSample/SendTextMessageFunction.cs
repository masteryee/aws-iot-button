/**
 * This is a sample Lambda function that sends an SMS on click of a
 * button. It needs one permission sns:Publish. The following policy
 * allows SNS publish to SMS but not topics or endpoints.
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "sns:Publish"
            ],
            "Resource": [
                "*"
            ]
        },
        {
            "Effect": "Deny",
            "Action": [
                "sns:Publish"
            ],
            "Resource": [
                "arn:aws:sns:*:*:*"
            ]
        }
    ]
}
 *
 * The following JSON template shows what is sent as the payload:
{
    "serialNumber": "GXXXXXXXXXXXXXXXXX",
    "batteryVoltage": "xxmV",
    "clickType": "SINGLE" | "DOUBLE" | "LONG"
}
 *
 * A "LONG" clickType is sent if the first press lasts longer than 1.5 seconds.
 * "SINGLE" and "DOUBLE" clickType payloads are sent for short clicks.
 *
 * For more documentation, follow the link below.
 * http://docs.aws.amazon.com/iot/latest/developerguide/iot-lambda-rule.html
 */

using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Aws.Lambda.Models;
using Newtonsoft.Json;

namespace Aws.Lambda
{
    public class SendTextMessageFunction
    {
        private readonly AmazonSimpleNotificationServiceClient _sns = new AmazonSimpleNotificationServiceClient();
        private const string PnoneNumberEnvironmentKey = "PhoneNumber";

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public Task ButtonHandler(IotButtonEvent buttonEvent, ILambdaContext context)
        {
            var phoneNumber = Environment.GetEnvironmentVariable(PnoneNumberEnvironmentKey);
            var payload = JsonConvert.SerializeObject(buttonEvent);
            Console.WriteLine($"Received event: {payload}");

            Console.WriteLine($"Sending SMS to {phoneNumber}");
            var request = new PublishRequest
            {
                PhoneNumber = phoneNumber,
                Message = $"Hello from your IoT Button ${buttonEvent.SerialNumber}. Here is the full event: ${payload}."
            };

            return _sns.PublishAsync(request);
        }
    }
}
