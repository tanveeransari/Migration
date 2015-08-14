using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alerting.CustomProperties;

namespace Alerting.Alert.EmailMessageBuilder.Models
{
    public class ErrorMessageQueueItem: QueueItem
    {
        public ErrorMessageQueueItem():this("")
        {
            
        }


        public ErrorMessageQueueItem(string text) : this(text, "")
        {

        }

        public ErrorMessageQueueItem(string text, string id)
            : this(text, id, null)
        {

        }

        public ErrorMessageQueueItem(string text, Exception lastException): this(text,"",lastException)
        {
            
        }

        public ErrorMessageQueueItem(string text, string id, Exception lastException)
        {
            Text = text;
            Id = id;
            LastException = lastException;
        }

        public override string DisplayString
        {
            get
            {
                    return string.Format(@"Error Message:
{0}", Text);
                
            }
        }
        public override string DisplayDetailedString
        {
            get
            {
                string s = "";

                if (LastException != null)
                {
                    s = string.Format(CustomPropertiesHome.GetCustomProperties().EmailErrorMessageBodyTemplate, Text, LastException.ToString(), LastException.StackTrace);
                }
                else
                {
                    s = string.Format(@"Error Message:
{0}", Text);
                }


                return s;
            }
        }

    }
}
