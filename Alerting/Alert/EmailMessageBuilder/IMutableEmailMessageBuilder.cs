namespace Alerting.Alert.EmailMessageBuilder
{
    public interface IMutableEmailMessageBuilder
    {
        
        /// <summary>
        /// 
        /// {0} is message {1} is exception 
        /// </summary>
        /// 
        //string EmailBodyTemplate { get; set; }
        
        bool AppendBody(string queueName, IQueueItem queueItem);
        bool AppendBody(IQueueItem queueItem);

        bool InsertBody(string queueName, IQueueItem queueItem);
        bool InsertBody(IQueueItem queueItem);
        
        bool SendQueuedMessage(string queueName);
        bool SendQueuedMessage();

        bool ClearQueue(string queueName);
        bool ClearQueue();

        bool RemoveAllBodyItems(string queueName, string id);
        bool RemoveAllBodyItems(string id);

        bool SetEmailProperties(string queueName, IEmailProperties emailProperties);
        bool SetEmailProperties(IEmailProperties emailProperties);

        IEmailProperties GetDefaultEmailProperties();

        int Count(string errorQueueName);
        int Count(string errorQueueName, string id);

        bool ShowDetails { get; set; }
    }
}