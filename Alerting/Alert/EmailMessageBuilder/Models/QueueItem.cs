using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alerting.Alert.EmailMessageBuilder.Models
{
    public class QueueItem: IQueueItem
    {
        public string Id { get; set; }
        public string Text { get; set; }

        public Exception LastException { get; set; }

        //protected string dislpayString;
        public virtual string DisplayString
        {
            get { return string.Format("{0}", Text); }
        }

        public virtual string DisplayDetailedString
        {
            get
            {


                //if (exception != null)
                //{
                //    s = string.Format("{0}: {1}{2}{3}", bodyText, exception.ToString(), Environment.NewLine, exception.StackTrace);
                //}
                //else
                //{
                //    s = string.Format("{0}", bodyText);
                //}

                string s = "";

                if (LastException != null)
                {
                    s = string.Format("{0}: {1}{2}{3}", Text, LastException.ToString(), Environment.NewLine, LastException.StackTrace);
                }
                else
                {
                    s = string.Format("{0}", Text);
                }
                
                return s;
            }
        }

        public QueueItem():this("")
        {
            
        }

        public QueueItem(string text)
            : this(text,"")
        {

        }

        public QueueItem(string text, string id):this(text,id,null)
        {

        }

        public QueueItem(string text, Exception exception)
            : this(text, "", exception)
        {

        }

        public QueueItem(string text, string id, Exception exception)
        {
            Text = text;
            Id = id;
            LastException = exception;
        }



        public bool Equals(QueueItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Id, Id) && Equals(other.Text, Text) && AreEquivalent(other.LastException, LastException);
        }

        public bool AreEquivalent(Exception lastException, Exception exception)
        {
            bool result = false;

            if (lastException == exception) return true;
            if (ReferenceEquals(null, lastException)) return false;
            if (ReferenceEquals(null, exception)) return false;
            if (ReferenceEquals(lastException, exception)) return true;

            if (lastException.Message.Equals(exception.Message))
            {
                if (lastException.StackTrace != null && exception.StackTrace != null)
                {
                    if (lastException.StackTrace.Equals(exception.StackTrace))
                    {
                        result = true;
                    }
                }
                else
                {
                    if (lastException.StackTrace == exception.StackTrace)
                    {
                        result = true;
                    }
                }
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (QueueItem)) return false;
            return Equals((QueueItem) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Id != null ? Id.GetHashCode() : 0);
                result = (result*397) ^ (Text != null ? Text.GetHashCode() : 0);
                result = (result*397) ^ (LastException != null ? LastException.GetHashCode() : 0);
                return result;
            }
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, Text: {1}, LastException: {2}", Id, Text, LastException);
        }
    }
}
