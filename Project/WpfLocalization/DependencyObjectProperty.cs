using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace WpfLocalization
{
    /// <summary>
    /// Contains information about a <see cref="DependencyObject"/> and one of its properties.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct DependencyObjectProperty : IProvideValueTarget, IServiceProvider
    {
        /// <summary>
        /// The owner of the property.
        /// </summary>
        public DependencyObject TargetObject { get; }

        /// <summary>
        /// The property.
        /// </summary>
        public LocalizableProperty TargetProperty { get; }

        public DependencyObjectProperty(DependencyObject targetObject, LocalizableProperty targetProperty)
        {
            this.TargetObject = targetObject ?? throw new ArgumentNullException(nameof(targetObject));
            this.TargetProperty = targetProperty ?? throw new ArgumentNullException(nameof(targetProperty));
        }

        object IProvideValueTarget.TargetObject => TargetObject;

        object IProvideValueTarget.TargetProperty => TargetProperty.PropertyObject;

        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(IProvideValueTarget))
            {
                return this;
            }
            else
            {
                return null;
            }
        }

#if DEPRECATED
        public static bool operator ==(DependencyObjectProperty a, DependencyObjectProperty b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(DependencyObjectProperty a, DependencyObjectProperty b)
        {
            return false == a.Equals(b);
        }

        public override int GetHashCode()
        {
            return TargetObject.GetHashCode() ^ TargetProperty.PropertyObject.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is DependencyObjectProperty other 
                && other.TargetObject == this.TargetObject 
                && other.TargetProperty.PropertyObject == this.TargetProperty.PropertyObject;
        }
#endif
    }
}
