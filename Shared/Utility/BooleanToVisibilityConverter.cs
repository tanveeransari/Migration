using System;
using System.Windows.Data;

namespace Shared.Utility
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        private const string InvertionFlag = "INVERSE";

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var stringParam = parameter as string;
            var inverse = string.Equals(stringParam, InvertionFlag, StringComparison.InvariantCultureIgnoreCase);

            var onTrue = inverse ? Shared.Utility.Visibility.Collapsed : Shared.Utility.Visibility.Visible;
            var onFalse = inverse ? Shared.Utility.Visibility.Visible : Shared.Utility.Visibility.Collapsed;

            return (bool)value ? onTrue : onFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}