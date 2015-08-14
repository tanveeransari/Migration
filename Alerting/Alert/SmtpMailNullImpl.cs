using System.Net.Mail;

namespace Alerting.Alert
{
    public class SmtpMailNullImpl : ISmtpMail
    {
#pragma warning disable 612,618
        public void Send(MailMessage msg)
#pragma warning restore 612,618
        {
            // do nothing with email
            //SmtpMail.Send(msg);
        }

        public string SmtpServer { get; set; }
    }
}