using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Proiect___Implementare_Software.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var emailMessage = new MimeMessage();

        // Use both a display name and the email address
        emailMessage.From.Add(new MailboxAddress("CraiovaRide", _configuration["EmailSettings:SenderEmail"]));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;

        var body = new TextPart("html")
        {
            Text = message
        };

        emailMessage.Body = body;

        using (var client = new SmtpClient())
        {
            // Optional but good practice to trust valid certificates
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            // Connect to the SMTP server (Use StartTls as recommended)
            await client.ConnectAsync(
                _configuration["EmailSettings:SmtpServer"],
                int.Parse(_configuration["EmailSettings:SmtpPort"]),
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(
                _configuration["EmailSettings:SenderEmail"],
                _configuration["EmailSettings:SenderPassword"]);

            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}
