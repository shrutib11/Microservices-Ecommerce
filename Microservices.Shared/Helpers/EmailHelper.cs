using MailKit.Net.Smtp;
using Microservices.Shared.Models;
using Microsoft.Extensions.Configuration;
using MimeKit;
using RazorLight;

namespace Microservices.Shared.Helpers
{
    public class EmailHelper
    {
        private readonly IConfiguration _configuration;
        private readonly RazorLightEngine _razorEngine;

        public EmailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            _razorEngine = new RazorLightEngineBuilder()
                .UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(), "Templates"))
                .UseMemoryCachingProvider()
                .Build();
        }

        private async Task<string> GenerateEmailBodyAsync(EmailTemplateViewModel model)
        {
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates","EmailTemplate.cshtml");;
            return await _razorEngine.CompileRenderAsync("EmailTemplate.cshtml", model);
        }

        public async Task SendEmailInstanceAsync(string toEmail, string subject, EmailTemplateViewModel model)
        {
            string emailBody = await GenerateEmailBodyAsync(model);

            var emailMessage = new MimeMessage();
            // string a1 = (_configuration["SENDERNAME"]);
            // string ae = (_configuration["SENDEREMAIL"]);
            // string a2 = (_configuration["SERVER"]);
            // string a4 = (_configuration["PORT"]);
            // string a5 = (_configuration["USERNAME"]);
            // string a6 = (_configuration["PASSWORD"]);
            emailMessage.From.Add(new MailboxAddress("supportdesk", "test.dotnet@etatvasoft.com"));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = emailBody
            };

    

            emailMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await client.ConnectAsync(
                "mail.etatvasoft.com",
                int.Parse("465" ?? "587"),
                MailKit.Security.SecureSocketOptions.SslOnConnect
            );

            await client.AuthenticateAsync(
                 "test.dotnet@etatvasoft.com",
                "P}N^{z-]7Ilp"
            );

            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }

        // ✅ Only this is static, requires everything to be passed
        public static async Task SendEmailAsync(string toEmail, string subject, EmailTemplateViewModel model, IConfiguration configuration)
        {
            var helper = new EmailHelper(configuration);
            await helper.SendEmailInstanceAsync(toEmail, subject, model);
        }
    }
}
