using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using NLog;

namespace Alerting.Alert
{
    public class LoggerHome
    {
        private static IDictionary<string, Logger> loggers;
        public static bool UnitTest { get; set; }
        public static bool DisplayMessageBox { get; set; }
        public static string ExePath { get; set; }

        public static string lockObject = "lockObject";

        public static Logger GetLogger(object o)
        {
            return GetLogger(o.GetType().FullName);
        }

        public static Logger GetLogger(MethodBase m)
        {
            Logger result = new Logger("",UnitTest, DisplayMessageBox);

            //MemberInfo info = m as MemberInfo;
            if (m != null)
            {
                result = GetLogger(m.DeclaringType.FullName);
            }
            else
            {
                result = GetLogger((object)m);
            }

            return result;
        }


        public static Logger GetLogger(string loggerName)
        {
            lock (lockObject)
            {
                if (loggers == null)
                {
                    //LogManager.ThrowExceptions = true;

                    loggers = new Dictionary<string, Logger>();
                }

                Logger aLogger;

                if (!loggers.ContainsKey(loggerName))
                {
                    aLogger = new Logger(loggerName, UnitTest, DisplayMessageBox);
                    loggers.Add(loggerName, aLogger);
                }
                else
                {
                    aLogger = loggers[loggerName];
                }

                return aLogger;
            }
        }

        static public void LogToWindowsEventLog(string messaage)
        {
            LogToWindowsEventLog(messaage, EventLogEntryType.Error);
        }

        static public void LogToWindowsEventLog(string messaage, EventLogEntryType type)
        {
            EventLog myLog = null;

            try
            {

                if (!EventLog.SourceExists("HALP Alert Logger"))
                {
                    EventLog.CreateEventSource("HALP Alert Logger", "Application");
                }

                myLog = new EventLog();
                myLog.Log = "Application";
                myLog.Source = "HALP Alert Logger";

                string s = string.Format("<Logger: writing to Event Log {0} {1}> {2}", AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName, messaage);

                myLog.WriteEntry(s, EventLogEntryType.Error);
            }
            catch (Exception e)
            {
                if (myLog != null)
                {
                    myLog.WriteEntry("Failed to write message to WindowsEventLog. " + e.ToString(), type);
                }

            }
        }        
    }
}