using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Threading.Tasks;

namespace InventoryApp.Services
{
    public class LocalEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("[Fake Email] Warning: no email address provided.");
                return Task.CompletedTask;
            }

            if (string.IsNullOrWhiteSpace(subject))
                subject = "(no subject)";

            Console.WriteLine($"[Fake Email] To: {email}, Subject: {subject}");
            Console.WriteLine($"[Fake Email] Body: {(htmlMessage ?? "(no content)")}");

            return Task.CompletedTask;
        }
    }
}
