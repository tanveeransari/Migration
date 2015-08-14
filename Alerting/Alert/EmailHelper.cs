using System;
using System.Diagnostics;
using System.Text;
using System.Net.Mail;
using Alerting.Alert.Formatters;
using Alerting.CustomProperties;

namespace Alerting.Alert
{
    /// <summary>
    /// 
    /// Where isHTLM parameters are available the format of the email message will be wrapped with the proper html tags
    /// </summary>
    public class EmailHelper : IEmailHelper
    {

#pragma warning disable 612,618
        /// <summary>
        /// Sends the mail message to email.
        /// Will not log anything unless the threshold on the throttling is exceeded
        /// or there is a failre sending email
        /// </summary>
        /// <param name="mailMessage">The mail message.</param>
        /// <returns></returns>
        public virtual bool SendMail(MailMessage mailMessage)
        {
            bool result = false;
            ISmtpMail smtpMail = SmtpMailHome.GetInstance();

            //smtpMail.SmtpServer = Properties.Settings.Default.EmailSMTPServer;
            smtpMail.SmtpServer = CustomPropertiesHome.GetCustomProperties().EmailSMTPServer;
            try
            {
                if (CustomPropertiesHome.GetCustomProperties().EmailManagerEnabled)
                {
                    if (EmailManagerHome.GetEmailManager().EmailPassesFilter())
                    {
                        smtpMail.Send(mailMessage);
                        result = true;
                        EmailManagerHome.GetEmailManager().SentEmail();
                    }
                    else
                    {
                        LoggerHome.GetLogger(this).Info("Email Sending hit threshold limit:"+ CustomPropertiesHome.GetCustomProperties().EmailThreashholdCount+" for "+CustomPropertiesHome.GetCustomProperties().EmailThreashholdDuration+" ms. Message=["+mailMessage+" ]");
                    }

                }
                else
                {
                    smtpMail.Send(mailMessage);
                    result = true;
                    
                }
            }
            catch (Exception exception)
            {
                LoggerHome.GetLogger(this).Error("Error - can't send email: ", exception);
            }

            return result;
        }

        /// <summary>
        /// Helper convenience method to help build an email message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="isHTML">if set to <c>true</c> [is HTML].</param>
        /// <returns></returns>
        public virtual MailMessage BuildEmailMessage(string message, string subject, bool isHTML)
        {
            return BuildEmailMessage(message, subject, isHTML, true);
        }

        /// <summary>
        /// Helper convenience method to help build an email message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="isHTML">if set to <c>true</c> [is HTML].</param>
        /// <returns></returns>
        public virtual MailMessage BuildEmailMessage(string message, Exception exception, bool isHTML)
        {
            string mergedMessage = AppendException(message, exception, isHTML);
            return BuildEmailMessage(mergedMessage, isHTML);
        }

        protected virtual string AppendException(string message, Exception exception, bool isHtml)
        {
            string result = message + System.Environment.NewLine + " Exception: " + exception.ToString();

            if (isHtml)
            {
                string bodyEndTag = "</body>";
                int index = message.ToLower().IndexOf(bodyEndTag);

                if (index >= 0)
                {
                    message = message.Insert(index , " <br/> " + exception.ToString());
                }
                else
                {
                    message += "<br/>" + exception.ToString();
                }
                 
                result = message;
            }

            return result;
        }

        /// <summary>
        /// Helper convenience method to help build an email message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="isHTML">if set to <c>true</c> [is HTML].</param>
        /// <returns></returns>
        public virtual MailMessage BuildEmailMessage(string message, bool isHTML)
        {
            return BuildEmailMessage(message, CustomPropertiesHome.GetCustomProperties().EmailSubject, isHTML);

        }

        public MailMessage BuildEmailMessage(string message, string subject, bool isHTML, bool showProcessInfo)
        {
            MailMessage msg = new MailMessage(CustomPropertiesHome.GetCustomProperties().EmailFrom, CustomPropertiesHome.GetCustomProperties().EmailTo);
            
            string body = message;

            //if (Properties.Settings.Default.TestMode == true)
            if (CustomPropertiesHome.GetCustomProperties().TestMode == true)
            {
                msg.Subject = CustomPropertiesHome.GetCustomProperties().EmailSubjectPrefix + " ";
                msg.Subject += subject;
                msg.Subject += " " + CustomPropertiesHome.GetCustomProperties().EmailSubjectSuffix;


                if (isHTML)
                {
                    msg.IsBodyHtml = true;
                    string bodyEndTag = "</body>";
                    int index = body.ToLower().IndexOf(bodyEndTag);

                    if (index >= 0)
                    {
                        body = body.Insert(index + bodyEndTag.Length, "<br/>" + BuildProcessInfo(true, true));
                    }
                    else
                    {
                        body += "<br/>" + BuildProcessInfo(true, true);
                    }


                    string bodyStartTag = "<body>";

                    index = body.ToLower().IndexOf(bodyStartTag);

                    if (index >= 0)
                    {
                        body = body.Insert(index, "<br/>" + CustomPropertiesHome.GetCustomProperties().EmailBodyHeader + "<br/>");
                    }
                    else
                    {
                        body = body.Insert(0, CustomPropertiesHome.GetCustomProperties().EmailBodyHeader);
                    }

                    index = body.ToLower().IndexOf(bodyEndTag);

                    if (index >= 0)
                    {
                        body = body.Insert(index + bodyEndTag.Length, "<br/>" + CustomPropertiesHome.GetCustomProperties().EmailBodyFooter);
                    }
                    else
                    {
                        body += "<br/>" + CustomPropertiesHome.GetCustomProperties().EmailBodyFooter;
                    }

                    msg.Body = body;

                }
                else
                {
                    msg.Body = CustomPropertiesHome.GetCustomProperties().EmailBodyHeader;
                    msg.Body += Environment.NewLine;
                    msg.Body += body;
                    msg.Body += Environment.NewLine;
                    msg.Body += CustomPropertiesHome.GetCustomProperties().EmailBodyFooter;
                    msg.Body += Environment.NewLine;
                    msg.Body += System.Environment.NewLine + BuildProcessInfo(false, showProcessInfo);
                }

            }
            else
            {
                if (isHTML)
                {
                    msg.IsBodyHtml = true;
                    string bodyEndTag = "</body>";
                    int index = body.ToLower().IndexOf(bodyEndTag);

                    if (index >= 0)
                    {
                        body = body.Insert(index + bodyEndTag.Length, "<br/>" + BuildProcessInfo(true, showProcessInfo));
                    }
                    else
                    {
                        body += "<br/>" + BuildProcessInfo(true, showProcessInfo);
                    }
                }
                else
                {
                    body += System.Environment.NewLine + BuildProcessInfo(false, showProcessInfo);
                }
                msg.Subject = subject;
                msg.Body = body;
            }

            return msg;            
        }

        public MailMessage BuildEmailMessage(string message, bool isHTML, bool showProcessInfo)
        {
            return BuildEmailMessage(message, CustomPropertiesHome.GetCustomProperties().EmailSubject, isHTML, showProcessInfo);
        }

        /*
        public string BuildEmailBody(string message)
        {
            StringBuilder results = new StringBuilder();
            results.Append(message);
 
            BuildProcessInfo(results);

            return results.ToString();
        }
        */
        public virtual string BuildProcessInfo(bool isHTML, bool showProcessInfo)
        {
            string result = "";
            Formatter formatter = new Formatter();
            if (showProcessInfo)
            {
                result= formatter.BuildProcessInfo(isHTML);
            }
            return result;
        }
    }

    public class EmailHelperHome
    {
        private static IEmailHelper IEmailHelper { get; set; }

        public static IEmailHelper GetEmailHelper()
        {
            if (IEmailHelper == null)
            {
                if (!CustomPropertiesHome.GetCustomProperties().UseEmailHelperNullImpl)
                {
                    IEmailHelper = new EmailHelper();
                }
                else
                {
                    IEmailHelper = new EmailHelperNullImpl();
                }
            }
            return IEmailHelper;
        }
    }


#pragma warning restore 612,618
}