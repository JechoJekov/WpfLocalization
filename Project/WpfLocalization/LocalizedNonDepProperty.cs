using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace WpfLocalization
{
    /// <summary>
    /// Represents a non-localized dependency property.
    /// </summary>
    class LocalizedNonDepProperty : LocalizedProperty
    {
        /// <summary>
        /// Gets the type of the property's value.
        /// </summary>
        public override Type PropertyType => Property.PropertyType;

        /// <summary>
        /// The property.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Gets the default value of the property.
        /// </summary>
        public override object DefaultValue => TypeUtils.GetDefaultValue(Property.PropertyType);

        public LocalizedNonDepProperty(PropertyInfo property)
        {
            this.Property = property ?? throw new ArgumentNullException(nameof(property));
        }

        internal protected override void SetValue(DependencyObject dependencyObject, object value)
        {
            Property.SetValue(dependencyObject, value, null);
        }

        public override bool Equals(object obj)
        {
            return obj is LocalizedNonDepProperty other && other.Property == this.Property;
        }

        public override int GetHashCode()
        {
            return Property.GetHashCode();
        }
    }
}
