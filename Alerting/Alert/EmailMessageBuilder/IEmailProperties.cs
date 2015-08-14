namespace Alerting.Alert.EmailMessageBuilder
{
    public interface IEmailProperties
    {
        string BCC { get; set; }
        string CC { get; set; }

        string From { get; set; }
        string Subject { get; set; }
        
        string To { get; set; }
        bool IsHTML { get; set; }
    }
}