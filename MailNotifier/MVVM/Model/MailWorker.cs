using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.IO;
using System.Windows;

namespace MailNotifier.MVVM.Model
{
    internal class MailWorker
    {
        public static MimeMessage GetReplyMessage(MimeMessage message, MailboxAddress from, string textBody)
        {
            var reply = new MimeMessage();
            reply.From.Add(from);

            if (message.ReplyTo.Count > 0)
            {
                reply.To.AddRange(message.ReplyTo);
            }
            else if (message.From.Count > 0)
            {
                reply.To.AddRange(message.From);
            }
            else if (message.Sender != null)
            {
                reply.To.Add(message.Sender);
            }

            if(!message.Subject.StartsWith("Re: "))
                reply.Subject = "Re: " + message.Subject;
            else 
                reply.Subject = message.Subject;

            if (!string.IsNullOrEmpty(message.MessageId))
            {
                reply.InReplyTo = message.MessageId;
                foreach (var id in message.References)
                    reply.References.Add(id);
                reply.References.Add(message.MessageId);
            }

            using (var quoted = new StringWriter())
            {
                var sender = message.Sender ?? message.From.Mailboxes.FirstOrDefault();
                var name = sender != null ? (!string.IsNullOrEmpty(sender.Name) ? sender.Name : sender.Address) : "someone";

                quoted.WriteLine("На {0}, {1} пишет:", message.Date.ToString("f"), name);
                using (var reader = new StringReader(message.TextBody))
                {
                    string? line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        quoted.Write("> ");
                        quoted.WriteLine(line);
                    }
                }

                reply.Body = new TextPart("plain")
                {
                    Text = quoted.ToString() + textBody
                };
            }

            return reply;
        }

        public async static Task SendMessageAsync(MimeMessage message, string username, string password)
        {
            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
