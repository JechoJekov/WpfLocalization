using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfLocalization.Converters
{
    /// <summary>
    /// Converts <see cref="Bitmap"/> and <see cref="Icon"/> to <see cref="ImageSource"/> and falls back to the built-in <see cref="ImageSourceConverter"/>
    /// for other conversions.
    /// </summary>
    public class EnhancedImageSourceConverter : ImageSourceConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return base.CanConvertFrom(context, sourceType) || sourceType == typeof(Bitmap) || sourceType == typeof(Icon);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            ImageSource result;

            if (value is Bitmap bitmap)
            {
                result = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            else if (value is Icon icon)
            {
                using (var obj = icon.ToBitmap())
                {
                    result = Imaging.CreateBitmapSourceFromHBitmap(obj.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
            }
            else
            {
                return base.ConvertFrom(context, culture, value);
            }

            result.Freeze();
            return result;
        }
    }
}
