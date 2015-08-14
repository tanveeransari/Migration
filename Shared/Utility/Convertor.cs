using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Shared.Utility
{
    public class ColorToBrushConverter : IValueConverter
    {
        private static readonly IDictionary<Color, SolidColorBrush> _cachedBrushes = new Dictionary<Color, SolidColorBrush>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Color color = (Color)value;
            string sValue = (string)value;
            Color color = (Color)ColorConverter.ConvertFromString(sValue);

            //BrushConverter m = new BrushConverter();
            
            SolidColorBrush brush;

            if (_cachedBrushes.TryGetValue(color, out brush))
            {
                return brush;
            }

            brush = new SolidColorBrush(color);
            if(_cachedBrushes.ContainsKey(color) == false)
                _cachedBrushes.Add(color, brush);

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}