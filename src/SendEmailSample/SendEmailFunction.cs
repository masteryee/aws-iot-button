/**
 * This is a sample Lambda function that sends an email on click of a
 * button. It requires these SES permissions.
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "ses:GetIdentityVerificationAttributes",
                "ses:SendEmail",
                "ses:VerifyEmailIdentity"
            ],
            "Resource": "*"
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
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Aws.Lambda.Models;
using Newtonsoft.Json;

namespace Aws.Lambda
{
    public class SendEmailFunction
    {
        private readonly AmazonSimpleEmailServiceClient _ses = new AmazonSimpleEmailServiceClient();
        private const string RecipientEnvironmentKey = "Recipient";
        private const string VerificationEmailWarning = "Verification email sent. Please verify it.";

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task ButtonHandler(IotButtonEvent buttonEvent, ILambdaContext context)
        {
            var email = Environment.GetEnvironmentVariable(RecipientEnvironmentKey);
            var payload = JsonConvert.SerializeObject(buttonEvent);
            Console.WriteLine($"Received event: {payload}");

            var emailStatus = await CheckEmailAsync(email);
            if (!emailStatus.IsVerified)
            {
                Console.WriteLine($"Failed to check email: {email}. {emailStatus.ErrorMessage}");
                return;
            }

            var subject = $"Hello from your IoT Button {buttonEvent.SerialNumber}";
            var body = $"Hello from your IoT Button {buttonEvent.SerialNumber}. Here is the full event: {payload}.";

            var request = new SendEmailRequest
            {
                Source = email,
                Destination = new Destination(new List<string> { email }),
                Message = new Message(new Content(subject), new Body(new Content(body)))
            };
            await _ses.SendEmailAsync(request);
        }

        private async Task<EmailStatus> SendVerificationAsync(string email)
        {
            var request = new VerifyEmailIdentityRequest
            {
                EmailAddress = email
            };
            await _ses.VerifyEmailIdentityAsync(request);
            return new EmailStatus(false, VerificationEmailWarning);
        }

        private async Task<EmailStatus> CheckEmailAsync(string email)
        {
            var request = new GetIdentityVerificationAttributesRequest
            {
                Identities = new List<string> { email }
            };
            var response = await _ses.GetIdentityVerificationAttributesAsync(request);

            var attributes = response.VerificationAttributes;
            IdentityVerificationAttributes verificationAttributes;
            if (attributes.TryGetValue(email, out verificationAttributes))
            {
                if (verificationAttributes.VerificationStatus == VerificationStatus.Success)
                {
                    return new EmailStatus(true, string.Empty);
                }
            }

            return await SendVerificationAsync(email);
        }
    }
}
