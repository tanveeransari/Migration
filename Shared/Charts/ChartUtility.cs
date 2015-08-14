using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Model.DataSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Visuals.RenderableSeries;

namespace Shared.Charts
{
    public class ChartUtility
    {
        public static int GetTimeDuration(BarDuration duration)
        {
            switch (duration)
            {
                case BarDuration.OneMinute:
                    return 1;
                case BarDuration.ThreeMinute:
                    return 3;
                case BarDuration.FiveMinute:
                    return 5;
                case BarDuration.TenMinute:
                    return 10;
                case BarDuration.ThirtyMinute:
                    return 30;
                case BarDuration.Hourly:
                    return 60;
                case BarDuration.Daily:
                    return 1440;
                default:
                    return 0;
            }
        }

        public static BarDuration GetTimeDuration(int minutes)
        {
            switch (minutes)
            {
                case 1:
                    return BarDuration.OneMinute;
                case 3:
                    return BarDuration.ThreeMinute;
                case 5:
                    return BarDuration.FiveMinute;
                case 10:
                    return BarDuration.TenMinute;
                case 30:
                    return BarDuration.ThirtyMinute;
                case 60:
                    return BarDuration.Hourly;
                case 1379:
                case 1440:
                    return BarDuration.Daily;
                default:
                    return BarDuration.OneMinute;
            }
        }

        public static int GetNumberOfBars(BarDuration minutes)
        {
            switch (minutes)
            {
                case BarDuration.OneMinute:
                    return 7200;    // 5 days
                case BarDuration.ThreeMinute:
                    return 7200;    // 15 days
                case BarDuration.FiveMinute:
                    return 7200;    // 25 days
                case BarDuration.TenMinute:
                    return 8640;    // 60 days
                case BarDuration.ThirtyMinute:
                    return 7200;    // 150 days
                case BarDuration.Hourly:
                    return 5040;    // 210 days
                case BarDuration.Daily:
                    return 730;     // 2 years
                default:
                    return 100;
            }
        }

        public static int GetNumberOfDays(BarDuration minutes)
        {
            switch (minutes)
            {
                case BarDuration.OneMinute:
                    return 5;
                case BarDuration.ThreeMinute:
                    return 15;
                case BarDuration.FiveMinute:
                    return 25;
                case BarDuration.TenMinute:
                    return 60;
                case BarDuration.ThirtyMinute:
                    return 150;
                case BarDuration.Hourly:
                    return 210;
                case BarDuration.Daily:
                    return 730;     // 2 years
                default:
                    return 100;
            }
        }


        public static string GetBarDuration(BarDuration duration)
        {
            string durName = "";
            switch (duration)
            {
                case BarDuration.OneMinute:
                    durName = "1 Minute";
                    break;
                case BarDuration.ThreeMinute:
                    durName = "3 Minute";
                    break;
                case BarDuration.FiveMinute:
                    durName = "5 Minute";
                    break;
                case BarDuration.TenMinute:
                    durName = "10 Minute";
                    break;
                case BarDuration.ThirtyMinute:
                    durName = "30 Minute";
                    break;
                case BarDuration.Hourly:
                    durName = "60 Minute";
                    break;
                case BarDuration.Daily:
                    durName = "Daily";
                    break;
            }
            return durName;
        }

        public static bool GetBucket(DateTime tradeDate, BarDuration duration, out DateTime retDate)
        {
            retDate = new DateTime();
            bool valid = true;

            try
            {
                int tMin = tradeDate.Minute;
                int iRet = 0;
                switch (duration)
                {
                    case BarDuration.OneMinute:
                        iRet = tradeDate.Minute;
                        retDate = new DateTime(tradeDate.Year, tradeDate.Month, tradeDate.Day, tradeDate.Hour, tradeDate.Minute, 0);
                        break;

                    case BarDuration.ThreeMinute:
                        iRet = tMin + (3 - tMin % 3);
                        break;

                    case BarDuration.FiveMinute:
                        iRet = tMin + (5 - tMin % 5);
                        break;

                    case BarDuration.TenMinute:
                        iRet = tMin + (10 - tMin % 10);
                        break;

                    case BarDuration.ThirtyMinute:
                        iRet = tMin + (30 - tMin % 30);
                        break;

                    case BarDuration.Hourly:
                        iRet = tMin + (60 - tMin % 60);
                        break;

                    case BarDuration.Daily:
                        if (tradeDate.Hour < 17)
                        {
                            iRet = tradeDate.Day;
                            retDate = new DateTime(tradeDate.Year, tradeDate.Month, tradeDate.Day, 0, 0, 0);
                        }
                        else
                        {
                            DateTime tDate = tradeDate.AddDays(1);
                            retDate = new DateTime(tDate.Year, tDate.Month, tDate.Day, 0, 0, 0);
                        }
                        break;
                }

                if (duration != BarDuration.OneMinute && duration != BarDuration.Daily)
                {
                    if (iRet == 60)
                        iRet = 0;

                    retDate = new DateTime(tradeDate.Year, tradeDate.Month, tradeDate.Day, tradeDate.Hour, iRet, 0);
                    if (iRet == 0)
                        retDate = retDate.AddHours(1);
                }
                if (retDate == DateTime.MinValue)
                    valid = false;
            }
            catch (Exception err)
            {
                Alerting.Alert.LoggerHome.GetLogger(new ChartUtility()).Error("Error in GetBucket " + tradeDate.ToString() + "," + duration.ToString(), err);
                valid = false;
            }

            return valid;
        }

        public static System.Windows.Style GetLineStyle(string color, int strokeThickness)
        {
            if (color == "")
                color = "FFFFFFFF";

            System.Windows.Style retStyle = new System.Windows.Style(typeof(FastLineRenderableSeries));
            var pColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color);
            retStyle.Setters.Add(new Setter(FastLineRenderableSeries.SeriesColorProperty, pColor));
            retStyle.Setters.Add(new Setter(FastLineRenderableSeries.StrokeThicknessProperty, strokeThickness));

            return retStyle;

        }

        public static DoubleRange GetPriceVisibleRange(IOhlcDataSeries<DateTime, double> series, IRange xRange)
        {
            try
            {
                var yRange = series.GetWindowedYRange(xRange);
                double? max = yRange.Max as double?;
                double? min = yRange.Min as double?;

                DoubleRange newRange = new DoubleRange();
                if (max != null && min != null)
                {
                    double maxD = (double)max;
                    double minD = (double)min;
                    double diff = (maxD - minD) * .1;

                    if (diff < 1 && maxD < 10)
                    {
                        newRange = new DoubleRange(-1, 11);
                    }
                    else
                        newRange = new DoubleRange(minD - diff, maxD + diff);
                }
                return newRange;
            }
            catch (Exception err)
            {
                //LoggerHome.GetLogger(new ChartUtility()).Error("Error in GetPriceVisibleRange : ", err);
                throw new Exception("Error in GetPriceVisibleRange : ", err);
                return new DoubleRange(0, 10);
            }
        }

        public static double GetAverageVisibleValue(IOhlcDataSeries<DateTime, double> series)
        {
            double average = 0.0;
            try
            {
                if (series.Count > 150)
                {
                    var high150Values = series.HighValues.Skip(Math.Max(0, series.HighValues.Count - 150)).Take(150);
                    average = high150Values.Average();
                }
                else
                {
                    average = series.HighValues.Average();
                }
            }
            catch (Exception err)
            {

                throw new Exception("Error in GetAverageVisibleValue : ", err);
            }
            return average;
        }


    }
}
