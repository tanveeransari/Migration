using System;

namespace Alerting.Alert.EmailMessageBuilder
{
    public interface IEmailMessageBuilder: IMutableEmailMessageBuilder
    {
        //bool AppendBody(string queueName, string bodyText);
        //bool AppendBody(string bodyText);

        //bool AppendBody(string queueName, string bodyText, Exception exception);
        //bool AppendBody(string bodyText, Exception exception);

        //bool InsertBody(string queueName, string bodyText);
        //bool InsertBody(string bodyText);

        //bool InsertBody(string queueName, string bodyText, Exception exception);
        //bool InsertBody(string bodyText, Exception exception);

        //bool SendQueuedMessage(string queueName);
        //bool SendQueuedMessage();

        //bool ClearQueue(string queueName);
        //bool ClearQueue();
    }
}
