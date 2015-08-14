using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Shared.Utility;
using System.Threading;
//using Oro;
#if TRADER
using Trader.Properties;
#endif
#if CHARTS
using Charts.Properties;
#endif

namespace Shared.InstrumentModels
{
    /*
    internal class InstrumentHelper
    {
        private static InstrumentHelper _instrumentHelper;

        private ProductFamily futureProducts = new ProductFamily("Futures");
        private ProductFamily optionProducts = new ProductFamily("Options");
        private Dictionary<string, ProductFamily> prodFamily = new Dictionary<string, ProductFamily>();

        private Dictionary<string, Instrument> instrumentCache = new Dictionary<string, Instrument>();
        private Dictionary<string, List<Tuple<string, int>>> legList = new Dictionary<string, List<Tuple<string, int>>>();
        private Dictionary<string, List<Instrument>> productToInstrumentCache = new Dictionary<string, List<Instrument>>();
        private Dictionary<string, InstrumentLevels> settlesDB = new Dictionary<string, InstrumentLevels>();
        private Dictionary<string, InstrumentLevels> highLowDB = new Dictionary<string, InstrumentLevels>();
        private Dictionary<string, double> closePxDB = new Dictionary<string, double>();
        private Dictionary<string, double> settlePxDB = new Dictionary<string,double>();
        private Dictionary<string, List<string>> symbolToSpreadMap = new Dictionary<string, List<string>>();

        private Dictionary<string, List<string>> flysToSpreadsMap = new Dictionary<string, List<string>>();

        private List<string> flyList = new List<string>();
        private Dictionary<string, List<string>> outrightToFlyList = new Dictionary<string, List<string>>();
        private List<string> chartSymbolList = new List<string>();

        public static InstrumentHelper getInstance()
        {
            if (_instrumentHelper == null)
            {
                _instrumentHelper = new InstrumentHelper();
            }
            return _instrumentHelper;
        }

        /// <summary>
        /// TA
        /// </summary>
        public InstrumentHelper()
        {
            var dbMock = new DbMock();
            List<RawInstrument> rawCache = dbMock.GetRawInstruments();
            this.legList = dbMock.GetSpreadLegs(out symbolToSpreadMap);
            this.InitalizeCache(rawCache);

            settlesDB = dbMock.GetSettleLevels();
            highLowDB = dbMock.GetHighLowLevels();

            closePxDB = dbMock.GetCloseValues(out settlePxDB);
            this.flyList = dbMock.GetConsecutiveFlys();
            this.chartSymbolList = dbMock.GetChartSymbols();
        }

        public void Initialize(HardWorker hwd)
        {
            DBHelper dbMgr = DBHelper.getInstance();
            hwd.OnProgressChanged(10);
            List<RawInstrument> rawCache = dbMgr.GetRawInstruments();
            hwd.OnProgressChanged(40);
            this.legList = dbMgr.GetSpreadLegs(out symbolToSpreadMap);
            hwd.OnProgressChanged(50);
            prodFamily.Add("Future", futureProducts);
            prodFamily.Add("Options", optionProducts);

            this.InitalizeCache(rawCache);
            hwd.OnProgressChanged(60);

            this.ProcessSpreadDurations();

            settlesDB = dbMgr.GetSettleLevels();
            highLowDB = dbMgr.GetHighLowLevels();
            hwd.OnProgressChanged(70);

            closePxDB = dbMgr.GetCloseValues(out settlePxDB);
            this.flyList = dbMgr.GetConsecutiveFlys();
            
            hwd.OnProgressChanged(80);

            this.ProcessFlyList();

            this.chartSymbolList = dbMgr.GetChartSymbols();
            //Console.WriteLine(futureProducts.ToString());
        }

        //public void ReloadClosePrices()
        //{
        //    Thread dbThread = new Thread(() => this.ReloadPrices());
        //    dbThread.Start();
        //}

        //private void ReloadPrices()
        //{
        //    DBHelper dbMgr = DBHelper.getInstance();
        //    settlesDB = dbMgr.GetSettleLevels();
        //    highLowDB = dbMgr.GetHighLowLevels();
        //    this.closePxDB = dbMgr.GetCloseValues(out settlePxDB);
        //    Trader.MarketGrid.MarketDisplayCache.getInstance().QueueReloadMsg();
        //}

        public Instrument GetInstrument(string instrumentName)
        {
            Instrument ins;
            if (instrumentName != null && this.instrumentCache.TryGetValue(instrumentName, out ins))
                return ins;
            else
                return null;
        }

        public Instrument GetComplexInstrument(List<string> componentList)
        {
            Instrument ins = new Instrument();
            ins.instrumentName = "Complex";
            foreach (string key in this.legList.Keys)
            {
                List<string> pList = this.legList[key].Select(x => x.Item1).ToList();
                
                var newData = pList.Intersect(componentList);
                if (newData.Count() == pList.Count && pList.Count == componentList.Count)
                {
                    if (this.instrumentCache.ContainsKey(key))
                    {
                        Instrument pInstrument = this.instrumentCache[key];
                        ins = null;
                        return pInstrument;
                    }
                }
            }
            return ins;
        }

        public List<string> GetComplexInstrumentList(List<string> componentList)
        {
            List<string> iList = new List<string>();
            foreach (string key in this.legList.Keys)
            {
                if (this.legList[key].Count == componentList.Count && this.instrumentCache.ContainsKey(key))
                {
                    List<string> pList = this.legList[key].Select(x => x.Item1).ToList();
                    var newData = pList.Intersect(componentList);
                    if (newData.Count() == pList.Count )
                    {
                        Instrument pInstrument = this.instrumentCache[key];
                        iList.Add(pInstrument.instrumentName);
                    }
                }
            }

            foreach (string key in this.flysToSpreadsMap.Keys)
            {
                if (this.flysToSpreadsMap[key].Count == componentList.Count && this.instrumentCache.ContainsKey(key))
                {
                    List<string> pList = this.flysToSpreadsMap[key].ToList();
                    var newData = pList.Intersect(componentList);

                    if (newData.Count() == pList.Count)
                    {
                        Instrument pInstrument = this.instrumentCache[key];
                        iList.Add(pInstrument.instrumentName);
                    }
                }
            }

            return iList;
        }

        public InstrumentLevels GetCloseSettleLevels(string instrumentName)
        {
            InstrumentLevels ins;
            if (this.settlesDB.TryGetValue(instrumentName, out ins))
            {
                return ins;
            }
            else ins = new InstrumentLevels();

            return ins;
        }

        public InstrumentLevels GetHighLowLevels(string instrumentName)
        {
            InstrumentLevels ins;
            if (this.highLowDB.TryGetValue(instrumentName, out ins))
            {
                return ins;
            }
            else ins = new InstrumentLevels();

            return ins;
        }

        public bool IsSpreadInstrument(string instrument)
        {
            return this.legList.ContainsKey(instrument);
        }

        public string GetStrategyType(string instrument)
        {
            if (this.instrumentCache.ContainsKey(instrument))
                return this.instrumentCache[instrument].strategyType;

            return "";
        }

        public double GetClosePx(string instrument)
        {
            double closePx;
            if (this.closePxDB.ContainsKey(instrument))
                closePx = this.closePxDB[instrument];
            else
                closePx = 0.0;

            return closePx;
        }

        public double GetPrevSettlePx(string instrument)
        {
            double settlePx;
            if (this.settlePxDB.ContainsKey(instrument))
                settlePx = this.settlePxDB[instrument];
            else
                settlePx = 0.0;

            return settlePx;
        }

        public bool IsInstrumentChartable(string instrument)
        {
            bool isChartable = false;

            if (this.instrumentCache.ContainsKey(instrument))
            {
                Instrument ins = this.instrumentCache[instrument];
                if (ins != null)
                {
                    if( this.chartSymbolList.Contains(ins.productGroup))
                        return true;
                }
            }

            return isChartable;
        }


        private void InitalizeCache(List<RawInstrument> rawCache)
        {
            this.instrumentCache.Clear();
            
            List<string> singleSessionInstrumentList = DBHelper.getInstance().GetSingleSessionInstruments();

            foreach (RawInstrument ri in rawCache)
            {
                Instrument ins = new Instrument(ri.securityID, ri.symbol, ri.productExpiry, ri.minPriceIncrement, ri.tickValue, ri.highLimitPx,
                    ri.lowLimitPx, ri.priceConvertFactor, ri.exchange, ri.productCode, ri.instrumentType, ri.strategyType,ri.productGroup);
                if (this.instrumentCache.ContainsKey(ins.instrumentName) == false)
                    this.instrumentCache.Add(ins.instrumentName, ins);

                if (singleSessionInstrumentList.Contains(ri.productCode + "," + ri.strategyType))
                    ins.isSingleSessionTraded = true;

                //Check if future or option and update corresponding product family
                if (ri.instrumentType == "Option")
                    optionProducts.AddRawInstrument(ri, ins);
                else
                    futureProducts.AddRawInstrument(ri, ins);

                if (ri.strategyType == "Outright" && ri.instrumentType == "Future")
                {
                    if (this.productToInstrumentCache.ContainsKey(ri.productCode) == false)
                    {
                        List<Instrument> pList = new List<Instrument>();
                        pList.Add(ins);
                        this.productToInstrumentCache.Add(ri.productCode, pList);
                    }
                    else
                    {
                        List<Instrument> rList = this.productToInstrumentCache[ri.productCode];
                        rList.Add(ins);
                    }
                }
            }

            foreach (var list in productToInstrumentCache.Values)
            {
                list.TrimExcess();
            }
        }

        public List<ProductFamily> GetProductFamilies()
        {
            return new List<ProductFamily>(prodFamily.Values);
        }

        public List<Instrument> GetInstrumentForProduct(string productCode)
        {
            List<Instrument> instList = new List<Instrument>();
            if (this.productToInstrumentCache.ContainsKey(productCode))
            {
                instList = this.productToInstrumentCache[productCode];
            }
            return instList;
        }

        public Image GetHighImageForPrice(string instrument, double dPrice)
        {
            Image levelImage = null;
            InstrumentLevels levels;
            if (this.highLowDB.TryGetValue(instrument, out levels))
            {
                if (dPrice >= levels.oneYearHigh)
                    return Resources.High1y;
                else if (dPrice >= levels.nineMonthHigh)
                    return Resources.High9m;
                else if (dPrice >= levels.sixMonthHigh)
                    return Resources.High6m;
                else if (dPrice >= levels.threeMonthHigh)
                    return Resources.High3m;
                else if (dPrice >= levels.oneMonthHigh)
                    return Resources.High1m;
                else if (dPrice >= levels.threeWeekHigh)
                    return Resources.High3w;
                else if (dPrice >= levels.twoWeekHigh)
                    return Resources.High2w;
                else if (dPrice >= levels.oneWeekHigh)
                    return Resources.High1w;
            }
            return levelImage;
        }

        public string GetHighImageStringForPrice(string instrument, double dPrice)
        {
            InstrumentLevels levels;
            if (this.highLowDB.TryGetValue(instrument, out levels))
            {
                if (dPrice >= levels.oneYearHigh)
                    return "High1y";
                else if (dPrice >= levels.nineMonthHigh)
                    return "High9m";
                else if (dPrice >= levels.sixMonthHigh)
                    return "High6m";
                else if (dPrice >= levels.threeMonthHigh)
                    return "High3m";
                else if (dPrice >= levels.oneMonthHigh)
                    return "High1m";
                else if (dPrice >= levels.threeWeekHigh)
                    return "High3w";
                else if (dPrice >= levels.twoWeekHigh)
                    return "High2w";
                else if (dPrice >= levels.oneWeekHigh)
                    return "High1w";
            }
            return "";
        }

        public Image GetLowImageForPrice(string instrument, double dPrice)
        {
            Image levelImage = null;
            InstrumentLevels levels;
            if (this.highLowDB.TryGetValue(instrument, out levels))
            {
                if (dPrice <= levels.oneYearLow)
                    return Resources.Low1y;
                else if (dPrice <= levels.nineMonthLow)
                    return Resources.Low9m;
                else if (dPrice <= levels.sixMonthLow)
                    return Resources.Low6m;
                else if (dPrice <= levels.threeMonthLow)
                    return Resources.Low3m;
                else if (dPrice <= levels.oneMonthLow)
                    return Resources.Low1m;
                else if (dPrice <= levels.threeWeekLow)
                    return Resources.Low3w;
                else if (dPrice <= levels.twoWeekLow)
                    return Resources.Low2w;
                else if (dPrice <= levels.oneWeekLow)
                    return Resources.Low1w;
            }
            return levelImage;
        }

        public string GetLowImageStringForPrice(string instrument, double dPrice)
        {
            InstrumentLevels levels;
            if (this.highLowDB.TryGetValue(instrument, out levels))
            {
                if (dPrice <= levels.oneYearLow)
                    return "Low1y";
                else if (dPrice <= levels.nineMonthLow)
                    return "Low9m";
                else if (dPrice <= levels.sixMonthLow)
                    return "Low6m";
                else if (dPrice <= levels.threeMonthLow)
                    return "Low3m";
                else if (dPrice <= levels.oneMonthLow)
                    return "Low1m";
                else if (dPrice <= levels.threeWeekLow)
                    return "Low3w";
                else if (dPrice <= levels.twoWeekLow)
                    return "Low2w";
                else if (dPrice <= levels.oneWeekLow)
                    return "Low1w";
            }
            return "";
        }

        public Image GetImageForSettlePrice(string instrument, decimal settlePrice)
        {
            double dPrice = Convert.ToDouble(settlePrice);
            Image levelImage = null;
            InstrumentLevels levels;
            if (this.settlesDB.TryGetValue(instrument, out levels))
            {
                if (dPrice >= levels.oneYearHigh)
                    return Resources.High1y;
                else if (dPrice >= levels.nineMonthHigh)
                    return Resources.High9m;
                else if (dPrice >= levels.sixMonthHigh)
                    return Resources.High6m;
                else if (dPrice >= levels.threeMonthHigh)
                    return Resources.High3m;
                else if (dPrice >= levels.oneMonthHigh)
                    return Resources.High1m;
                else if (dPrice >= levels.threeWeekHigh)
                    return Resources.High3w;
                else if (dPrice >= levels.twoWeekHigh)
                    return Resources.High2w;
                else if (dPrice >= levels.oneWeekHigh)
                    return Resources.High1w;
                else if (dPrice <= levels.oneYearLow)
                    return Resources._1y_red;
                else if (dPrice <= levels.nineMonthLow)
                    return Resources.Low9m;
                else if (dPrice <= levels.sixMonthLow)
                    return Resources.Low6m;
                else if (dPrice <= levels.threeMonthLow)
                    return Resources.Low3m;
                else if (dPrice <= levels.oneMonthLow)
                    return Resources.Low1m;
                else if (dPrice <= levels.threeWeekLow)
                    return Resources.Low3w;
                else if (dPrice <= levels.twoWeekLow)
                    return Resources.Low2w;
                else if (dPrice <= levels.oneWeekLow)
                    return Resources._1w_red;
            }
            return levelImage;
        }

        public string GetImageStringForSettlePrice(string instrument, double settlePrice)
        {
            if (instrument != null)
            {
                double dPrice = Convert.ToDouble(settlePrice);
                InstrumentLevels levels;
                if (this.settlesDB.TryGetValue(instrument, out levels))
                {
                    if (dPrice >= levels.oneYearHigh)
                        return "High1y";
                    else if (dPrice >= levels.nineMonthHigh)
                        return "High9m";
                    else if (dPrice >= levels.sixMonthHigh)
                        return "High6m";
                    else if (dPrice >= levels.threeMonthHigh)
                        return "High3m";
                    else if (dPrice >= levels.oneMonthHigh)
                        return "High1m";
                    else if (dPrice >= levels.threeWeekHigh)
                        return "High3w";
                    else if (dPrice >= levels.twoWeekHigh)
                        return "High2w";
                    else if (dPrice >= levels.oneWeekHigh)
                        return "High1w";
                    else if (dPrice <= levels.oneYearLow)
                        return "Low1y";
                    else if (dPrice <= levels.nineMonthLow)
                        return "Low9m";
                    else if (dPrice <= levels.sixMonthLow)
                        return "Low6m";
                    else if (dPrice <= levels.threeMonthLow)
                        return "Low3m";
                    else if (dPrice <= levels.oneMonthLow)
                        return "Low1m";
                    else if (dPrice <= levels.threeWeekLow)
                        return "Low3w";
                    else if (dPrice <= levels.twoWeekLow)
                        return "Low2w";
                    else if (dPrice <= levels.oneWeekLow)
                        return "Low1w";
                }
            }
            return "";
        }

        public void SetSettleDataFromHalo(halo.SettleMsg sMsg)
        {
            string instrument = sMsg.securityName;
            InstrumentLevels highLowLevels = new InstrumentLevels(
                                sMsg.highLowLevels[(int)halo.TimePeriod.TP_1_WEEK].low,
                                sMsg.highLowLevels[(int)halo.TimePeriod.TP_1_WEEK].high,
                                sMsg.highLowLevels[(int)halo.TimePeriod.TP_2_WEEK].low,
                                sMsg.highLowLevels[(int)halo.TimePeriod.TP_2_WEEK].high,
                                sMsg.highLowLevels[(int)halo.TimePeriod.TP_3_WEEK].low,
                                sMsg.highLowLevels[(int)halo.TimePeriod.TP_3_WEEK].high,
                                sMsg.highLowLevels[(int)halo.TimePeriod.TP_1_MONTH].low,
                                sMsg.highLowLevels[(int)halo.TimePeriod.TP_1_MONTH].high,
                                sMsg.highLowLevels[(int)halo.TimePeriod.TP_3_MONTH].low,
                                sMsg.highLowLevels[(int)halo.TimePeriod.TP_3_MONTH].high,
                                sMsg.highLowLevels[(int)halo.TimePeriod.TP_6_MONTH].low,
                                sMsg.highLowLevels[(int)halo.TimePeriod.TP_6_MONTH].high,
                                sMsg.highLowLevels[(int)halo.TimePeriod.TP_9_MONTH].low,
                                sMsg.highLowLevels[(int)halo.TimePeriod.TP_9_MONTH].high,
                                sMsg.highLowLevels[(int)halo.TimePeriod.TP_1YEAR].low,
                                sMsg.highLowLevels[(int)halo.TimePeriod.TP_1YEAR].high);

            this.highLowDB[instrument] = highLowLevels;

            InstrumentLevels settleLevels = new InstrumentLevels(sMsg.exoticSettleLevels[(int)halo.TimePeriod.TP_1_WEEK].low,
                                                            sMsg.exoticSettleLevels[(int)halo.TimePeriod.TP_1_WEEK].high,
                                                            sMsg.exoticSettleLevels[(int)halo.TimePeriod.TP_2_WEEK].low,
                                                            sMsg.exoticSettleLevels[(int)halo.TimePeriod.TP_2_WEEK].high,
                                                            sMsg.exoticSettleLevels[(int)halo.TimePeriod.TP_3_WEEK].low,
                                                            sMsg.exoticSettleLevels[(int)halo.TimePeriod.TP_3_WEEK].high,
                                                            sMsg.exoticSettleLevels[(int)halo.TimePeriod.TP_1_MONTH].low,
                                                            sMsg.exoticSettleLevels[(int)halo.TimePeriod.TP_1_MONTH].high,
                                                            sMsg.exoticSettleLevels[(int)halo.TimePeriod.TP_3_MONTH].low,
                                                            sMsg.exoticSettleLevels[(int)halo.TimePeriod.TP_3_MONTH].high,
                                                            sMsg.exoticSettleLevels[(int)halo.TimePeriod.TP_6_MONTH].low,
                                                            sMsg.exoticSettleLevels[(int)halo.TimePeriod.TP_6_MONTH].high,
                                                            sMsg.exoticSettleLevels[(int)halo.TimePeriod.TP_9_MONTH].low,
                                                            sMsg.exoticSettleLevels[(int)halo.TimePeriod.TP_9_MONTH].high,
                                                            sMsg.exoticSettleLevels[(int)halo.TimePeriod.TP_1YEAR].low,
                                                            sMsg.exoticSettleLevels[(int)halo.TimePeriod.TP_1YEAR].high);

            this.settlesDB[instrument] = settleLevels;

            this.settlePxDB[instrument] = sMsg.settles.settle;
            this.closePxDB[instrument] = sMsg.settles.close;

        }

        private void ProcessFlyList()
        {
            foreach (string flySymbol in this.flyList)
            {
                List<Tuple<string, int>> legList;
                if (this.legList.TryGetValue(flySymbol, out legList))
                {
                    if (legList.Count == 3)
                    {
                        string uLeg = legList[1].Item1;
                        if (this.outrightToFlyList.ContainsKey(uLeg))
                        {
                            List<string> parentList = this.outrightToFlyList[uLeg];
                            if (parentList.Contains(flySymbol) == false)
                                parentList.Add(flySymbol);
                        }
                        else
                        {
                            List<string> parentList = new List<string>();
                            parentList.Add(flySymbol);
                            this.outrightToFlyList[uLeg] = parentList;
                        }
                    }

                    //foreach (string leg in legList)
                    //{
                    //    if (this.outrightToFlyList.ContainsKey(leg))
                    //    {
                    //        List<string> parentList = this.outrightToFlyList[leg];
                    //        if (parentList.Contains(flySymbol) == false)
                    //            parentList.Add(flySymbol);
                    //    }
                    //    else
                    //    {
                    //        List<string> parentList = new List<string>();
                    //        parentList.Add(flySymbol);
                    //        this.outrightToFlyList[leg] = parentList;
                    //    }
                    //}
                }
            }
        }

        private void ProcessSpreadDurations()
        {
            foreach (KeyValuePair<string, List<Tuple<string, int>>> kvp in this.legList)
            {
                Instrument ins = this.GetInstrument(kvp.Key);
                if (ins != null)
                {
                    if (kvp.Key.IndexOf("-") > 0 && kvp.Value.Count == 2)
                    {
                        ins.spreadDuration = this.GetSpreadDuration(kvp.Value);
                    }
                    else if (kvp.Value.Count == 3 && kvp.Key.IndexOf(":BF") > 0)
                    {
                        ins.spreadDuration = this.GetButterflySpreadDuration(kvp.Value);
                        this.ProcessSpreadBreakdownForButterfly(ins.instrumentName,ins.productCode);
                    }
                    else if (kvp.Value.Count == 4 && (kvp.Key.IndexOf(":DF") > 0 ||  kvp.Key.IndexOf(":CF") > 0))
                    {
                        ins.spreadDuration = this.GetDoubleFlySpreadDuration(kvp.Value);
                        this.ProcesSpreadBreakDownForDoubleFlysAndCondors(ins.instrumentName, ins.productCode);
                        //System.Diagnostics.Debug.WriteLine(ins.instrumentName + "," + ins.spreadDuration);
                    }
                }
            }
        }


        private void ProcessSpreadBreakdownForButterfly(string instrument,string productCode)
        {
            string[] tStr = instrument.Split(' ');
            if (tStr.Length == 2)
            {
                string legs = tStr[1];
                string[] tLegs = legs.Split('-');
                if (tLegs.Length == 3)
                {
                    List<string> legList = new List<string>(2);
                    legList.Add(productCode + tLegs[0] + "-" + productCode + tLegs[1]);
                    legList.Add(productCode + tLegs[1] + "-" + productCode + tLegs[2]);

                    this.flysToSpreadsMap[instrument] = legList;
                }
                else
                {
                    System.Diagnostics.Debug.Write("Invaild Butterfly : " + instrument);
                }
            }
            else 
            {
                System.Diagnostics.Debug.Write("Invaild Butterfly : " + instrument);
            }
        }

        private void ProcesSpreadBreakDownForDoubleFlysAndCondors(string instrument, string productCode)
        {
            if (instrument.IndexOf(":CF") > 0)
            {
                string[] tStr = instrument.Split(' ');
                string legs = tStr[1];
                if (legs.Length == 8)
                {
                    List<string> legList = new List<string>(2);
                    legList.Add(productCode + legs.Substring(0, 2) + "-" + productCode + legs.Substring(2, 2));
                    legList.Add(productCode + legs.Substring(4, 2) + "-" + productCode + legs.Substring(6, 2));

                    this.flysToSpreadsMap[instrument] = legList;
                }
                else
                {
                    System.Diagnostics.Debug.Write("Invalid Condor : " + instrument);
                }
            }
            else if (instrument.IndexOf(":DF") > 0)
            {
                string[] tStr = instrument.Split(' ');
                string legs = tStr[1];
                if (legs.Length == 8)
                {
                    List<string> legList = new List<string>(2);
                    legList.Add(productCode + ":BF " + legs.Substring(0, 2) + "-" + legs.Substring(2, 2) + "-" + legs.Substring(4, 2));
                    legList.Add(productCode + ":BF " + legs.Substring(2, 2) + "-" + legs.Substring(4, 2) + "-" + legs.Substring(6, 2));

                    this.flysToSpreadsMap[instrument] = legList;
                }
                else 
                {
                    System.Diagnostics.Debug.Write("Invalid double fly : " + instrument);
                }
            }

        }

        public List<string> GetSpreadsForFlysAndDoubleFlys(string flySymbol)
        {
            if (this.flysToSpreadsMap.ContainsKey(flySymbol))
                return this.flysToSpreadsMap[flySymbol];
            else
                return null;
        }

        public List<string> GetFlyInstrumentsForOutright(string outrightSymbol)
        {
            List<string> retList;
            if (this.outrightToFlyList.TryGetValue(outrightSymbol, out retList))
                return retList;
            else
                retList = new List<string>();
            return retList;
        }

        public List<Tuple<string, int>> GetSpreadLegList(string spreadSymbol)
        {
            List<Tuple<string, int>> symList;
            if (this.legList.TryGetValue(spreadSymbol, out symList))
                return symList;
            else
                symList = new List<Tuple<string, int>>();

            return symList;
        }

        public List<string> GetSpreadLegListSymbolsOnly(string spreadSymbol)
        {
            if (this.legList.ContainsKey(spreadSymbol))
            {
                return this.legList[spreadSymbol].Select(x => x.Item1).ToList();
            }
            return null;
        }




        public bool IsFly(string instrument)
        {
            if (this.flyList.Contains(instrument))
                return true;
            else
                return false;
        }

        public bool IsFlyLeg(string instrument)
        {
            if (this.outrightToFlyList.ContainsKey(instrument))
                return true;
            else
                return false;
        }

        public StrategyType GetStrategyType(Instrument inst)
        {
            if (inst != null)
            {
                switch (inst.strategyType)
                {
                    case "Outright":
                        return StrategyType.Outright;

                    case "Calendar":
                        return StrategyType.Calendar;

                    case "BF":
                        return StrategyType.BF;

                    case "DF":
                        return StrategyType.DF;

                    case "CF":
                        return StrategyType.CF;
                }
            }

            return StrategyType.Unkown;
        }

        private SpreadDuration GetSpreadDuration(List<Tuple<string,int>> spreadList)
        {
            SpreadDuration spd = SpreadDuration.None;
            if( spreadList.Count == 2)
            {
                Instrument legOne = this.GetInstrument(spreadList[0].Item1);
                Instrument legTwo = this.GetInstrument(spreadList[1].Item1);
                if (legOne != null && legTwo != null)
                {
                    int duration = Utility.TimeHelper.MonthDifference(legTwo.expiry, legOne.expiry);
                    spd = this.GetSpreadDuration(duration);
                }
            }
            return spd;
        }

        private SpreadDuration GetButterflySpreadDuration(List<Tuple<string, int>> flyList)
        {
            SpreadDuration spd = SpreadDuration.None;
            if (flyList.Count == 3)
            {
                Instrument legOne = this.GetInstrument(flyList[0].Item1);
                Instrument legTwo = this.GetInstrument(flyList[1].Item1);
                Instrument legThree = this.GetInstrument(flyList[2].Item1);

                if (legOne != null && legTwo != null && legThree != null)
                {
                    int durOne = Utility.TimeHelper.MonthDifference(legTwo.expiry, legOne.expiry);
                    int durTwo = Utility.TimeHelper.MonthDifference(legThree.expiry, legTwo.expiry);
                    if (durOne == durTwo)
                    {
                        spd = this.GetSpreadDuration(durOne);
                    }
                }
            }
            return spd;
        }

        private SpreadDuration GetDoubleFlySpreadDuration(List<Tuple<string, int>> flyList)
        {
            SpreadDuration spd = SpreadDuration.None;
            if (flyList.Count == 4)
            {
                Instrument legOne = this.GetInstrument(flyList[0].Item1);
                Instrument legTwo = this.GetInstrument(flyList[1].Item1);
                Instrument legThree = this.GetInstrument(flyList[2].Item1);
                Instrument legFour = this.GetInstrument(flyList[3].Item1);

                if (legOne != null && legTwo != null && legThree != null && legFour != null )
                {
                    int durOne = Utility.TimeHelper.MonthDifference(legTwo.expiry, legOne.expiry);
                    int durTwo = Utility.TimeHelper.MonthDifference(legThree.expiry, legTwo.expiry);
                    int durThree = Utility.TimeHelper.MonthDifference(legFour.expiry, legThree.expiry);
                    if (durOne == durTwo && durTwo == durThree)
                    {
                        spd = this.GetSpreadDuration(durOne);
                    }
                }
            }
            return spd;
        }


        private SpreadDuration GetSpreadDuration(int monthDifference)
        {
            if (monthDifference > 21)
                return SpreadDuration.TwentyOneMonthPlus;


            switch(monthDifference)
            {
                case 3:
                    return SpreadDuration.ThreeMonth;
                case 6:
                    return SpreadDuration.SixMonth;
                case 9:
                    return SpreadDuration.NineMonth;
                case 12:
                    return SpreadDuration.TwelveMonth;
                case 15:
                    return SpreadDuration.FifteenMonth;
                case 18:
                    return SpreadDuration.EighteenMonth;
                case 21:
                    return SpreadDuration.TwentyOneMonthPlus;
                default:
                    return SpreadDuration.None;
            }


        }

#region Volume scoreboard
        public List<Instrument> GetDurationSpreads(string product,int duration)
        {
            List<Instrument> sList = new List<Instrument>();
            var insList = this.legList.Where(x => x.Value.Count == 2).Select(x => x.Key).ToList<string>();

            foreach (string spreadKey in insList)
            {
                Instrument ins = this.GetInstrument(spreadKey);
               
                if (ins != null && ins.productCode == product)
                {
                    Instrument legOne = this.GetInstrument(this.legList[spreadKey][0].Item1);
                    Instrument legTwo = this.GetInstrument(this.legList[spreadKey][1].Item1);

                    if (duration == Utility.TimeHelper.MonthDifference(legTwo.expiry, legOne.expiry))
                    {
                        sList.Add(ins);
                    }
                }
                
            }

            return sList;   
        }

        public List<Instrument> GetDurationSpreadsForPlus(string product, int duration)
        {
            List<Instrument> sList = new List<Instrument>();
            var insList = this.legList.Where(x => x.Value.Count == 2).Select(x => x.Key).ToList<string>();

            foreach (string spreadKey in insList)
            {
                Instrument ins = this.GetInstrument(spreadKey);

                if (ins != null && ins.productCode == product)
                {
                    Instrument legOne = this.GetInstrument(this.legList[spreadKey][0].Item1);
                    Instrument legTwo = this.GetInstrument(this.legList[spreadKey][1].Item1);

                    if (duration <= Utility.TimeHelper.MonthDifference(legTwo.expiry, legOne.expiry))
                    {
                        sList.Add(ins);
                    }
                }

            }

            return sList;
        }

        public List<Instrument> GetDurationButterflys(string product,int duration)
        {
            List<Instrument> sList = new List<Instrument>();

            var insList = this.legList.Where(x => x.Value.Count == 3).Select(x => x.Key).ToList<string>();

            foreach (string spreadKey in insList)
            {
                Instrument ins = this.GetInstrument(spreadKey);
                if (ins != null && ins.productCode == product && ins.instrumentName.IndexOf(":BF") > 0)
                {
                    Instrument legOne = this.GetInstrument(this.legList[spreadKey][0].Item1);
                    Instrument legTwo = this.GetInstrument(this.legList[spreadKey][1].Item1);
                    Instrument legThree = this.GetInstrument(this.legList[spreadKey][2].Item1);

                    if( (duration == Utility.TimeHelper.MonthDifference(legTwo.expiry,legOne.expiry)) && (duration == Utility.TimeHelper.MonthDifference(legThree.expiry,legTwo.expiry)) )
                    {
                        sList.Add(ins);
                    }
                }
            }
            return sList;
        }

        public List<Instrument> GetDurationButterflyForPlus(string product, int duration)
        {
            List<Instrument> sList = new List<Instrument>();

            var insList = this.legList.Where(x => x.Value.Count == 3).Select(x => x.Key).ToList<string>();

            foreach (string spreadKey in insList)
            {
                Instrument ins = this.GetInstrument(spreadKey);
                if (ins != null && ins.productCode == product && ins.instrumentName.IndexOf(":BF") > 0)
                {
                    Instrument legOne = this.GetInstrument(this.legList[spreadKey][0].Item1);
                    Instrument legTwo = this.GetInstrument(this.legList[spreadKey][1].Item1);
                    Instrument legThree = this.GetInstrument(this.legList[spreadKey][2].Item1);

                    if ((duration <= Utility.TimeHelper.MonthDifference(legTwo.expiry, legOne.expiry)) && (duration <= Utility.TimeHelper.MonthDifference(legThree.expiry, legTwo.expiry)))
                    {
                        sList.Add(ins);
                    }
                }
            }
            return sList;
        }

#endregion

        public List<Instrument> GetOrderedListOfQuarterlies(string productCode)
        {
            List<Instrument> sList = new List<Instrument>();

            if (this.productToInstrumentCache.ContainsKey(productCode))
            {
                foreach (var ins in this.productToInstrumentCache[productCode])
                {
                    if (ins.expiry.Month == 3 || ins.expiry.Month == 6 || ins.expiry.Month == 9 || ins.expiry.Month == 12)
                    {
                        sList.Add(ins);
                    }
                }

                sList.Sort((x1, x2) => x1.expiry.CompareTo(x2.expiry));
            }

            return sList;
        }

        public List<Instrument> GetSpreadsForInstrument(string instrument, StrategyType stratType)
        {
            List<Instrument> sList = new List<Instrument>();
            if (this.symbolToSpreadMap.ContainsKey(instrument))
            {
                foreach (string symbolName in this.symbolToSpreadMap[instrument])
                {
                    if (this.instrumentCache.ContainsKey(symbolName))
                    {
                        if (this.GetStrategyType(this.instrumentCache[symbolName]) == stratType)
                        {
                            sList.Add(this.instrumentCache[symbolName]);
                        }
                    }
                }
            }
            return sList;
        }




    }
    */
    [Serializable]
    public class ProductFamily
    {
        public string familyName = "";
        private Dictionary<string, ExchangeInfo> exchanges = new Dictionary<string, ExchangeInfo>();

        public ProductFamily(string name)
        {
            this.familyName = name;
        }

        public void AddRawInstrument(RawInstrument ri, InstrumentShared ins)
        {
            ExchangeInfo ei;
            if (exchanges.ContainsKey(ri.exchange))
            {
                ei = this.exchanges[ri.exchange];
            }
            else
            {
                ei = new ExchangeInfo(ri.exchange);
                this.exchanges.Add(ri.exchange, ei);
            }

            ei.AddRawInstrument(ri, ins);
        }

        public List<ExchangeInfo> GetExchanges()
        {
            return new List<ExchangeInfo>(this.exchanges.Values);
        }
    }

    [Serializable]
    public class ExchangeInfo
    {
        public string exchange;
        public Dictionary<string, ProductClassType> productClass = new Dictionary<string, ProductClassType>();

        public ExchangeInfo(string pExchange)
        {
            this.exchange = pExchange;
        }

        public void AddRawInstrument(RawInstrument ri, InstrumentShared ins)
        {
            ProductClassType pi;

            if (this.productClass.ContainsKey(ri.productFamily))
            {
                pi = this.productClass[ri.productFamily];
            }
            else
            {
                pi = new ProductClassType(ri.productFamily);
                this.productClass.Add(ri.productFamily, pi);
            }

            pi.AddRawInstrument(ri, ins);
        }

        public List<ProductClassType> GetProductClassTypes()
        {
            return new List<ProductClassType>(this.productClass.Values);
        }
    }

    [Serializable]
    public class ProductClassType
    {
        public string productClass;
        public Dictionary<string, ProductSymbol> productSymbolList = new Dictionary<string, ProductSymbol>();

        public ProductClassType(string pProductClassType)
        {
            this.productClass = pProductClassType;
        }

        public void AddRawInstrument(RawInstrument ri, InstrumentShared ins)
        {
            ProductSymbol ps;
            if (this.productSymbolList.ContainsKey(ri.productCode))
            {
                ps = this.productSymbolList[ri.productCode];
            }
            else
            {
                ps = new ProductSymbol(ri.productCode);
                this.productSymbolList.Add(ri.productCode, ps);
            }
            ps.AddRawInstrument(ri, ins);
        }

        public List<ProductSymbol> GetProductSymbolList()
        {
            return new List<ProductSymbol>(this.productSymbolList.Values);
        }
    }

    [Serializable]
    public class ProductSymbol
    {
        public string symbolName = "";
        public Dictionary<string, InstrumentType> instrumentTypeCache = new Dictionary<string, InstrumentType>();

        public ProductSymbol(string pName)
        {
            this.symbolName = pName;
        }

        public void AddRawInstrument(RawInstrument ri, InstrumentShared ins)
        {
            InstrumentType insType;
            if (instrumentTypeCache.ContainsKey(ri.strategyType))
            {
                insType = instrumentTypeCache[ri.strategyType];
            }
            else
            {
                insType = new InstrumentType(ri.strategyType);
                this.instrumentTypeCache.Add(ri.strategyType, insType);
            }

            insType.AddRawInstrument(ri, ins);
        }

        public List<InstrumentType> GetInstrumentTypes()
        {
            List<InstrumentType> theList = new List<InstrumentType>();

            if (instrumentTypeCache.ContainsKey("Outright"))
            {
                theList.Add(this.instrumentTypeCache["Outright"]);
            }

            if (instrumentTypeCache.ContainsKey("Calendar"))
            {
                theList.Add(this.instrumentTypeCache["Calendar"]);
            }

            if (instrumentTypeCache.ContainsKey("BF"))
            {
                theList.Add(this.instrumentTypeCache["BF"]);
            }

            if (instrumentTypeCache.ContainsKey("DF"))
            {
                theList.Add(this.instrumentTypeCache["DF"]);
            }

            if (instrumentTypeCache.ContainsKey("CF"))
            {
                theList.Add(this.instrumentTypeCache["CF"]);
            }

            foreach (InstrumentType instType in this.instrumentTypeCache.Values)
            {
                if (theList.Contains(instType) == false)
                    theList.Add(instType);
            }

            return theList;
        }
    }

    public class InstrumentType
    {
        public string instType = "";
        private List<InstrumentShared> instrumentList = new List<InstrumentShared>();
        private Dictionary<string, SplitInstrument> splitList = new Dictionary<string, SplitInstrument>();

        public InstrumentType(string instType)
        {
            this.instType = instType;
        }

        public void AddRawInstrument(RawInstrument ri, InstrumentShared ins)
        {
            this.instrumentList.Add(ins);

            string leadingMonth = " ";
            if (ri.symbol.IndexOf("-") > 0)
            {
                string[] tStr = ri.symbol.Split('-');
                if (tStr.Length > 1)
                {
                    leadingMonth = tStr[0];
                }
            }
            else if (this.instType == "DF" || this.instType == "CF")
            {
                if (ri.symbol.IndexOf(" ") > 0)
                {
                    string[] tStr = ri.symbol.Split(' ');
                    if (tStr.Length > 1)
                    {
                        leadingMonth = tStr[0] + " " + tStr[1].Substring(0, 2);
                    }
                }
            }

            if (leadingMonth != "")
            {
                if (splitList.ContainsKey(leadingMonth) == false)
                {
                    SplitInstrument spIns = new SplitInstrument();
                    spIns.expirationDate = ins.expiry;
                    spIns.name = leadingMonth + "...";
                    spIns.instrumentList = new List<InstrumentShared>();
                    spIns.instrumentList.Add(ins);
                    this.splitList.Add(leadingMonth, spIns);
                }
                else
                {
                    SplitInstrument spIns = splitList[leadingMonth];
                    spIns.instrumentList.Add(ins);
                }
            }
        }

        public List<InstrumentShared> GetInstruments()
        {
            return this.instrumentList;
        }

        public Dictionary<string, SplitInstrument> GetSplitInformation()
        {
            return splitList;
        }
    }

}