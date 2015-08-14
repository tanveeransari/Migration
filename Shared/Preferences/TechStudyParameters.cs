using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Preferences
{
    [Serializable]
    public class TechStudyParameters
    {
        public int ADX_TimePeriod = 14;
        public string ADX_LineColor = "#FFFFFFAE";

        public int ATR_TimePeriod = 14;
        public string ATR_LineColor = "#FFFFFFAE";

        public int CCI_TimePeriod = 20;
        public string CCI_LineColor = "#FFFFFFAE";

        public string Fibonacci_LineColor = "#FF0000";

        public int BB_TimePeriod = 20;
        public string BB_UpperLineColor = "#FFFFFFAE";
        public string BB_MiddleLineColor = "#FF0000";
        public string BB_LowerLineColor = "#FFFFFFAE";
        public double BB_StandardDeviation = 2.0;

        public int LR_TimePeriod = 20;
        public string LR_LineColor = "#FFFFFFAE";

        public int RSI_TimePeriod = 14;
        public string RSI_LineColor = "#FFFFFFAE";

        public int SMA_TimePeriod = 20;
        public string SMA_LineColor = "#FFFFFFAE";
        public SMAType SMA_Type = SMAType.Simple;

        public int VWAP_TimePeriod = 20;
        public string VWAP_LineColor = "#FFFFFFAE";

        public int MACD_EMA1 = 12;
        public int MACD_EMA2 = 26;
        public int MACD_EMA3 = 9;
        public string MACD_LineColor = "#FFFFFFAE";
        public string MACD_SignalColor = "#FF0000";

        public int STOCH_TimePeriodOne = 14;
        public int STOCH_TimePeriodTwo = 3;
        public string STOCH_LineColorOne = "#FFFFFFAE";
        public string STOCH_LineColorTwo = "#FF0000";

        public double SAR_TimePeriodOne = 0.02;
        public double SAR_TimePeriodTwo = 0.20;

        public string TREND_LineColor = "#FFFF6600";

    }


    public enum SMAType
    {
        Simple,
        Exponential
    }
}
