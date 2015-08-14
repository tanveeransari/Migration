using System;
using System.Net.Mail;
using Alerting.Alert.Formatters;

namespace Alerting.Alert
{
    public class EmailHelperNullImpl : IEmailHelper
    {
        public bool SendMail(MailMessage mailMessage)
        {
            return true;
        }

        public MailMessage BuildEmailMessage(string message, string subject, bool isHTML)
        {
            return new MailMessage();
        }

        public MailMessage BuildEmailMessage(string message, bool isHTML)
        {
            return new MailMessage();
        }

        public MailMessage BuildEmailMessage(string message, string subject, bool isHTML, bool showProcessInfo)
        {
            return new MailMessage();
        }

        public MailMessage BuildEmailMessage(string message, bool isHTML, bool showProcessInfo)
        {
            return new MailMessage();
        }

        public string BuildProcessInfo(bool isHTML, bool showProcessInfo)
        {
            string result = "";
            Formatter formatter = new Formatter();
            if (showProcessInfo)
            {
                result = formatter.BuildProcessInfo(isHTML);
            }
            return result;
        }

        public string BuildProcessInfo(bool isHTML)
        {
            return BuildProcessInfo(isHTML, true);
        }
    }
}