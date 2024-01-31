using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Toolkit.Uwp.Notifications;
using MimeKit;
using Org.BouncyCastle.Asn1.X509;
using System.Diagnostics;
using System.Windows;
using Windows.Foundation.Collections;

namespace MailNotifier.MVVM.Model
{
    internal class NotificationHandler
    {
        public static async Task ReplyAsync(ValueSet userInput, ToastArguments args)
        {
            //obtain parameters from the notification
            string receivedMessageId = args["receivedMessageId"];
            string replyText = (string)userInput["tbReply"];

            JSONReader reader = new();
            reader.ReadJson();

            string username = reader.Username;
            string password = reader.Password;
            string email = reader.Email;

            ImapClient client = new();
            try
            {
                //connect
                await client.ConnectAsync("imap.gmail.com", 993, true);
                await client.AuthenticateAsync(username, password);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }

            await client.Inbox.OpenAsync(FolderAccess.ReadOnly);

            //get recieved message
            var uId = await client.Inbox.SearchAsync(SearchQuery.HeaderContains("Message-Id", receivedMessageId));
            var message = await client.Inbox.GetMessageAsync(uId.First());

            //disconnect
            await client.Inbox.CloseAsync();
            await client.DisconnectAsync(true);

            MailboxAddress mailboxAddress = new("Mailnotifier", email);

            //create reply message
            MimeMessage replyMessage = MailWorker.GetReplyMessage(message, mailboxAddress, replyText);
            await MailWorker.SendMessageAsync(replyMessage, username, password);
        }

        public static void View()
        {
            try
            {
                Process.Start(new ProcessStartInfo() {
                    FileName = "https://mail.google.com/mail/",
                    UseShellExecute = true
                });
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message + "\n" + "NO BROWSER!!!!!!");
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }
    }
}
