using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Markup;

namespace WpfLocalization
{
    /// <summary>
    /// Returns the <see cref="CultureInfo"/> that corresponds to the specified name.
    /// </summary>
    [MarkupExtensionReturnType(typeof(CultureInfo))]
    public class CultureInfoExtension : MarkupExtension
    {
        public string Name { get; set; }

        CultureInfo _cultureInfo;

        public CultureInfoExtension() { }

        public CultureInfoExtension(string name)
        {
            this.Name = name;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Name))
            {
                return null;
            }

            return _cultureInfo ?? (_cultureInfo = CultureInfo.GetCultureInfo(Name));
        }
    }
}
