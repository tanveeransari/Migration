using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.InstrumentModels
{
    public class RawInstrument
    {
        public string symbol = "";
        public string productCode = "";
        public string instrumentType = "";
        public string strategyType = "";
        public string exchange = "";
        public string productFamily = "";
        public DateTime productExpiry;

        public double minPriceIncrement;
        public double tickValue;
        public double highLimitPx;
        public double lowLimitPx;

        public int securityID;
        public double priceConvertFactor;
        public string productGroup;

        public RawInstrument(int pSecurityID, string pSymbol, string pProductCode, string pInstrumentType, string pStrategyType,
                            string pExchange, string pProductFamily, DateTime expiry,
                            double pMinPriceIncrement, double pTickValue, double pHighLimitPx, double pLowLimitPx, double priceConvertFactor,
                            string pProductGroup)
        {
            this.symbol = pSymbol;
            this.productCode = pProductCode;
            this.instrumentType = pInstrumentType;
            this.strategyType = pStrategyType;
            this.exchange = pExchange;
            this.productFamily = pProductFamily;
            this.productExpiry = expiry;

            this.minPriceIncrement = pMinPriceIncrement;
            this.tickValue = pTickValue;
            this.highLimitPx = pHighLimitPx;
            this.lowLimitPx = pLowLimitPx;

            this.securityID = pSecurityID;
            this.priceConvertFactor = priceConvertFactor;
            this.productGroup = pProductGroup;
        }
    }
}
