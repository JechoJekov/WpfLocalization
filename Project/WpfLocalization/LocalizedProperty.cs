using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace WpfLocalization
{
    /// <summary>
    /// Represents a localized property.
    /// </summary>
    /// <remarks>
    /// CAUTION Descendants of this class *must* override <see cref="Object.Equals(object)"/> and <see cref="Object.GetHashCode"/>
    /// for instances of descendant type to operate correctly.
    /// </remarks>
    public abstract class LocalizedProperty
    {
        /// <summary>
        /// Gets the type of the property's value.
        /// </summary>
        public abstract Type PropertyType { get; }

        /// <summary>
        /// Gets the default value of the property.
        /// </summary>
        public abstract object DefaultValue { get; }

        protected LocalizedProperty() { }

        /// <summary>
        /// Sets the value of the property.
        /// </summary>
        /// <param name="targetObject">The owner of the property.</param>
        /// <param name="value"></param>
        internal protected abstract void SetValue(DependencyObject targetObject, object value);
    }
}
