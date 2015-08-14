using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alerting.Alert
{
    public class EmailManagerHome
    {
        private static EmailManager EmailManager { get; set; }

        public static EmailManager GetEmailManager()
        {
            if (EmailManager == null)
            {
                EmailManager = new EmailManager();
            }
            return EmailManager;
        }


    }
}
