using System;

namespace Alerting.Alert.EmailErrorBuilder
{
    public interface IEmailErrorBuilder
    {


        bool InsertError(string message, Exception exception);
        bool InsertError(string message);

        bool InsertError(string id,string message, Exception exception);
        bool InsertError(string id, string message);


        bool AppendError(string message, Exception exception);
        bool AppendError(string message);

        bool AppendError(string id, string message, Exception exception);
        bool AppendError(string id, string message);

        bool SendErrorEmail();

        bool RemoveAllErrorsById(string id);

        int Count();
        int Count(string id);

        bool ShowDetails { get; set; }

        bool ClearQueue();
    }
}
