using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace WpfLocalization.Converters
{
    /// <summary>
    /// Makes it possible to use a <see cref="IValueConverter"/> with <see cref="MultiBinding"/>.
    /// </summary>
    class MultiToSingleValueConverter : IMultiValueConverter
    {
        IValueConverter _valueConverter;

        public MultiToSingleValueConverter(IValueConverter valueConverter)
        {
            this._valueConverter = valueConverter ?? throw new ArgumentNullException(nameof(valueConverter));
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0)
            {
                return TypeUtils.GetDefaultValue(targetType);
            }
            else
            {
                return _valueConverter.Convert(values[0], targetType, parameter, culture);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
