using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abt.Controls.SciChart.Visuals.Axes;
using Alerting.Alert;

namespace SciChartElD.Utility
{
    public class CustomDateLabelFormatter : DateTimeLabelProvider
    {
        /// <summary>
        /// Called when the label formatted is initialized as it is attached to the parent axis, with the parent axis instance
        /// </summary>
        /// <param name="parentAxis">The parent <see cref="T:Abt.Controls.SciChart.IAxis" /> instance</param>
        public override void Init(IAxis parentAxis)
        {
            base.Init(parentAxis);
        }

        /// <summary>
        /// Called at the start of an axis render pass, before any labels are formatted for the current draw operation
        /// </summary>
        public override void OnBeginAxisDraw()
        {
        }

        public override string FormatCursorLabel(IComparable dataValue)
        {
            try
            {
                DateTime sTemp = (DateTime)dataValue;

                return sTemp.ToShortDateString();
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
                DateTime sTemp = (DateTime)dataValue;

                return sTemp.ToShortDateString();
            }
            catch (Exception err)
            {
                LoggerHome.GetLogger(this).Error("Error in CustomDateLabelFormatter::FormatLabel " + dataValue.ToString(), err);
                return string.Empty;
            }
        }
    }
}
