using System;

namespace Shared
{
    [Serializable]
    public enum SpreadDuration
    {
        ThreeMonth,
        SixMonth,
        NineMonth,
        TwelveMonth,
        FifteenMonth,
        EighteenMonth,
        TwentyOneMonthPlus,
        None
    }

    public enum StrategyType
    {
        Outright,
        Calendar,
        BF,
        DF,
        CF,
        Unkown
    }

    public enum BarDuration
    {
        OneMinute,
        ThreeMinute,
        FiveMinute,
        TenMinute,
        ThirtyMinute,
        Hourly,
        Daily
    }

    public enum ChartQueueMsgType
    {
        RealTimeMarketData,
        HistoricalMarketData,
        Timer,
        ClearAndAddNewSymbol,
        AddNewSymbol,
        RemoveSymbol,
        RemoveSeries,
        SyntheticInstrument,
        RealTimeBookData,
        ChangeDuration,
        ResetChartRange
    }
}