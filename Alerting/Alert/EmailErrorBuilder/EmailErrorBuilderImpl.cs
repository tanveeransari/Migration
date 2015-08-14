using System;
using Alerting.Alert.EmailMessageBuilder;
using Alerting.Alert.EmailMessageBuilder.Models;
using Alerting.Alert.Formatters;

namespace Alerting.Alert.EmailErrorBuilder
{
    class EmailErrorBuilderImpl : IEmailErrorBuilder
    {

        public static string ERROR_QUEUE_NAME = "ERROR_QUEUE_NAME*";

        public bool ShowDetails { get; set; }

        protected IEmailMessageBuilder EmailMessageBuilder { get; set; }


        public bool ClearQueue()
        {
            return EmailMessageBuilder.ClearQueue(ERROR_QUEUE_NAME);
        }

        // Note: using Home to create new instance per instance of EmailErrorBuilderImpl
        // Note: do not use Home's get - it is a singleton
        public EmailErrorBuilderImpl()
        {

            EmailMessageBuilder = EmailMessageBuilderHome.CreateNewInstance();
            EmailMessageBuilder.SetEmailProperties(ERROR_QUEUE_NAME, EmailMessageBuilderHome.GetEmailMessageBuilder().GetDefaultEmailProperties());
        }

        public bool InsertError(string message, Exception exception)
        {
            //Formatter formatter  = new Formatter();

            //string s = formatter.FormatErrorBody(message, exception, ShowDetails);
            ////
            //s += Environment.NewLine;
            //s += Environment.NewLine;
            IQueueItem queueItem = new ErrorMessageQueueItem();

            return EmailMessageBuilder.InsertBody(ERROR_QUEUE_NAME, new ErrorMessageQueueItem(message, "", exception));
        }

        public bool InsertError(string message)
        {
            return EmailMessageBuilder.InsertBody(ERROR_QUEUE_NAME, new ErrorMessageQueueItem(message));
        }

        public bool InsertError(string id, string message, Exception exception)
        {
            return EmailMessageBuilder.InsertBody(ERROR_QUEUE_NAME, new ErrorMessageQueueItem(message, id, exception));
        }

        public bool InsertError(string id, string message)
        {
            return EmailMessageBuilder.InsertBody(ERROR_QUEUE_NAME, new ErrorMessageQueueItem(message, id));
        }

        public bool AppendError(string message, Exception exception)
        {
            //Formatter formatter = new Formatter();

            //string s = formatter.FormatErrorBody(message, exception, ShowDetails);
            ////
            //s += Environment.NewLine;
            //s += Environment.NewLine;
            return EmailMessageBuilder.AppendBody(ERROR_QUEUE_NAME, new ErrorMessageQueueItem(message, "", exception));
        }

        public bool AppendError(string message)
        {
            return EmailMessageBuilder.AppendBody(ERROR_QUEUE_NAME, new ErrorMessageQueueItem(message));
        }

        public bool AppendError(string id, string message, Exception exception)
        {
            return EmailMessageBuilder.AppendBody(ERROR_QUEUE_NAME, new ErrorMessageQueueItem(message, id, exception));
        }

        public bool AppendError(string id, string message)
        {
            return EmailMessageBuilder.AppendBody(ERROR_QUEUE_NAME, new ErrorMessageQueueItem(message, id));
        }

        public bool SendErrorEmail()
        {
            return EmailMessageBuilder.SendQueuedMessage(ERROR_QUEUE_NAME);
        }

        public bool RemoveAllErrorsById(string id)
        {
            return EmailMessageBuilder.RemoveAllBodyItems(ERROR_QUEUE_NAME, id);
        }

        public int Count()
        {
            return EmailMessageBuilder.Count(ERROR_QUEUE_NAME);
        }

        public int Count(string id)
        {
            return EmailMessageBuilder.Count(ERROR_QUEUE_NAME, id);
        }


    }
}
