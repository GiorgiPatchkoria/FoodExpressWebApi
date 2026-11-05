using FoodExpress.Services.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace FoodExpress.Services.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void SendPasswordResetEmail(string to, string resetPath)
        {
            var baseUrl = _configuration["PasswordReset:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException("Configure PasswordReset:BaseUrl for password reset emails.");
            }

            var resetLink = baseUrl.TrimEnd('/') + resetPath;
            var safeResetLink = WebUtility.HtmlEncode(resetLink);
            var body = $"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="utf-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1">
                    <title>Reset your FoodExpress password</title>
                </head>
                <body style="margin:0;padding:0;background-color:#faf9f6;font-family:Arial,Helvetica,sans-serif;color:#262a26;">
                    <div style="display:none;max-height:0;overflow:hidden;mso-hide:all;">
                        A fresh start for your FoodExpress account. Reset your password within 30 minutes.
                    </div>
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:#faf9f6;">
                        <tr>
                            <td align="center" style="padding:32px 16px;">
                                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width:560px;">
                                    <tr>
                                        <td style="padding:0 8px 24px;font-size:24px;font-weight:700;letter-spacing:-1px;color:#2f4c3b;">
                                            Food<span style="color:#e95632;">Express</span>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="padding:36px 28px;background-color:#2f4c3b;border-radius:16px 16px 0 0;">
                                            <p style="margin:0 0 12px;color:#e5ebdf;font-size:12px;font-weight:700;letter-spacing:2px;">YOUR ACCOUNT</p>
                                            <h1 style="margin:0;color:#ffffff;font-size:30px;line-height:1.25;">A fresh start.<br>A new password.</h1>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="padding:32px 28px;background-color:#ffffff;border:1px solid #e4e5dd;border-top:0;border-radius:0 0 16px 16px;">
                                            <p style="margin:0 0 16px;font-size:16px;line-height:1.7;">Hi there,</p>
                                            <p style="margin:0 0 24px;font-size:16px;line-height:1.7;color:#555b53;">
                                                We received a request to reset your FoodExpress password. Choose a new one below to get back to your favorite food.
                                            </p>
                                            <table role="presentation" cellspacing="0" cellpadding="0" border="0">
                                                <tr>
                                                    <td align="center" bgcolor="#e95632" style="border-radius:8px;">
                                                        <a href="{safeResetLink}" style="display:inline-block;padding:16px 28px;border:1px solid #e95632;border-radius:8px;color:#ffffff;font-size:16px;font-weight:700;text-decoration:none;mso-padding-alt:0;">
                                                            Reset my password
                                                        </a>
                                                    </td>
                                                </tr>
                                            </table>
                                            <p style="margin:20px 0 24px;font-size:13px;line-height:1.6;color:#72766e;">This link expires in <strong style="color:#2f4c3b;">30 minutes</strong>. Please keep it private.</p>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align="center" style="padding:24px 16px;font-size:12px;line-height:1.8;color:#72766e;">
                                            <strong style="color:#2f4c3b;">FoodExpress</strong><br>
                                            Good food. Just a few clicks away.<br>
                                            This is an automated account email.
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>
                """;

            SendEmail(to, "Reset your FoodExpress password", body);
        }

        public void SendEmail(string to, string subject, string body)
        {
            try
            {
                var senderEmail = _configuration["EmailSettings:SenderEmail"]!;
                var appPassword = _configuration["EmailSettings:SenderPassword"];

                using var smtpClient = new SmtpClient("smtp.gmail.com",587);
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(senderEmail, appPassword);
                smtpClient.EnableSsl = true;

                using var mailMessage = new MailMessage(senderEmail, to, subject, body);
                mailMessage.IsBodyHtml = true;
                mailMessage.BodyEncoding = Encoding.UTF8;
                mailMessage.SubjectEncoding = Encoding.UTF8;

                smtpClient.Send(mailMessage);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to send email to {To} with subject {Subject}.", to, subject);
            }
        }
    }
}