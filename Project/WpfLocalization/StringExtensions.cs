using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfLocalization
{
    /// <summary>
    /// Provides extension methods for <see cref="String"/>s.
    /// </summary>
    static class StringExtensions
    {
        public static string NullIfEmpty(this string value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }
    }
}
