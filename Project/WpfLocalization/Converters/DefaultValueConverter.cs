using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfLocalization.Converters
{
    /// <summary>
    /// The default value converter used by the extension.
    /// </summary>
    class DefaultValueConverter : IValueConverter
    {
        public static DefaultValueConverter Instance { get; } = new DefaultValueConverter();

        static DefaultValueConverter()
        {
            // Register converters

            if (false == TypeDescriptor.GetAttributes(typeof(ImageSource)).OfType<TypeConverterAttribute>().Where(x => x.ConverterTypeName != typeof(ImageSourceConverter).AssemblyQualifiedName).Any())
            {
                TypeDescriptor.AddAttributes(typeof(ImageSource), new TypeConverterAttribute(typeof(EnhancedImageSourceConverter)));
            }
            if (false == TypeDescriptor.GetAttributes(typeof(BitmapSource)).OfType<TypeConverterAttribute>().Where(x => x.ConverterTypeName != typeof(ImageSourceConverter).AssemblyQualifiedName).Any())
            {
                TypeDescriptor.AddAttributes(typeof(BitmapSource), new TypeConverterAttribute(typeof(BitmapSourceConverter)));
            }
        }

        DefaultValueConverter() { }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TypeDescriptor.GetConverter(targetType).ConvertFrom(null, culture, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
