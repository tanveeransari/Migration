using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alerting.Alert
{
    public interface IQueueItem
    {
        string Id { get; set; }
        string Text { get; set; }
        Exception LastException { get; set; }

        string DisplayString { get;  }
        string DisplayDetailedString { get; }
    }
}
