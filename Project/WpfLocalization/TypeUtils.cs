using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfLocalization
{
    /// <summary>
    /// Provides various methods to work with types.
    /// </summary>
    static class TypeUtils
    {
        /// <summary>
        /// Returns a default value for the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetDefaultValue(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.IsClass ? null : Activator.CreateInstance(type);
        }

        /// <summary>
        /// Returns <paramref name="value"/> if it can be assigned to a variable of the specified <see cref="Type"/>;
        /// otherwise, returns the default value for the type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object GetValueOrDefaultValue(Type type, object value)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (value == null)
            {
                return value;
            }

            return type.IsAssignableFrom(value.GetType()) ? value : GetDefaultValue(type);
        }
    }
}
