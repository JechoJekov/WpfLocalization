using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace WpfLocalization.Converters
{
    class CallbackValueConverter : IValueConverter
    {
        /// <summary>
        /// The current UI culture.
        /// </summary>
        /// <remarks>
        /// This property is set each time before the <see cref="Convert"/> method is invoked.
        /// </remarks>
        internal CultureInfo UICulture { get; set; }

        LocalizedValueBase _localizedValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localizedValue"></param>
        /// <remarks>
        /// While it is good design to avoid dependencies as in the implementation at the bottom of this file
        /// it requires more memory than referencing a <see cref="LocalizedValue"/> directly.
        /// </remarks>
        public CallbackValueConverter(LocalizedValueBase localizedValue)
        {
            this._localizedValue = localizedValue ?? throw new ArgumentNullException(nameof(localizedValue));

            if (localizedValue.Callback == null)
            {
                throw new ArgumentNullException(nameof(LocalizedValue.Callback));
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (_localizedValue.Converter != null)
            {
                value = _localizedValue.Converter.Convert(value, targetType, _localizedValue.ConverterParameter, culture);
            }

            return _localizedValue.Callback(culture, UICulture, parameter, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

#if GOOD_DESIGN_MORE_MEMORY

    class CallbackValueConverter : IValueConverter
    {
        /// <summary>
        /// The current UI culture.
        /// </summary>
        /// <remarks>
        /// This property is set each time before the <see cref="Convert"/> method is invoked.
        /// </remarks>
        internal CultureInfo UICulture { get; set; }

        LocalizationCallback _callback;
        IValueConverter _converter;
        object _converterParameter;

        public CallbackValueConverter(LocalizationCallback callback, IValueConverter converter, object converterParameter)
        {
            this._callback = callback ?? throw new ArgumentNullException(nameof(callback));
            this._converter = converter;
            this._converterParameter = converterParameter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (_converter != null)
            {
                value = _converter.Convert(value, targetType, _converterParameter, culture);
            }

            return _callback(culture, UICulture, parameter, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

#endif
}
