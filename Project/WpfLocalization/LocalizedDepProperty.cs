using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace WpfLocalization
{
    /// <summary>
    /// Represents localized dependency property.
    /// </summary>
    class LocalizedDepProperty : LocalizedProperty
    {
        /// <summary>
        /// Gets the type of the property's value.
        /// </summary>
        public override Type PropertyType => Property.PropertyType;

        /// <summary>
        /// The property.
        /// </summary>
        public DependencyProperty Property { get; }

        public LocalizedDepProperty(DependencyProperty property)
        {
            this.Property = property ?? throw new ArgumentNullException(nameof(property));
        }

        internal protected override void SetValue(DependencyObject dependencyObject, object value)
        {
            dependencyObject.SetValue(Property, value);
        }

        public override bool Equals(object obj)
        {
            return obj is LocalizedDepProperty other && other.Property == this.Property;
        }

        public override int GetHashCode()
        {
            return Property.GetHashCode();
        }
    }
}
