namespace FoodExpress.Services.Services.Interfaces
{
    public interface IEmailService
    {
        void SendEmail(string to, string subject, string body);
        void SendPasswordResetEmail(string to, string resetPath);
    }
}