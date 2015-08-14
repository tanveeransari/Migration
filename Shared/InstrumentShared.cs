using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    [Serializable]
    public class InstrumentShared
    {
        public string instrumentName;
        public DateTime expiry;
        public double minPriceIncrement;
        public double tickValue;
        public double highLimitPx;
        public double lowLimitPx;
        public int securityID;
        public double convertFactor;
        public string exchange;
        public string productCode;
        public string instrumentType;
        public string strategyType;
        public SpreadDuration spreadDuration = SpreadDuration.None;
        public bool isSingleSessionTraded = false;
        public string productGroup;

        public InstrumentShared()
        { }

        public InstrumentShared(int pSecID, string pInstrumentName, DateTime pExpiry, double pMinPxIncrement, double pTickValue,
            double pHighLimitPx, double pLowLimitPx, double pConvertFactor, string pExchange, string pProductCode, string pInstrumentType,
            string pStrategyType, string pProductGroup)
        {
            this.securityID = pSecID;
            this.instrumentName = pInstrumentName;
            this.expiry = pExpiry;
            this.minPriceIncrement = pMinPxIncrement;
            this.tickValue = pTickValue;
            this.highLimitPx = pHighLimitPx;
            this.lowLimitPx = pLowLimitPx;
            this.convertFactor = pConvertFactor;
            this.exchange = pExchange;
            this.productCode = pProductCode;
            this.instrumentType = pInstrumentType;
            this.strategyType = pStrategyType;
            this.productGroup = pProductGroup;
        }

        private SpreadDuration GetSpreadDurationforCalendarSpreads(string instrumentName)
        {
            if (this.instrumentName.IndexOf("-") > 0)
            {
                string[] insSplit = this.instrumentName.Split(',');
                if (insSplit.Length == 2)
                {

                }
            }
            return SpreadDuration.None;
        }

    }
}
