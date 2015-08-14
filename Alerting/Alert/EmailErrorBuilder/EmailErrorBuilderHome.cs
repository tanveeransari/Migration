namespace Alerting.Alert.EmailErrorBuilder
{
    

    public class EmailErrorBuilderHome
    {
        private static IEmailErrorBuilder IEmailErrorBuilder { get; set; }

        public static IEmailErrorBuilder GetEmailErrorBuilder()
        {
            if (IEmailErrorBuilder == null)
            {
                IEmailErrorBuilder = new EmailErrorBuilderImpl();
            }
            return IEmailErrorBuilder;
        }
    }
}
