using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Net.Mail;
using Alerting.CustomProperties;
using Alerting.GUI;
using NLog;
using NLog.Targets;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace Alerting.Alert
{
#pragma warning disable 612,618

    public enum Severity
    {
        Trace,
        Info,
        Debug,
        Audit,
        Warning,
        Error,
        Fatal
    } ;

    /// <summary>
    /// Where isHTLM parameters are available the format of the email message will be wrapped with the proper html tags
    /// 
    /// Where displayMessageBox parameters are available and set to true MessageBox will display to the user with the the log message parameters or defaults
    ///
    /// Where rawMessage parameters are available this will be used as the text in the log message and message will be used as the text in the email body
    ///  
    /// </summary>
    public class Logger
    {
        protected internal Logger(string name, bool unitTest, bool displayMessageBox)
        {
            Name = name;
            UnitTest = unitTest;
            DisplayMessageBox = displayMessageBox;
            
        }

        // only one instance of NUllLogger required
        protected static NLog.Logger NullLogger = NLog.LogManager.CreateNullLogger();
        protected string Name { get; set; }
        protected bool UnitTest { get; set; }
        protected bool validated;

        protected bool Validated {
            get { return validated; }
            set
            {
                validated = value;
                SomeLoggerValidated = true;
            }
        }

        protected bool DisplayMessageBox { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [some logger validated].
        /// </summary>
        /// <value><c>true</c> if [some logger validated] then at least one logger will be validated; otherwise, none are validated <c>false</c>.</value>
        public static bool SomeLoggerValidated { get; protected set; }


        /// <summary>
        /// Gets or sets the executing path.
        /// </summary>
        /// <value>The executing path. Useful to set if running a service or web as opposed to an application</value>
        public string ExecutingPath { get; set; }

        /// <summary>
        /// Use LoggerHome.GetLogger.
        /// 
        /// This is used for internal use but public as other wrappers require this.
        /// Gets the NLogger wrapper. 
        /// 
        /// </summary>
        /// <value>The N logger.</value>
        public NLog.Logger NLogger
        {
            get
            {
                NLog.Logger logger;

                try
                {
                    // any part of fully qualified name that has "UnitTest" in it will use a NUllLogger
                    if (Name.Contains("UnitTest") || UnitTest == true)
                    {
                        logger = NullLogger;
                    }
                    else
                    {
                        logger = LogManager.GetLogger(Name);

                        if (ShouldValidate())
                        {
                            if (!ValidLogFiles(logger))
                            {
                                logger = NullLogger;
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    LoggerHome.LogToWindowsEventLog("Logger creation failed. "+exception.ToString());
                    logger = Logger.NullLogger;
                }

                return logger;
            }
        }

        protected bool ShouldValidate()
        {
            bool result = true;
            if (!CustomPropertiesHome.GetCustomProperties().NoValidation)
            {

                if (CustomPropertiesHome.GetCustomProperties().ValidateAllLoggerInstances)
                {
                    result = !Validated;
                }
                else
                {
                    result = !SomeLoggerValidated;
                }
            }
            else
            {
                result = false;
            }

            return result;
        }
        
        protected bool ValidLogFiles(NLog.Logger aLogger)
        {
            bool result = false;

            try
            {

                Exception exception = new Exception("Test logger exception - not an error/exception.");
                /*
                IDictionary<string, string> map = GetLogPathMaps();
                foreach (KeyValuePair<string, string> keyValuePair in map)
                {
                    string name = keyValuePair.Value;
                    if (!HasWritePermission(name))
                    {
                        result = false;
                        myLog.WriteEntry("Error Logger creation failed. Using NULL Impl." + "Log file: " + name + " does not have write permission.", EventLogEntryType.Error);
                        Validated = true;
                        break;
                    }
                    else
                    {
                        result = true;
                        Validated = true;
                    }

                }
                
                if (result)
                {
                    aLogger.DebugException("Logger Creation Test: test logger - debug", exception);
                    aLogger.ErrorException("Logger Creation Test: test logger - exception", exception);

                }
                 */

                aLogger.DebugException("Logger Creation Test: test logger - debug", exception);
                aLogger.ErrorException("Logger Creation Test: test logger - exception", exception);

                Validated = true;
                result = true;
                //CustomPropertiesHome.GetCustomProperties();
            }
            catch (Exception exception)
            {
                try
                {
                    aLogger.FatalException("Logger creation failed. Using NULL Impl", exception);
                }
                catch (Exception exception2)
                {
                    LoggerHome.LogToWindowsEventLog("Null Logger creation failed. " + exception2.ToString());
                }

                LoggerHome.LogToWindowsEventLog("Error Logger creation failed. Using NULL Impl." + exception.ToString());

                result = false;
            }

            return result;
        }
        
        protected bool HasWritePermission(string filePath)
        {
            try
            {
                FileSystemSecurity security;
                if (File.Exists(filePath))
                {
                    security = File.GetAccessControl(filePath);
                }
                else
                {
                    security = Directory.GetAccessControl(Path.GetDirectoryName(filePath));
                }
                var rules = security.GetAccessRules(true, true, typeof(NTAccount));

                var currentuser = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                bool result = false;
                foreach (FileSystemAccessRule rule in rules)
                {
                    if (0 == (rule.FileSystemRights &
                        (FileSystemRights.WriteData | FileSystemRights.Write)))
                    {
                        continue;
                    }

                    SecurityIdentifier s;

                    NTAccount f = new NTAccount(rule.IdentityReference.Value);
                    try
                    {
                        s = (SecurityIdentifier)f.Translate(typeof(SecurityIdentifier));
                    }
                    catch
                    {
                        s = new System.Security.Principal.SecurityIdentifier(rule.IdentityReference.Value);
                    }
                    String sidString = s.ToString();


                    //Random - this one works better, without that SDDL is taken as name of a group literally                
                    //System.Security.Principal.SecurityIdentifier sid = new System.Security.Principal.SecurityIdentifier(rule.IdentityReference.Value);
                    System.Security.Principal.SecurityIdentifier sid = new System.Security.Principal.SecurityIdentifier(sidString);

                    if (!currentuser.IsInRole(sid))
                    {
                        continue;
                    }
                    if (rule.AccessControlType == AccessControlType.Deny)
                        return false;
                    if (rule.AccessControlType == AccessControlType.Allow)
                        result = true;
                }
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        /// <summary>
        /// Logs Audits of the specified message to Log.
        /// This will use  WindowsIdentity.GetCurrent() to prepend the user 
        /// in "<Audit:"+user+"> format
        /// </summary>
        /// <param name="message">The message.</param>
        public void Audit(string message)
        {
            string user = "";
            WindowsIdentity current;
            current = WindowsIdentity.GetCurrent();

            if(current != null)
            {
                user = current.Name;
            }

            NLogger.Info("<Audit:"+user+"> "+message);
        }

        /// <summary>
        /// Logs Traces of the specified message to log.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Trace(string message)
        {
            Trace(message, null);
            
        }

        /// <summary>
        /// Logs Traces of the specified message and exception to log.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public void Trace(string message, Exception exception)
        {
            
            Trace(message, exception, DisplayMessageBox);
        }


        /// <summary>
        /// Logs Traces of the specified message and exception to log.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="displayMessage">if set to <c>true</c> [display message].</param>
        public void Trace(string message, Exception exception, bool displayMessage)
        {
            NLogger.TraceException(message, exception);
            DisplayMessage(message, exception, displayMessage, Severity.Trace);
        }

        /// <summary>
        /// Logs Info of the specified message to log
        /// </summary>
        /// <param name="message">The message.</param>
        public void Info(string message)
        {
            Info(message, null);
        }

        /// <summary>
        /// Logs Info of the specified message and exception to log.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public void Info(string message, Exception exception)
        {
            
            Info(message, exception, DisplayMessageBox);
        }

        /// <summary>
        /// Logs Info of the specified message and exception to log.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="displayMessage">if set to <c>true</c> [display message].</param>
        public void Info(string message, Exception exception, bool displayMessage)
        {

            NLogger.InfoException(message, exception); 
            DisplayMessage(message, exception, displayMessage, Severity.Info);
        }


        /// <summary>
        /// Logs Debugs of the specified message to log.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Debug(string message)
        {
            Debug(message, null);
        }

        /// <summary>
        /// Logs Debugs of the specified message and exception to log.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public void Debug(string message, Exception exception)
        {
            
            Debug(message, exception, DisplayMessageBox);
        }

        /// <summary>
        /// Logs Debugs of the specified message and exception to log.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="displayMessage">if set to <c>true</c> [display message].</param>
        public void Debug(string message, Exception exception, bool displayMessage)
        {
            
            NLogger.DebugException(message, exception);
            DisplayMessage(message, exception, displayMessage, Severity.Debug);
        }


        /// <summary>
        /// Logs Warns of the specified message to log.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Warn(string message)
        {
            Warn(message, null);
        }

        /// <summary>
        /// Logs Warns of the specified message and exception to log.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public void Warn(string message, Exception exception)
        {
            
            Warn(message, exception, DisplayMessageBox);
        }

        /// <summary>
        /// Log Warns of the specified message and exception to log.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="displayMessage">if set to <c>true</c> [display message].</param>
        public void Warn(string message, Exception exception, bool displayMessage)
        {
            NLogger.WarnException(message, exception);
            DisplayMessage(message, exception, displayMessage, Severity.Warning);
        }


        /// <summary>
        /// Logs Error of the specified message to log.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Error(string message)
        {
            Error(message, null);
        }

        /// <summary>
        /// Logs Error of the specified message and exception to log.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public void Error(string message, Exception exception)
        {
            
            Error(message, exception, DisplayMessageBox);
        }

        /// <summary>
        /// Logs Error of the specified message and exception to log.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="displayMessage">if set to <c>true</c> [display message].</param>
        public void Error(string message, Exception exception, bool displayMessage)
        {
            NLogger.ErrorException(message, exception);
            DisplayMessage(message, exception, displayMessage, Severity.Error);
        }


        /// <summary>
        /// Logs Fatal of the specified message to log.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Fatal(string message)
        {
            Fatal(message, null);
        }

        /// <summary>
        /// Logs Fatal of the specified message and exception to log.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public void Fatal(string message, Exception exception)
        {
            Fatal(message, exception, DisplayMessageBox);
        }

        /// <summary>
        /// Logs Fatal of the specified message and exception to log.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="displayMessage">if set to <c>true</c> [display message].</param>
        public void Fatal(string message, Exception exception, bool displayMessage)
        {
            NLogger.FatalException(message, exception);
            DisplayMessage(message, exception, displayMessage, Severity.Fatal);
        }

  

        protected virtual void DisplayMessage(string message, Exception exception, bool display, Severity severity)
        {
            if (display)
            {
                //Application.OpenForms
               MessageBox.Show(null, FormatMessage(message, exception), "Notification: "+severity, MessageBoxButtons.OK, GetMessageBoxIcon(severity));
               // MessageDisplay.Show( FormatMessage(message, exception), "Notification: " + severity, GetMessageBoxIcon(severity));
            }
        }

        protected virtual string FormatMessage(string message, Exception exception)
        {
            string formattedMessage;
            if (exception != null)
            {
                formattedMessage = string.Format("{0}: {1}", message, exception.ToString());
            }
            else
            {
                formattedMessage = string.Format("{0}", message);
            }

            return formattedMessage;
        }

        protected MessageBoxIcon GetMessageBoxIcon(Severity severity)
        {
            MessageBoxIcon result = MessageBoxIcon.None;

            switch (severity)
            {
                case Severity.Trace:
                case Severity.Info:
                case Severity.Debug:
                    result = MessageBoxIcon.Information;
                    break;

                case Severity.Warning:
                    result = MessageBoxIcon.Warning;
                    break;

                case Severity.Error:
                case Severity.Fatal:
                    result = MessageBoxIcon.Error;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Sends Fatal of the message to log and sends email.
        /// </summary>
        /// <param name="rawLogMessage">The raw log message.</param>
        /// <param name="message">The message.</param>
        /// <param name="isHTML">if set to <c>true</c> [is HTML].</param>
        public void FatalEmail(string rawLogMessage, string message, bool isHTML)
        {
            NLogger.Fatal(rawLogMessage);

            EmailHelper emailHelper = new EmailHelper();

            MailMessage msg = emailHelper.BuildEmailMessage(message, isHTML);

            emailHelper.SendMail(msg);

        }

        /// <summary>
        /// Sends Fatal of the message to log and sends email.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="isHTML">if set to <c>true</c> [is HTML].</param>
        public void FatalEmail(string message, bool isHTML)
        {
            FatalEmail(message, message, isHTML);
        }

        /// <summary>
        /// Sends Fatal of the message to log and sends email.
        /// </summary>
        /// <param name="message">The message.</param>
        public void FatalEmail(string message)
        {
            FatalEmail(message, false);
        }

        /// <summary>
        /// Sends Fatal of the message to log and sends email.
        /// </summary>
        /// <param name="rawLogMessage">The raw log message.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="isHTML">if set to <c>true</c> [is HTML].</param>
        public void FatalEmail(string rawLogMessage, string message, Exception exception, bool isHTML)
        {
            NLogger.FatalException(rawLogMessage, exception);
            EmailHelper emailHelper = new EmailHelper();

            MailMessage msg = emailHelper.BuildEmailMessage(message , exception, isHTML);

            emailHelper.SendMail(msg);

        }

        /// <summary>
        /// Sends Fatal of the message and exception to log and sends email.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="isHTML">if set to <c>true</c> [is HTML].</param>
        public void FatalEmail(string message, Exception exception, bool isHTML)
        {
            FatalEmail(message, message, exception, isHTML);
        }

        /// <summary>
        /// Sends Fatal of the message and exception to log and sends email.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public void FatalEmail(string message, Exception exception)
        {
            FatalEmail(message, exception, false);
        }

        /// <summary>
        /// Logs (based on severity type) the message to log and sends email.
        /// </summary>
        /// <param name="severity">The severity.</param>
        /// <param name="rawLogMessage">The raw log message.</param>
        /// <param name="message">The message used in email body.</param>
        /// <param name="isHTML">if set to <c>true</c> [is HTML] for email format.</param>
        public void LogEmail(Severity severity, string rawLogMessage, string message, bool isHTML)
        {
            //NLogger.Fatal(rawLogMessage);
            NLogger.Log(GetLevel(severity), rawLogMessage);
            EmailHelper emailHelper = new EmailHelper();

            MailMessage msg = emailHelper.BuildEmailMessage(message, isHTML);

            emailHelper.SendMail(msg);

        }

        /// <summary>
        /// Gets the NLog (severity) level for a given Alerting (severity) level.
        /// </summary>
        /// <param name="severity">The severity.</param>
        /// <returns></returns>
        public LogLevel GetLevel(Severity severity)
        {
            LogLevel result;

            switch (severity)
            {
                case Severity.Trace:
                    result = LogLevel.Trace;
                    break;

                case Severity.Info:
                case Severity.Audit:
                    result = LogLevel.Info;
                    break;

                case Severity.Debug:
                    result = LogLevel.Debug;
                    break;

                case Severity.Warning:
                    result = LogLevel.Warn;
                    break;

                case Severity.Error:
                    result = LogLevel.Error;
                    break;

                case Severity.Fatal:
                    result = LogLevel.Fatal;
                    break;

                default:
                    result = LogLevel.Error;
                    break;

            }

            return result;
        }

        /// <summary>
        /// Logs (based on severity type) the message to log and sends email.
        /// </summary>
        /// <param name="severity">The severity.</param>
        /// <param name="message">The message.</param>
        /// <param name="isHTML">if set to <c>true</c> [is HTML].</param>
        public void LogEmail(Severity severity, string message, bool isHTML)
        {
            LogEmail(severity, message, message, isHTML);
        }

        /// <summary>
        /// Logs (based on severity type) the message to log and sends email.
        /// </summary>
        /// <param name="severity">The severity.</param>
        /// <param name="message">The message.</param>
        public void LogEmail(Severity severity, string message)
        {
            LogEmail(severity, message, false);
        }

        /// <summary>
        /// Logs (based on severity type) the message and exception to log and sends email.
        /// </summary>
        /// <param name="severity">The severity.</param>
        /// <param name="rawLogMessage">The raw log message.</param>
        /// <param name="message">The message used in email body.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="isHTML">if set to <c>true</c> [is HTML].</param>
        public void LogEmail(Severity severity, string rawLogMessage, string message, Exception exception, bool isHTML)
        {
           // NLogger.FatalException(rawLogMessage, exception);
            NLogger.LogException(GetLevel(severity), rawLogMessage, exception);
            EmailHelper emailHelper = new EmailHelper();

            MailMessage msg = emailHelper.BuildEmailMessage(message, exception, isHTML);

            emailHelper.SendMail(msg);

        }

        /// <summary>
        /// Logs (based on severity type) the message and exception to log and sends email.
        /// </summary>
        /// <param name="severity">The severity.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="isHTML">if set to <c>true</c> [is HTML].</param>
        public void LogEmail(Severity severity, string message, Exception exception, bool isHTML)
        {
            LogEmail(severity, message, message, exception, isHTML);
        }

        /// <summary>
        /// Logs (based on severity type) the message and exception to log and sends email.
        /// </summary>
        /// <param name="severity">The severity.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public void LogEmail(Severity severity, string message, Exception exception)
        {
            LogEmail(severity, message, exception, false);
        }

        /// <summary>
        /// Gets the error log path define for NLog config.
        /// </summary>
        /// <returns></returns>
        public string GetErrorLogPath()
        {
            string result = "";

            GetLogPathMaps().TryGetValue("Error", out result);

            return result;
        }

        /// <summary>
        /// Gets the log path define for NLog config.
        /// </summary>
        /// <returns></returns>
        public string GetLogPath()
        {
            string result = "";

            GetLogPathMaps().TryGetValue("Log", out result);

            return result;
            
        }

        /// <summary>
        /// Gets the audit log path define for NLog config.
        /// </summary>
        /// <returns></returns>
        public string GetAuditPath()
        {
            string result = "";

            GetLogPathMaps().TryGetValue("Audit", out result);

            return result;

        }

        /// <summary>
        /// Gets all exceptions log path define for NLog config.
        /// </summary>
        /// <returns></returns>
        public string GetAllExceptionsPath()
        {
            string result = "";

            GetLogPathMaps().TryGetValue("AllExceptions", out result);

            return result;

        }

        /// <summary>
        /// Gets all the log path maps for defined NLog config file.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetLogPathMaps()
        {
            IDictionary<string, string> logAgentFileNames = new Dictionary<string, string>();

            IList<string> names = LoggerHome.GetLogger(this).GetLogPaths();
            string[] indetifierTypes = {"{0}.log", "{0}_AllExceptions.err", "{0}.err","{0}_Audit.log"};
            string[] typeNames = {"Log", "AllExceptions", "Error","Audit"};

            for (int x = 0; x < indetifierTypes.Length; x++)
            {
                foreach (string name in names)
                {
                    string s = String.Format(indetifierTypes[x], System.Diagnostics.Process.GetCurrentProcess().ProcessName);
                    if (name.ToLower().Contains(s.ToLower()))
                    {
                        //results[x] = name;
                        logAgentFileNames.Add(typeNames[x], name);
                    }
                }
            }
            return logAgentFileNames;
        }

        /// <summary>
        /// Gets the log paths for defined NLog config file.
        /// </summary>
        /// <returns></returns>
        public IList<string> GetLogPaths()
        {
            ReadOnlyCollection<Target> items = NLog.LogManager.Configuration.ConfiguredNamedTargets;
            FileTarget fileTarget = null;
            IList<string> fileNames = new List<string>();

            foreach (Target target in items)
            {
                fileTarget = target as FileTarget;
                if (fileTarget != null)
                {
                    string f = fileTarget.FileName.ToString().Replace("${processname}", "{0}");
                    f = String.Format(f, Process.GetCurrentProcess().ProcessName);

                    fileNames.Add(f);
                }
            }

            return fileNames;
        }

        /// <summary>
        /// Gets the log path links as HTLM for NLog config file.
        /// </summary>
        /// <returns></returns>
        public string GetLogPathLinks()
        {
            ReadOnlyCollection<Target> items = NLog.LogManager.Configuration.ConfiguredNamedTargets;
            FileTarget fileTarget = null;
            IList<string> fileNames = new List<string>();
            //StringBuilder sb = new StringBuilder(200);

            foreach (Target target in items)
            {
                fileTarget = target as FileTarget;
                if (fileTarget != null)
                {
                    string f = fileTarget.FileName.ToString().Replace("${processname}", "{0}");
                    f = String.Format(f, Process.GetCurrentProcess().ProcessName);

                    string unc = f.Replace(@"\", "/");
                    fileNames.Add(unc);
                }
            }

            return FormatPathLinks(fileNames);

        }

        protected string FormatPathLinks(IList<string> fileNames)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<br/>");
            sb.Append(System.Environment.NewLine);

            foreach (string fileName in fileNames)
            {
                string s;
                s = String.Format("<a href=\"file://{0}\">{0}</a><br/>", fileName);

                sb.Append(s);
                sb.Append(System.Environment.NewLine);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Determines whether [is trace enabled].
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is trace enabled]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsTraceEnabled()
        {
            return NLogger.IsTraceEnabled;
        }

        /// <summary>
        /// Determines whether [is debug enabled].
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is debug enabled]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsDebugEnabled()
        {
            return NLogger.IsDebugEnabled;
        }

        /// <summary>
        /// Determines whether [is info enabled].
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is info enabled]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsInfoEnabled()
        {
            return NLogger.IsInfoEnabled;
        }

        /// <summary>
        /// Determines whether [is warn enabled].
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is warn enabled]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsWarnEnabled()
        {
            return NLogger.IsWarnEnabled;
        }

        /// <summary>
        /// Determines whether [is display message box enabled].
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is display message box enabled]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsDisplayMessageBoxEnabled()
        {
            return DisplayMessageBox;
        }

        public string GetDllPath()
        {
            return Application.StartupPath + ";" + System.IO.Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// Sends email without sending email.
        /// </summary>
        /// <param name="bcc">The BCC.</param>
        /// <param name="body">The body.</param>
        /// <param name="cc">The cc.</param>
        /// <param name="from">From.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="to">To.</param>
        /// <param name="attachmentFiles">The attachment files.</param>
        /// <param name="isHTML">if set to <c>true</c> [is HTML].</param>
        /// <returns></returns>
        public bool SendMail(string bcc, string body, string cc, string from, string subject, string to, string[] attachmentFiles, bool isHTML)
        {
            EmailHelper emailHelper = new EmailHelper();

            MailMessage mailMessage = new MailMessage(from, to, subject, body);
            mailMessage.Bcc.Add(new MailAddress(bcc));
            mailMessage.CC.Add(new MailAddress(cc));

            foreach (string attachmentFile in attachmentFiles)
            {
                Attachment mailAttachment = new Attachment(attachmentFile);
                mailMessage.Attachments.Add(mailAttachment);
            }

            mailMessage.IsBodyHtml = isHTML;

            return emailHelper.SendMail(mailMessage);
        }
    }
#pragma warning restore 612,618
}