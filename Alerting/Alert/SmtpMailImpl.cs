using System.Net.Mail;

namespace Alerting.Alert
{
#pragma warning disable 612,618
    public class SmtpMailImpl : ISmtpMail
    {
        SmtpClient client;

        public void Send(MailMessage msg)
        {
            if (client != null)
                client.Send(msg);
        }

        public string SmtpServer
        {
            set
            {
                client = new SmtpClient(value);
            }
        }
    }
#pragma warning restore 612,618
}