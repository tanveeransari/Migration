using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alerting.CustomProperties
{
    public interface ICustomProperties
    {
        bool NoValidation { get; set; }

        bool ValidateAllLoggerInstances { get; set; }
        string EmailTo { get; set; }
        string EmailFrom { get; set; }
        string EmailSMTPServer { get; set; }
        string EmailSubject { get; set; }
       // string EmailBodyTemplate { get; set; }
        bool TestMode { get; set; }
        string EmailSubjectPrefix { get; set; }
        string EmailSubjectSuffix { get; set; }
        string EmailBodyHeader { get; set; }
        string EmailBodyFooter { get; set; }

        int EmailThreashholdCount { get; set; }
        int EmailThreashholdDuration { get; set; }
        bool EmailDisabled { get; set; }
        bool EmailManagerEnabled { get; set; }
        string EmailErrorMessageBodyTemplate { get; set; }

        bool UseEmailHelperNullImpl { get; set; }
    }
}
