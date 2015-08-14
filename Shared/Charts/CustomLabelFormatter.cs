using System;
using Abt.Controls.SciChart.Visuals.Axes;
using Shared.Utility;
using System.Diagnostics;

namespace Shared.Charts
{
    public class CustomLabelFormatter : NumericLabelProvider
    {
        private PriceConverter formatter;

        public CustomLabelFormatter(PriceConverter pFormatter)
            : base()
        {
            this.formatter = pFormatter;
        }

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

        /// <summary>
        /// Formats a label for the axis from the specified data-value passed in
        /// </summary>
        /// <param name="dataValue">The data-value to format</param>
        /// <returns>
        /// The formatted label string
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override string FormatLabel(IComparable dataValue)
        {
            //int index = (int)Convert.ChangeType(dataValue, typeof(int));
            //if (index >= 0 && index < _xLabels.Length)
            //    return _xLabels[index];

            //return index.ToString();
            try
            {
                if (dataValue is double)
                {
                    return formatter.displayPrice((double)dataValue);
                }
                else if (dataValue == null)
                {
                    return string.Empty;
                }
                else
                {
                    return dataValue.ToString();
                }
            }
            catch (Exception err)
            {
                Alerting.Alert.LoggerHome.GetLogger(this).Error("Error in CustomLabelFormatter::FormatLabel ", err);
                return string.Empty;
            }
        }

        /// <summary>
        /// Formats a label for the cursor, from the specified data-value passed in
        /// </summary>
        /// <param name="dataValue">The data-value to format</param>
        /// <returns>
        /// The formatted cursor label string
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override string FormatCursorLabel(IComparable dataValue)
        {
            //int index = (int)Convert.ChangeType(dataValue, typeof(int));
            //if (index >= 0 && index < _xLabels.Length)
            //    return _xLabels[index];

            //return index.ToString();
            try
            {
                if (dataValue is double)
                {
                    return formatter.displayPrice((double)dataValue);
                }
                else if (dataValue == null)
                {
                    return string.Empty;
                }
                else
                {
                    return dataValue.ToString();
                }
            }
            catch (Exception err)
            {
                Alerting.Alert.LoggerHome.GetLogger(this).Error("Error in CustomLabelFormatter::FormatCursorLabel ", err);
                return string.Empty;
            }
        }
    }
}
