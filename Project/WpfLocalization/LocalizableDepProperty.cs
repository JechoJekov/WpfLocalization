using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace WpfLocalization
{
    /// <summary>
    /// Represents a dependency property that can be localized.
    /// </summary>
    class LocalizableDepProperty : LocalizableProperty
    {
        /// <summary>
        /// Gets the type of the property's value.
        /// </summary>
        public override Type PropertyType => Property.PropertyType;

        /// <summary>
        /// The object that represents the property (e.g. <see cref="DependencyProperty"/> or <see cref="PropertyInfo"/>).
        /// </summary>
        protected internal override object PropertyObject => Property;

        /// <summary>
        /// The property.
        /// </summary>
        public DependencyProperty Property { get; }

        /// <summary>
        /// Gets the default value of the property.
        /// </summary>
        public override object DefaultValue => Property.DefaultMetadata.DefaultValue;

        public LocalizableDepProperty(DependencyProperty property)
        {
            this.Property = property ?? throw new ArgumentNullException(nameof(property));
        }

        internal protected override void SetValue(DependencyObject obj, object value)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            obj.SetValue(Property, value);
        }

        public override bool Equals(object obj)
        {
            return obj is LocalizableDepProperty other && other.Property == this.Property;
        }

        public override int GetHashCode()
        {
            return Property.GetHashCode();
        }
    }
}
