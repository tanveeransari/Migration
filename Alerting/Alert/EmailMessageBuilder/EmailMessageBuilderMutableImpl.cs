using System;
using System.Collections.Generic;
using System.Net.Mail;
using Alerting.Alert.Formatters;
using Alerting.CustomProperties;

namespace Alerting.Alert.EmailMessageBuilder
{
    class EmailMessageBuilderMutableImpl : IEmailMessageBuilder
    {
        public static string DEFAULT_KEY_NAME = "default_key";
        protected IDictionary<string, IList<IQueueItem>> QueueMapper { get; set; }
        protected IDictionary<string, IEmailProperties> QueuePropertiesMapper { get; set; }
        
        public EmailMessageBuilderMutableImpl()
        {
            QueueMapper = new Dictionary<string, IList<IQueueItem>>();
            QueuePropertiesMapper = new Dictionary<string, IEmailProperties>();

            //GetDefaultEmailProperties();
        }

        public IEmailProperties GetDefaultEmailProperties()
        {

            IEmailProperties emailProperties  = new EmailProperties();

            emailProperties.BCC = "";
            emailProperties.CC = "";

            emailProperties.From = CustomPropertiesHome.GetCustomProperties().EmailFrom;
            emailProperties.Subject = CustomPropertiesHome.GetCustomProperties().EmailSubject;
            emailProperties.To = CustomPropertiesHome.GetCustomProperties().EmailTo;

            emailProperties.IsHTML = false;

            return emailProperties;
        }

        public int Count(string queueName)
        {
            int result = 0;

            IList<IQueueItem> queueItems = new List<IQueueItem>();

            if (QueueMapper.ContainsKey(queueName))
            {
                QueueMapper.TryGetValue(queueName, out queueItems);

                result = queueItems.Count;
            }

            return result;

        }

        public int Count(string queueName, string id)
        {
            int result = 0;

            IList<IQueueItem> queueItems = new List<IQueueItem>();

            if (QueueMapper.ContainsKey(queueName))
            {
                QueueMapper.TryGetValue(queueName, out queueItems);

                foreach (IQueueItem queueItem in queueItems)
                {
                    if (queueItem.Id.Equals(id))
                    {
                        result++;
                    }
                }
            }

            return result;
        }

        public bool ShowDetails {get;set;}


        protected bool AddToBody(string queueName, IQueueItem queueItem, bool insert)
        {
            bool sucessful = true;

            try
            {
                IList<IQueueItem> queueItems = new List<IQueueItem>();

                if (QueueMapper.ContainsKey(queueName))
                {
                    QueueMapper.TryGetValue(queueName, out queueItems);
                }
                else
                {
                    QueueMapper.Add(queueName, queueItems);    
                }

                if (!insert)
                {
                    queueItems.Add(queueItem);
                }
                else
                {
                    queueItems.Insert(0,queueItem);
                }

                
            }
            catch (Exception e)
            {
                string s = string.Format("Error Queuing BodyText for {0}", queueName);
                LoggerHome.GetLogger(this).Error(s, e);
                sucessful = false;
            }

            return sucessful;
        }

        public bool AppendBody(string queueName, IQueueItem queueItem)
        {
            return AddToBody(queueName, queueItem, false);
        }

        public bool AppendBody(IQueueItem queueItem)
        {
            return AppendBody(DEFAULT_KEY_NAME, queueItem);
        }

        public bool InsertBody(string queueName, IQueueItem queueItem)
        {
            return AddToBody(queueName, queueItem, true);
        }

        public bool InsertBody(IQueueItem queueItem)
        {
            return InsertBody(DEFAULT_KEY_NAME, queueItem);
        }

        public bool SendQueuedMessage(string queueName)
        {

            bool result = false;
            string body = "";

            IList<IQueueItem> queueItems;

            QueueMapper.TryGetValue(queueName, out queueItems);
            IEmailProperties emailProperties;

            QueuePropertiesMapper.TryGetValue(queueName, out emailProperties);
            if (emailProperties == null)
            {
                LoggerHome.GetLogger(this).Error("Could not load email properties for queue: "+queueName);
                result = false;
            }
            else
            {
                if (queueItems != null && queueItems.Count > 0)
                {

                    //bool result = LoggerHome.GetLogger(this).SendMail(BCC, Body, CC, From, Subject, To, Attachments, IsHTML);
                    //EmailHelper emailHelper = new EmailHelper();
                    body = BuildEmailBody(queueItems, true);

                    MailMessage mailMessage = new MailMessage(emailProperties.From, emailProperties.To, emailProperties.Subject, body);
                    MailAddress bcc = new MailAddress(emailProperties.BCC);
                    mailMessage.Bcc.Add(bcc);
                    MailAddress cc = new MailAddress(emailProperties.CC);
                    mailMessage.CC.Add(cc);
                    mailMessage.IsBodyHtml = true;

                    result =  EmailHelperHome.GetEmailHelper().SendMail(mailMessage);
                    QueueMapper.Remove(queueName);
                }
            }
            return result;
        }

        protected string BuildEmailBody(IList<IQueueItem> queueItems, bool showDetails)
        {
            string newBodyText="";
            Formatter formatter = new Formatter();

            foreach (IQueueItem queueItem in queueItems)
            {
                newBodyText = formatter.FormatBody(newBodyText, queueItem, showDetails);
            }
            //newBodyText = formatter.FormatBody(currentSubject, bodyText, exception , insert);

            return newBodyText;
        }

        public bool SendQueuedMessage()
        {
            return SendQueuedMessage(DEFAULT_KEY_NAME);
        }

        public bool ClearQueue(string queueName)
        {
            bool successful = true;
            try
            {
                QueueMapper.Remove(queueName);
            }
            catch (Exception e)
            {
                string s = string.Format("Could not clear queue: {0}", queueName);

                LoggerHome.GetLogger(this).Error(s, e);
                successful = false;
            }

            return successful;
        }

        public bool ClearQueue()
        {
            return ClearQueue(DEFAULT_KEY_NAME);
        }

        public bool RemoveAllBodyItems(string queueName, string id)
        {
            bool result = false;

            IList<IQueueItem> queueItems;
            IList<IQueueItem> queueItemsMarkedForDelete = new List<IQueueItem>();

            QueueMapper.TryGetValue(queueName, out queueItems);

            if (queueItems != null)
            {
                foreach (IQueueItem queueItem in queueItems)
                {
                    if (queueItem.Id.Equals(id))
                    {
                        queueItemsMarkedForDelete.Add(queueItem);
                        //queueItems.Remove(queueItem);
                    }
                }

                foreach (IQueueItem queueItem in queueItemsMarkedForDelete)
                {
                    queueItems.Remove(queueItem);
                }

                result = true;
            }
            else
            {
                LoggerHome.GetLogger(this).Warn("Queue not found: "+queueName);
                result = true;
            }
            

            return result;
        }

        public bool RemoveAllBodyItems(string id)
        {
            return RemoveAllBodyItems(DEFAULT_KEY_NAME, id);
        }

        public bool SetEmailProperties(IEmailProperties emailProperties)
        {
            return SetEmailProperties(DEFAULT_KEY_NAME, emailProperties);
        }

        public bool SetEmailProperties(string queueName, IEmailProperties emailProperties)
        {
            bool sucessful = true;

            try
            {
                if (QueuePropertiesMapper.ContainsKey(queueName))
                {
                    QueuePropertiesMapper[queueName] = emailProperties;
                }

                QueuePropertiesMapper.Add(queueName, emailProperties);    
            }
            catch (Exception e)
            {
                string s = string.Format("Error setting email properties: [{0}] for Queue {1}",emailProperties, queueName);
                LoggerHome.GetLogger(this).Error(s, e);
                sucessful = false;
            }

            return sucessful;

        }
    }
}
