using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Abt.Controls.SciChart;
using Abt.Controls.SciChart.ChartModifiers;
using Abt.Controls.SciChart.Utility;
using Abt.Controls.SciChart.Visuals;
using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Visuals.RenderableSeries;

namespace SciChartElD.Utility
{
    public class YAxisDoubleClickChartModifier : ChartModifierBase
    {
        public YAxisDoubleClickChartModifier()
            : base()
        {
            //this.ReceiveHandledEvents = true;
        }

        public override void OnModifierDoubleClick(ModifierMouseArgs e)
        {
            base.OnModifierDoubleClick(e);
            var currentMousePoint = e.MousePoint;
            Point relPnt = this.GetPointRelativeTo(currentMousePoint, base.YAxis);

            if (relPnt != null && relPnt.X >= 0)
            {
                Debug.WriteLine("User clicked on axis. Relative position {0}:{1}", relPnt.X, relPnt.Y);
                Debug.WriteLine("Y Axis Visible Range {0}:{1} ", this.YAxis.VisibleRange.Min, this.YAxis.VisibleRange.Max);
                Debug.WriteLine("Y Axis Data Range {0}:{1} ", this.YAxis.DataRange.Min, this.YAxis.DataRange.Max);

                var frstChrtSer = ParentSurface.RenderableSeries.FirstOrDefault();

                double currentYValue = frstChrtSer.YAxis.GetCurrentCoordinateCalculator().GetDataValue(currentMousePoint.Y);

                if (frstChrtSer.XAxis.IsCategoryAxis) Debug.WriteLine("Series X Axis is category axis");
                //frstChrtSer.XAxis.DataRange
                var horizLine = new XyDataSeries<DateTime, double>();
                if (frstChrtSer.DataSeries.XValues.Count == 0 || !(frstChrtSer.DataSeries.XValues[0] is DateTime)) return;

                foreach (var datum in frstChrtSer.DataSeries.XValues)
                {
                    horizLine.Append((DateTime)datum, currentYValue);
                }

                using (var ssp = ParentSurface.SuspendUpdates())
                {
                    ParentSurface.SeriesSource.Add(new ChartSeriesViewModel(horizLine, new FastLineRenderableSeries()));
                    ParentSurface.ResumeUpdates(ssp);
                }
            }
        }
    }
}
