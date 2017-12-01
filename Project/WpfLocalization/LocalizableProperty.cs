using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace WpfLocalization
{
    /// <summary>
    /// Represents a property that can be localized.
    /// </summary>
    /// <remarks>
    /// CAUTION Descendants of this class *must* override <see cref="Object.Equals(object)"/> and <see cref="Object.GetHashCode"/>
    /// for instances of descendant type to operate correctly.
    /// </remarks>
    public abstract class LocalizableProperty
    {
        /// <summary>
        /// Gets the type of the property's value.
        /// </summary>
        public abstract Type PropertyType { get; }

        /// <summary>
        /// The object that represents the property (e.g. <see cref="DependencyProperty"/> or <see cref="PropertyInfo"/>).
        /// </summary>
        internal protected abstract object PropertyObject { get; }

        /// <summary>
        /// Gets the default value of the property.
        /// </summary>
        public abstract object DefaultValue { get; }

        protected LocalizableProperty() { }

        /// <summary>
        /// Sets the value of the property.
        /// </summary>
        /// <param name="targetObject">The owner of the property.</param>
        /// <param name="value"></param>
        internal protected abstract void SetValue(DependencyObject targetObject, object value);
    }
}
