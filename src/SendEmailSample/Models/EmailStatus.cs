namespace Aws.Lambda.Models
{
    public class EmailStatus
    {
        public EmailStatus() { }

        public EmailStatus(bool isVerified, string errorMessage)
        {
            IsVerified = isVerified;
            ErrorMessage = errorMessage;
        }

        public bool IsVerified { get; set; }
        public string ErrorMessage { get; set; }
    }
}
