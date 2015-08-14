using System.Net.Mail;

namespace Alerting.Alert
{
    public interface ISmtpMail
    {
#pragma warning disable 612,618
        void Send(MailMessage msg);
#pragma warning restore 612,618
        string SmtpServer { set; }
    }
}