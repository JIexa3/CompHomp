using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CompHomp.Models;

namespace CompHomp.Services
{
    public class EmailService
    {
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SmtpUsername = "lexinskii2281@gmail.com"; 
        private const string SmtpPassword = "flah obip iuuv fiyw"; 

        public async Task SendVerificationCodeAsync(string toEmail, string verificationCode)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("CompHomp", SmtpUsername));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = "Код подтверждения регистрации";

                message.Body = new TextPart("plain")
                {
                    Text = $"Ваш код подтверждения для регистрации в CompHomp: {verificationCode}"
                };

                using var client = new SmtpClient();
                await client.ConnectAsync(SmtpServer, SmtpPort, false);
                await client.AuthenticateAsync(SmtpUsername, SmtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при отправке email: {ex.Message}");
            }
        }

        public async Task SendBuildStatusChangeNotificationAsync(string toEmail, string buildName, BuildStatus newStatus)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("CompHomp", SmtpUsername));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = "Изменение статуса сборки";

                var statusText = newStatus switch
                {
                    BuildStatus.Pending => "На рассмотрении",
                    BuildStatus.Approved => "Одобрено",
                    BuildStatus.Rejected => "Отклонено",
                    _ => newStatus.ToString()
                };

                message.Body = new TextPart("plain")
                {
                    Text = $"Уважаемый пользователь,\n\n" +
                          $"Статус вашей сборки \"{buildName}\" был изменен на: {statusText}.\n\n" +
                          $"С уважением,\nКоманда CompHomp"
                };

                using var client = new SmtpClient();
                await client.ConnectAsync(SmtpServer, SmtpPort, false);
                await client.AuthenticateAsync(SmtpUsername, SmtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при отправке уведомления о статусе: {ex.Message}");
            }
        }

        public async Task SendBuildComponentsChangeNotificationAsync(string toEmail, string buildName, 
            Dictionary<string, (string OldComponent, string NewComponent)> changes)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("CompHomp", SmtpUsername));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = "Изменение комплектующих в сборке";

                var text = $"Уважаемый пользователь,\n\n" +
                          $"В вашей сборке \"{buildName}\" были произведены следующие изменения:\n\n";

                foreach (var change in changes)
                {
                    text += $"- {change.Key}: {change.Value.OldComponent} → {change.Value.NewComponent}\n";
                }

                text += "\nС уважением,\nКоманда CompHomp";

                message.Body = new TextPart("plain") { Text = text };

                using var client = new SmtpClient();
                await client.ConnectAsync(SmtpServer, SmtpPort, false);
                await client.AuthenticateAsync(SmtpUsername, SmtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при отправке уведомления об изменениях: {ex.Message}");
            }
        }
    }
}
