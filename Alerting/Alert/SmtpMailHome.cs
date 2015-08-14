namespace Alerting.Alert
{
    public class SmtpMailHome
    {
        private static ISmtpMail smtpMail = null;
        private static SmtpMailNullImpl nullSmtp = null;

        public static ISmtpMail GetInstance()
        {
            if (smtpMail == null)
            {
                smtpMail = new SmtpMailImpl();
            }
            return smtpMail;
        }

        public static ISmtpMail GetNullImpl()
        {
            if (nullSmtp == null)
            {
                nullSmtp = new SmtpMailNullImpl();
            }
            return nullSmtp;
        }
    }
}