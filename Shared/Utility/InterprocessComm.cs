using System;
using System.Collections.Generic;

namespace Shared.Utility
{
    public static class InterprocessComm
    {
        public static void QueueAddChartSubscription(string symbol, string _instrument, Action<string, byte[]> bookMsgRecvd)
        {
            throw new NotImplementedException();
            //MarketGrid.MarketDisplayCache.getInstance().QueueAddChartSubscription(symbol, _instrument, BookMsgRecvd);
        }

        public static void QueueRemoveChartSubscription(string bSymbol, string _chartName)
        {
            throw new NotImplementedException();
            //MarketGrid.MarketDisplayCache.getInstance().QueueRemoveChartSubscription(bSymbol, _chartName);
        }

        internal static void SendChartCloseMessage()
        {
            throw new NotImplementedException();
        }

        internal static void StartChartApp()
        {
            throw new NotImplementedException();
        }

        internal static void AddChartForm(string chartWindowName)
        {
            throw new NotImplementedException();
        }

        internal static void RequestSaveChartPreferences()
        {
            throw new NotImplementedException();
        }

        internal static void QueueRemoveDOMSubscription(string bSymbol, string _chartName)
        {
            throw new NotImplementedException();
            //MarketGrid.MarketDisplayCache.getInstance().QueueRemoveDOMSubscription(bSymbol, _chartName);
        }
    }
}
