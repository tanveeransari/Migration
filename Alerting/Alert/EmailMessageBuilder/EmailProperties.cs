namespace Alerting.Alert.EmailMessageBuilder
{
    public class EmailProperties: IEmailProperties
    {
        public string BCC { get; set; }
        public string CC { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string To { get; set; }
        public bool IsHTML { get; set; }

        public override string ToString()
        {
            return string.Format("BCC: {0}, CC: {1}, From: {2}, Subject: {3}, To: {4}, IsHTML: {5}", BCC, CC, From, Subject, To, IsHTML);
        }
    }
}