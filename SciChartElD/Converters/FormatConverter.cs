using Abt.Controls.SciChart.Visuals.Axes;
using Shared.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SciChartElD.Converters
{
    public class FormatConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (values[0] is ILabelProvider)
                {
                    var formatter = (CustomLabelFormatter)values[0];
                    return formatter.FormatLabel((double)values[1]);
                }
                else if (values[1] is ILabelProvider)
                {
                    var formatter = (CustomLabelFormatter)values[1];
                    return formatter.FormatLabel((double)values[0]);
                }
            }
            catch (Exception)
            {

            }

            return "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
            object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}
