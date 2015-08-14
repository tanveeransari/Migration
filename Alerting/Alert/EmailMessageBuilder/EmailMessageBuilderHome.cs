using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alerting.Alert.EmailMessageBuilder;

namespace Alerting.Alert
{

    public class EmailMessageBuilderHome
    {
        //private static IEmailMessageBuilder IEmailMessageBuilder { get; set; }

        private static IEmailMessageBuilder IEmailMessageBuilder { get; set; }

        public static IEmailMessageBuilder GetEmailMessageBuilder()
        {
            if (IEmailMessageBuilder == null)
            {
                //IEmailMessageBuilder = new EmailMessageBuilderSimpleImpl();
                IEmailMessageBuilder = new EmailMessageBuilderMutableImpl();
            }
            return IEmailMessageBuilder;
        }

        public static IEmailMessageBuilder CreateNewInstance()
        {
            IEmailMessageBuilder = new EmailMessageBuilderMutableImpl();
            return IEmailMessageBuilder;
        }

    }

}
