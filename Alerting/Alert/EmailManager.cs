using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Alerting.CustomProperties;

namespace Alerting.Alert
{
    public class EmailManager
    {
        protected int EmailThreashholdCount { get; set; }
        protected int EmailThreashholdDuration { get; set; }
        public bool EmailDisabled { get; set; }
        protected Timer EmailTimer { get; set; }

        protected internal EmailManager()
        {
            EmailDisabled = CustomPropertiesHome.GetCustomProperties().EmailDisabled;
            EmailThreashholdCount = 0;
            EmailThreashholdDuration = CustomPropertiesHome.GetCustomProperties().EmailThreashholdDuration;
            EmailTimer = new Timer();
            EmailTimer.Interval = EmailThreashholdDuration;

            EmailTimer.Elapsed += new ElapsedEventHandler(EmailTimer_Elapsed);
               
        }

        void EmailTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ResetTimerCounts();
        }

        protected void ResetTimerCounts()
        {
            EmailTimer.Enabled = false;

            EmailThreashholdCount = 0;

            EmailTimer.Enabled = true;
        }


        public bool EmailPassesFilter()
        {
            bool result = false;

            if (!CustomPropertiesHome.GetCustomProperties().EmailManagerEnabled && !EmailDisabled)
            {
                result = true;
            }
            else if (!EmailDisabled && EmailThreashholdCount < CustomPropertiesHome.GetCustomProperties().EmailThreashholdCount)
            {
                //EmailThreashholdCount++;
                result = true;
            }

            return result;
        }

        public void SentEmail()
        {
            EmailThreashholdCount++;
        }

    }
}
