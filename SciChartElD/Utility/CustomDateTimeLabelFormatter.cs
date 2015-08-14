using System;
using Abt.Controls.SciChart.Visuals.Axes;
using Alerting.Alert;

namespace SciChartElD.Utility
{
    public class CustomDateTimeLabelFormatter : TradeChartAxisLabelProvider
    {
        public override string FormatCursorLabel(IComparable dataValue)
        {
            try
            {
                return base.FormatCursorLabel(dataValue);
            }
            catch (Exception err)
            {
                LoggerHome.GetLogger(this).Error("Error in CustomDateLabelFormatter:FormatCursorLabel " + dataValue.ToString(), err);
                return string.Empty;
            }
        }

        public override string FormatLabel(IComparable dataValue)
        {
            try
            {
                return base.FormatLabel(dataValue);
            }
            catch (Exception err)
            {
                LoggerHome.GetLogger(this).Error("Error in CustomDateLabelFormatter::FormatLabel " + dataValue.ToString(), err);
                return string.Empty;
            }
        }
    }
}
