using System.Net.Mail;

namespace Alerting.Alert
{
    public interface IEmailHelper
    {
        bool SendMail(MailMessage mailMessage);
        MailMessage BuildEmailMessage(string message, string subject, bool isHTML);
        MailMessage BuildEmailMessage(string message, bool isHTML);

        MailMessage BuildEmailMessage(string message, string subject, bool isHTML, bool  showProcessInfo);
        MailMessage BuildEmailMessage(string message, bool isHTML, bool  showProcessInfo);

        string BuildProcessInfo(bool isHTML, bool showProcessInfo);
    }
}