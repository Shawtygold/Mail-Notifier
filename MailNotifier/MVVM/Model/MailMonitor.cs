using MailKit;
using MailKit.Net.Imap;
using Microsoft.Toolkit.Uwp.Notifications;
using MimeKit;
using System.Timers;
using System.Windows;

namespace MailNotifier.MVVM.Model
{
    internal class MailMonitor
    {
        private readonly string _username = null!;
        private readonly string _password = null!;
        private System.Timers.Timer Timer { get; set; }
        private IMailFolder Inbox { get; set; } = null!;
        private ImapClient ImapClient { get; set; } = null!;
        private int LastMessageCount { get; set; } //The field that stores the number of messages at the time of the last check

        public MailMonitor()
        {
            Timer = new(2000);
            Timer.AutoReset = true;
            Timer.Elapsed += Timer_Elapsed;

            JSONReader reader = new();
            reader.ReadJson();

            _username = reader.Username;
            _password = reader.Password;

            if (!Setup())
                return;
        }

        private bool Setup()
        {
            ImapClient = new ImapClient();

            if (!Connect(_username, _password))
                return false;

            Inbox = ImapClient.Inbox;

            Inbox.Open(FolderAccess.ReadOnly);
            LastMessageCount = Inbox.Count;
            Inbox.Close();

            return true;
        }
        private bool Connect(string username, string password)
        {
            if (ImapClient == null)
                return false;

            bool result = false;

            try
            {
                ImapClient.Connect("imap.gmail.com", 993, true);
                ImapClient.Authenticate(username, password);
                result = true;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }

            return result;
        }
        public async Task StartMonitoringAsync()
        {
            if (Timer == null)
                return;

            if (!ImapClient.IsConnected)
                if (!await ConnectAsync())
                    return;

            Timer.Start();
        }
        public async Task StopMonitoringAsync()
        {
            if (Timer == null)
                return;

            Timer.Stop();
            await DisconnectAsync();
        }
        private async Task<bool> ConnectAsync()
        {
            return await Task.Run(() => Connect(_username, _password));
        }
        private async Task DisconnectAsync()
        {
            if (ImapClient == null)
                return;

            if (ImapClient.IsConnected)
                await ImapClient.DisconnectAsync(true);
        }
        private async Task<MimeMessage?> GetNewMessageAsync()
        {
            if (Inbox == null)
                return null;

            await Inbox.OpenAsync(FolderAccess.ReadOnly);

            int inboxCount = Inbox.Count;

            MimeMessage? message = null;

            //if the number of emails in inbox has increased
            if (LastMessageCount < inboxCount)
                message = await Inbox.GetMessageAsync(inboxCount - 1);

            LastMessageCount = inboxCount;

            await Inbox.CloseAsync();

            return message;
        }
        private async void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            //getting new message
            var newMessage = await GetNewMessageAsync();

            if (newMessage == null)
                return;

            //sending notification
            var notify = new ToastContentBuilder();
            notify.AddText(newMessage.Subject);
            notify.AddAppLogoOverride(new Uri(@"C:\Users\user\source\repos\MailNotifier\MailNotifier\Resources\MailNotifierNotifyIcon.png"), ToastGenericAppLogoCrop.Circle);
            notify.AddArgument("action", "view");
            notify.AddText(newMessage.TextBody);
            notify.AddButton(new ToastButton().SetContent("View").AddArgument("action", "view").SetBackgroundActivation());
            notify.AddButton(new ToastButton().SetContent("Reply").AddArgument("action", "reply").SetBackgroundActivation());
            notify.AddArgument("receivedMessageId", newMessage.MessageId);
            notify.AddInputTextBox("tbReply", placeHolderContent: "Type a response");
            notify.Show();
        }
    }
}
