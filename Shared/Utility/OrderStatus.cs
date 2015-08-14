using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Utility
{
    public enum OrderStatus
    {
        Pending,
        Working,
        Filled,
        Canceled,
        Deleted,
        Paused,
        Rejected,
        ReviseRejected,
        CancelRejected,
        PausePending,
        FlashPending
    }
}
