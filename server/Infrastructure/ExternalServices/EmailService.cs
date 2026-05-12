// <copyright file="EmailService.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Infrastructure.ExternalServices;

using Book2Screen.API__Web_.Configurations;
using Book2Screen.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

/// <summary>
/// Реалізація сервісу відправки пошти за допомогою MailKit.
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailOptions emailOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailService"/> class.
    /// </summary>
    /// <param name="options">Налаштування пошти.</param>
    public EmailService(IOptions<EmailOptions> options)
    {
        this.emailOptions = options.Value;
    }

    /// <inheritdoc/>
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(this.emailOptions.SenderName, this.emailOptions.SenderEmail));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = body };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(this.emailOptions.SmtpServer, this.emailOptions.SmtpPort, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(this.emailOptions.SenderEmail, this.emailOptions.SenderPassword);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
